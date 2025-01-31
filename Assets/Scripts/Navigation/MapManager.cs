using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class MapManager : MonoBehaviour {
    public WFCGenerator wfcGenerator; // Reference to your WFC generator
    public bool allowEdgeTiles = false;

    public NavigationCell[,] navigationGrid; // Abstract grid

    private bool _navigationGridGenerated = false;

    public void Start() {
        LoadMapData();
        GenerateNavigationGrid();
        PrintNavigationGrid();
    }

    private void LoadMapData() {
        if (wfcGenerator == null || wfcGenerator.wfcData == null) {
            Debug.LogError("LoadMapData: WFC generator is null, or data is null");
            return;
        }
        string filePath = Path.Combine(wfcGenerator.saveDirectory, wfcGenerator.saveFileName);
        if (!File.Exists(filePath)) {
            Debug.LogError($"LoadMapData: Map data not found at {filePath}");
            return;
        }

        try {
            string json = File.ReadAllText(filePath);
            MapDataToSave mapData = JsonUtility.FromJson<MapDataToSave>(json);
            if (mapData == null) {
                Debug.LogError("LoadMapData: Failed to deserialize map data");
                return;
            }

            wfcGenerator.wfcData.wfcObject.gridSize = mapData.gridSize;
            wfcGenerator.wfcData.wfcObject.InitializeGrid(wfcGenerator.wfcData.wfcObject.tiles);
            foreach (CollapsedCellData collapsedCellData in mapData.collapsedGrid) {
                Tile tile = wfcGenerator.wfcData.wfcObject.tiles.Find(tile => tile.tileType == collapsedCellData.tileType);
                if (!tile.Equals(default(Tile))) {
                    SetCellToTile(collapsedCellData.x, collapsedCellData.y, tile);
                } else {
                    Debug.LogError($"LoadMapData: Failed to find tile type {collapsedCellData.tileType}");
                }
            }
            Debug.Log($"MapManager: Map data loaded from {filePath}");
        } catch (Exception e) {
            Debug.LogError($"LoadMapData: Failed to load map data: {e.Message}");
        }
    }
    private void SetCellToTile(int x, int y, Tile tile) {
        if (x < 0 || x >= wfcGenerator.wfcData.wfcObject.gridSize.x || y < 0 || y >= wfcGenerator.wfcData.wfcObject.gridSize.y) {
            Debug.LogError($"SetCellToTile: Invalid cell coordinates x:{x}, y:{y}");
            return;
        }
        WFCGridCell cell = wfcGenerator.wfcData.wfcObject.grid[x, y];
        cell.possibleTiles = new List<Tile>() { tile };
        cell.collapsed = true;
        wfcGenerator.wfcData.wfcObject.grid[x, y] = cell;
        //Debug.Log($"SetCellToTile: Set cell at x: {x}, y: {y} to tile {tile.tileType}");
    }

    public void GenerateNavigationGrid() {
        if (wfcGenerator == null || wfcGenerator.wfcData == null || wfcGenerator.wfcData.wfcObject == null || wfcGenerator.wfcData.wfcObject.grid == null) {
            Debug.LogError("MapManager: WFC Generator not set, or data is null");
            return;
        }

        Vector2Int gridSize = wfcGenerator.wfcData.wfcObject.gridSize;
        navigationGrid = new NavigationCell[gridSize.x, gridSize.y]; // Initialize abstract grid
        if (wfcGenerator.wfcData.wfcObject.grid == null) {
            Debug.LogError("GenerateNavigationGrid: Grid is null");
            return;
        }
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                WFCGridCell cell = wfcGenerator.wfcData.wfcObject.grid[x, y];
                if (cell.collapsed) {
                    if (cell.possibleTiles != null && cell.possibleTiles.Count > 0) {
                        Tile tile = cell.possibleTiles[0];
                        navigationGrid[x, y] = new NavigationCell(IsTileWalkable(tile));
                        if (navigationGrid[x, y].IsWalkable) {
                            PopulateNavigationCellConnections(x, y, tile, navigationGrid);
                        }
                    } else {
                        navigationGrid[x, y] = new NavigationCell(false); // No tile, not walkable
                        Debug.LogWarning($"MapManager: Cell at {x},{y} is collapsed but has no possible tiles, setting to non-walkable");
                    }
                } else {
                    navigationGrid[x, y] = new NavigationCell(false);
                    Debug.LogWarning($"MapManager: Cell at {x},{y} is not collapsed, setting to non-walkable");
                }
            }
        }
        _navigationGridGenerated = true;
        Debug.Log("MapManager: Navigation grid generated successfully");
    }

    public void PrintNavigationGrid() {
        if (navigationGrid == null) {
            Debug.Log("Navigation Grid is null, cannot print");
            return;
        }
        string output = "";
        for (int y = navigationGrid.GetLength(1) - 1; y >= 0; y--) {
            for (int x = 0; x < navigationGrid.GetLength(0); x++) {
                output += navigationGrid[x, y].IsWalkable ? "1" : "0";
            }
            output += "\n";

        }
        Debug.Log(output);
    }

    private void PopulateNavigationCellConnections(int x, int y, Tile currentTile, NavigationCell[,] grid) {
        CheckAndSetPassableConnection(x, y, currentTile, x, y + 1, Direction.Up, grid);
        CheckAndSetPassableConnection(x, y, currentTile, x, y - 1, Direction.Down, grid);
        CheckAndSetPassableConnection(x, y, currentTile, x - 1, y, Direction.Left, grid);
        CheckAndSetPassableConnection(x, y, currentTile, x + 1, y, Direction.Right, grid);
    }

    private void CheckAndSetPassableConnection(int currentX, int currentY, Tile currentTile, int neighborX, int neighborY, Direction direction, NavigationCell[,] grid) {
        if (neighborX < 0 || neighborX >= grid.GetLength(0) || neighborY < 0 || neighborY >= grid.GetLength(1)) {
            return; // Neighbor is out of bounds
        }
        if (wfcGenerator.wfcData.wfcObject.grid == null) return;
        WFCGridCell neighborCell = wfcGenerator.wfcData.wfcObject.grid[neighborX, neighborY];
        if (neighborCell.collapsed && neighborCell.possibleTiles.Count > 0) {
            if (IsEdgePassable(currentTile, neighborCell.possibleTiles[0], direction)) {
                grid[currentX, currentY].passableConnections[direction] = true;
            }
        }
    }

    private bool IsEdgePassable(Tile currentTile, Tile neighborTile, Direction direction) {
        switch (direction) {
            case Direction.Up:
                return IsUpEdgePassable(currentTile);
            case Direction.Down:
                return IsDownEdgePassable(currentTile);
            case Direction.Left:
                return IsLeftEdgePassable(currentTile);
            case Direction.Right:
                return IsRightEdgePassable(currentTile);
            default:
                return false;
        }
    }
    private bool IsUpEdgePassable(Tile currentTile) {
        return !(currentTile.upConnections != null && currentTile.upConnections.Contains(TileType.Water));
    }

    private bool IsDownEdgePassable(Tile currentTile) {
        return !(currentTile.downConnections != null && currentTile.downConnections.Contains(TileType.Water));
    }


    private bool IsLeftEdgePassable(Tile currentTile) {
        return !(currentTile.leftConnections != null && currentTile.leftConnections.Contains(TileType.Water));
    }

    private bool IsRightEdgePassable(Tile currentTile) {
        return !(currentTile.rightConnections != null && currentTile.rightConnections.Contains(TileType.Water));
    }

    private bool HasValidConnection(Tile currentTile, Tile neighborTile, Direction direction) {
        switch (direction) {
            case Direction.Up:
                if (currentTile.upConnections != null && currentTile.upConnections.Contains(neighborTile.tileType)
                && neighborTile.downConnections != null && neighborTile.downConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Down:
                if (currentTile.downConnections != null && currentTile.downConnections.Contains(neighborTile.tileType)
                    && neighborTile.upConnections != null && neighborTile.upConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Left:
                if (currentTile.leftConnections != null && currentTile.leftConnections.Contains(neighborTile.tileType)
                  && neighborTile.rightConnections != null && neighborTile.rightConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
            case Direction.Right:
                if (currentTile.rightConnections != null && currentTile.rightConnections.Contains(neighborTile.tileType)
                   && neighborTile.leftConnections != null && neighborTile.leftConnections.Contains(currentTile.tileType)) {
                    return true;
                }
                break;
        }
        return false;
    }

    private bool IsTileWalkable(Tile tile) {
        if (tile.tileType == TileType.Water) {
            return false;
        }
        if (!allowEdgeTiles) {
            return tile.tileType != TileType.LeftLand &&
                   tile.tileType != TileType.LowerLand &&
                   tile.tileType != TileType.UpperLand &&
                   tile.tileType != TileType.RightLand;
        }
        return true;

    }

    // Conversion Methods
    public Vector3 GridToWorldPosition(int x, int y) {
        if (wfcGenerator == null || wfcGenerator.wfcData == null || wfcGenerator.wfcData.wfcObject == null) {
            Debug.LogError("GridToWorldPosition: WFC generator is null.");
            return Vector3.zero;
        }
        // Assuming your tiles are 1x1 and the offset is handled by WFCGenerator
        return new Vector3(x, y, 0) + wfcGenerator.offset; // Consider cell center
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition) {
        if (wfcGenerator == null || wfcGenerator.wfcData == null || wfcGenerator.wfcData.wfcObject == null) {
            Debug.LogError("WorldToGridPosition: WFC generator is null.");
            return Vector2Int.zero;
        }
        Vector3 posWithoutOffset = worldPosition - wfcGenerator.offset;
        return new Vector2Int(Mathf.RoundToInt(posWithoutOffset.x), Mathf.RoundToInt(posWithoutOffset.y));
    }

    [System.Serializable]
    private class MapDataToSave {
        public Vector2Int gridSize;
        public List<CollapsedCellData> collapsedGrid;
    }

    [System.Serializable]
    private class CollapsedCellData {
        public int x;
        public int y;
        public TileType tileType;
    }
    public bool NavigationGridGenerated => _navigationGridGenerated;
}