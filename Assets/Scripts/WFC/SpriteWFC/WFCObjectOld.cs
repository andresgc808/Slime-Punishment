using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;

[System.Serializable]
public class WFCObjectOld {
    public List<TileSprite> tiles;
    public WFCGridCellSprite[,] grid;
    public Vector2Int gridSize;

    public void InitializeGrid(List<TileSprite> tiles) {
        Debug.Log("WFCObject: Initializing Grid");
        this.tiles = tiles;
        grid = new WFCGridCellSprite[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                this.grid[x, y] = new WFCGridCellSprite(x, y);
                List<TileSprite> allTiles = new List<TileSprite>();
                foreach (TileSprite tile in tiles) {
                    allTiles.Add(tile);
                }
                this.grid[x, y].possibleTiles = allTiles;
            }
        }
        Debug.Log("WFCObject: Finished Initializing Grid");
    }


     public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Grid Size: {gridSize.x} x {gridSize.y}");
        sb.AppendLine("--- Tiles ---");
        foreach (var tile in tiles)
        {
            sb.AppendLine(TileToString(tile));
        }
        sb.AppendLine("--- Grid ---");

          if (grid != null)
            {
              for (int y = gridSize.y - 1; y >= 0; y--)
                {
                    for (int x = 0; x < gridSize.x; x++)
                    {
                         sb.Append(GridCellToString(grid[x,y]));
                         sb.Append(" ");
                    }
                    sb.AppendLine();
                }
           } else {
               sb.AppendLine("Grid is null");
           }

        return sb.ToString();
    }


    private string TileToString(TileSprite tile)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"  - {tile.tileType}:");
        sb.Append("    Sprites: ");
        if (tile.possibleSprites != null) {
          sb.Append(string.Join(", ", tile.possibleSprites.Select(s => s.name)));
        } else {
          sb.Append("None");
        }
        sb.AppendLine();
        sb.Append("    Up Connections: ");
        if (tile.upConnections != null)
        {
            sb.Append(string.Join(", ", tile.upConnections));
        }
        else
        {
            sb.Append("None");
        }
         sb.AppendLine();
        sb.Append("    Down Connections: ");
         if (tile.downConnections != null)
        {
            sb.Append(string.Join(", ", tile.downConnections));
        }
        else
        {
            sb.Append("None");
        }
         sb.AppendLine();
        sb.Append("    Left Connections: ");
         if (tile.leftConnections != null)
        {
            sb.Append(string.Join(", ", tile.leftConnections));
        }
        else
        {
             sb.Append("None");
        }
         sb.AppendLine();
        sb.Append("    Right Connections: ");
         if (tile.rightConnections != null)
        {
            sb.Append(string.Join(", ", tile.rightConnections));
        }
        else
        {
            sb.Append("None");
        }
         sb.AppendLine();
        return sb.ToString();
    }

        private string GridCellToString(WFCGridCellSprite cell) {
        if (cell.collapsed) {
           if (cell.possibleTiles != null && cell.possibleTiles.Count > 0) {
             return cell.possibleTiles[0].tileType.ToString();
           } else {
              return "Error";
           }
        } else {
             return $"[{cell.possibleTiles.Count}]";
         }
    }
}