using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class WFCGenerator : MonoBehaviour {
    public WFCData wfcData;
    public Transform spriteParent;
    public Vector3 offset;
    public bool autoRun = false;

    [ContextMenu("Generate")]
    public void Generate() {
        if (spriteParent != null) {
            foreach (Transform child in spriteParent) {
                Destroy(child.gameObject);
            }
        }
        StartGeneration();
    }

    private void StartGeneration() {
        Debug.Log("StartGeneration: Starting WFC process");
        if (wfcData.wfcObject.tiles.Count == 0) {
            Debug.LogError("StartGeneration: No tiles in wfcData!");
            return;
        }
        if (wfcData.wfcObject.gridSize.x == 0 || wfcData.wfcObject.gridSize.y == 0) {
            Debug.LogError("StartGeneration: WFC Grid size cannot be zero");
            return;
        }

        int maxRetries = 10; // You can adjust this
        int currentRetry = 0;

        bool finished = false;
        while (!finished && currentRetry < maxRetries) {
            Debug.Log($"StartGeneration: Starting WFC process, Attempt: {currentRetry + 1}");
            // Explicitly initialize the grid
            wfcData.wfcObject.InitializeGrid();

            while (!finished) {
                Debug.Log("StartGeneration: Calling Collapse");
                if (Collapse(out WFCGridCell cell)) {
                    Debug.Log("StartGeneration: Collapse successful");
                    Propagate(cell);
                    if (CheckIfDone()) {
                        finished = true;
                    }
                } else {
                    Debug.LogError($"StartGeneration: Cannot Collapse Map, retrying ({currentRetry + 1}/{maxRetries})");
                    ResetGrid();
                    break;
                }
            }

            if (!finished) {
                currentRetry++;
            }

        }


        if (finished) {
            VisualizeMap();
            Debug.Log("StartGeneration: WFC process finished");
        } else {
            Debug.LogError("StartGeneration: WFC process failed after multiple retries");
            // Optionally handle complete failure more explicitly
        }

    }

    private bool CheckIfDone() {
        Debug.Log("CheckIfDone: Checking if map is fully collapsed");
        for (int x = 0; x < wfcData.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcData.wfcObject.gridSize.y; y++) {
                if (!wfcData.wfcObject.grid[x, y].collapsed) {
                    Debug.Log($"CheckIfDone: Cell not collapsed at x:{x} y:{y}");
                    return false;
                }
            }
        }

        Debug.Log("CheckIfDone: Map is fully collapsed");
        return true;
    }

    private void ResetGrid() {
        Debug.Log("ResetGrid: Resetting grid");
        for (int x = 0; x < wfcData.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcData.wfcObject.gridSize.y; y++) {
                wfcData.wfcObject.grid[x, y].possibleTiles = new List<Tile>(wfcData.wfcObject.tiles);
                wfcData.wfcObject.grid[x, y].collapsed = false;
            }
        }
        Debug.Log("ResetGrid: Grid has been reset");
    }

    private bool Collapse(out WFCGridCell lowestEntropyCell) {
        lowestEntropyCell = new WFCGridCell();
        List<WFCGridCell> possibleCells = new List<WFCGridCell>();

        Debug.Log("Collapse: Starting collapse process");
        for (int x = 0; x < wfcData.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcData.wfcObject.gridSize.y; y++) {
                WFCGridCell cell = wfcData.wfcObject.grid[x, y];
                if (!cell.collapsed) {
                    possibleCells.Add(cell);
                    Debug.Log($"Collapse: Adding cell to possible cells, x:{x} y:{y}, collapsed: {cell.collapsed}");
                }

            }
        }

        Debug.Log($"Collapse: possibleCells count = {possibleCells.Count}");

        if (possibleCells.Count == 0) {
            Debug.Log("Collapse: No possible cells, returning false");
            return false;
        }

        possibleCells = possibleCells.OrderBy(cell => cell.possibleTiles.Count).ToList();

        if (possibleCells.Count == 0) {
            Debug.Log("Collapse: No possible cells after ordering, returning false");
            return false;
        }
        lowestEntropyCell = possibleCells[0];

        if (lowestEntropyCell.possibleTiles == null) {
            Debug.LogError("Collapse: possibleTiles is null!");
            return false;
        }

        if (lowestEntropyCell.possibleTiles.Count == 0) {
            Debug.Log("Collapse: Lowest entropy cell has no possible tiles. returning false");
            return false;
        }


        int index = Random.Range(0, lowestEntropyCell.possibleTiles.Count);
        Tile chosenTile = lowestEntropyCell.possibleTiles[index];
        lowestEntropyCell.possibleTiles = new List<Tile>() { chosenTile };
        lowestEntropyCell.collapsed = true;
        wfcData.wfcObject.grid[lowestEntropyCell.x, lowestEntropyCell.y] = lowestEntropyCell;

        Debug.Log($"Collapse: Collapsed cell at x: {lowestEntropyCell.x} y: {lowestEntropyCell.y}, with tile {chosenTile}");
        return true;
    }

    private void Propagate(WFCGridCell collapsedCell) {
        Debug.Log($"Propagate: Starting propagation for cell at x: {collapsedCell.x} y: {collapsedCell.y}");
        int x = collapsedCell.x;
        int y = collapsedCell.y;
        Tile collapsedTile = collapsedCell.possibleTiles[0];
        Debug.Log($"Propagate: Collapsed tile is {collapsedTile}");

        PropagateToCell(x, y + 1, collapsedTile, "up");
        PropagateToCell(x, y - 1, collapsedTile, "down");
        PropagateToCell(x - 1, y, collapsedTile, "left");
        PropagateToCell(x + 1, y, collapsedTile, "right");
        Debug.Log("Propagate: Finished propagation");
    }

    private void PropagateToCell(int x, int y, Tile collapsedTile, string direction) {
        Debug.Log($"PropagateToCell: Starting propagate to x:{x} y:{y} from tile: {collapsedTile}, direction: {direction}");
        if (x < 0 || x >= wfcData.wfcObject.gridSize.x) {
            Debug.Log($"PropagateToCell: Invalid x:{x}, skipping");
            return;
        }

        if (y < 0 || y >= wfcData.wfcObject.gridSize.y) {
            Debug.Log($"PropagateToCell: Invalid y:{y}, skipping");
            return;
        }

        WFCGridCell cell = wfcData.wfcObject.grid[x, y];

        if (cell.collapsed) {
            Debug.Log($"PropagateToCell: Cell x:{x} y:{y} already collapsed, skipping");
            return;
        }

        Debug.Log($"PropagateToCell: Before filtering possible tiles: {string.Join(", ", cell.possibleTiles)} for cell at x:{x}, y:{y}");

        List<Tile> validTiles = new List<Tile>();



        switch (direction) {
            case "up":
                foreach (Tile tile in cell.possibleTiles) {
                    if (collapsedTile.upConnections != null && collapsedTile.upConnections.Contains(tile.name)) {
                        validTiles.Add(tile);
                        Debug.Log($"PropagateToCell: Tile {tile.name} is valid up connection");
                    } else {
                        Debug.Log($"PropagateToCell: Tile {tile.name} is not valid up connection");
                    }
                }
                break;
            case "down":
                foreach (Tile tile in cell.possibleTiles) {
                    if (collapsedTile.downConnections != null && collapsedTile.downConnections.Contains(tile.name)) {
                        validTiles.Add(tile);
                        Debug.Log($"PropagateToCell: Tile {tile.name} is valid down connection");
                    } else {
                        Debug.Log($"PropagateToCell: Tile {tile.name} is not valid down connection");
                    }
                }
                break;
            case "left":
                foreach (Tile tile in cell.possibleTiles) {
                    if (collapsedTile.leftConnections != null && collapsedTile.leftConnections.Contains(tile.name)) {
                        validTiles.Add(tile);
                        Debug.Log($"PropagateToCell: Tile {tile.name} is valid left connection");
                    } else {
                        Debug.Log($"PropagateToCell: Tile {tile.name} is not valid left connection");
                    }
                }
                break;
            case "right":
                foreach (Tile tile in cell.possibleTiles) {
                    if (collapsedTile.rightConnections != null && collapsedTile.rightConnections.Contains(tile.name)) {
                        validTiles.Add(tile);
                        Debug.Log($"PropagateToCell: Tile {tile.name} is valid right connection");
                    } else {
                        Debug.Log($"PropagateToCell: Tile {tile.name} is not valid right connection");
                    }
                }
                break;
        }


        cell.possibleTiles = validTiles;

        Debug.Log($"PropagateToCell: After filtering possible tiles: {string.Join(", ", cell.possibleTiles)} for cell at x:{x}, y:{y}");

        wfcData.wfcObject.grid[x, y] = cell;

        Debug.Log($"PropagateToCell: finished propagate to x:{x} y:{y}");
    }

    private void VisualizeMap() {
        Debug.Log("VisualizeMap: Starting map visualization");
        for (int x = 0; x < wfcData.wfcObject.gridSize.x; x++) {
            for (int y = 0; y < wfcData.wfcObject.gridSize.y; y++) {
                var cell = wfcData.wfcObject.grid[x, y];
                if (cell.possibleTiles == null || cell.possibleTiles.Count == 0) {
                    Debug.LogError($"VisualizeMap: No valid tiles in cell at x:{x} y:{y}, skipping");
                    continue; // Skip if no tile could be placed
                }

                var tile = cell.possibleTiles[0];
                if (tile.sprite != null) {
                    Debug.Log($"VisualizeMap: instantiating sprite: {tile}, at x: {x}, y: {y}");
                    GameObject spriteObject = new GameObject(tile.name);
                    SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = tile.sprite;

                    // Get the bounds of the sprite
                    var bounds = renderer.sprite.bounds;
                    // Calculate the offset to center the sprite
                    var offset = new Vector3(bounds.center.x, bounds.center.y, 0);

                    spriteObject.transform.position = new Vector3(x, y, 0) + offset + this.offset;
                    spriteObject.transform.SetParent(spriteParent, true);
                }

            }
        }
        Debug.Log("VisualizeMap: Finished map visualization");
    }
}