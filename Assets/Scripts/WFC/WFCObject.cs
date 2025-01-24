using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WFCObject {
    public List<Tile> tiles;
    public WFCGridCell[,] grid;
    public Vector2Int gridSize;

    public WFCObject(List<Tile> tiles, Vector2Int gridSize) {
        this.tiles = tiles;
        this.gridSize = gridSize;
    }

    public void InitializeGrid() {
        Debug.Log("WFCObject: Initializing Grid");
        grid = new WFCGridCell[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                this.grid[x, y] = new WFCGridCell(x, y);
                this.grid[x, y].possibleTiles = new List<Tile>(tiles);
            }
        }
        Debug.Log("WFCObject: Finished Initializing Grid");
    }
}