using System.Collections.Generic;

[System.Serializable]
public struct WFCGridCell {
    public List<Tile> possibleTiles;
    public bool collapsed;
    public int x;
    public int y;

    public WFCGridCell(int x, int y) {
        this.possibleTiles = new List<Tile>();
        this.collapsed = false;
        this.x = x;
        this.y = y;
    }
}