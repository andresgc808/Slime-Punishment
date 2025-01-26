using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WFCObject {
    public List<Tile> tiles;
    public WFCGridCell[,] grid;
    public Vector2Int gridSize;


    public void InitializeGrid(List<Tile> tiles) {
        Debug.Log("WFCObject: Initializing Grid");
        this.tiles = tiles;
        grid = new WFCGridCell[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                this.grid[x, y] = new WFCGridCell(x, y);
                List<Tile> allTiles = new List<Tile>();
                foreach (Tile tile in tiles) {
                    allTiles.Add(tile);
                }
                this.grid[x, y].possibleTiles = allTiles;
            }
        }
        Debug.Log("WFCObject: Finished Initializing Grid");
    }

}