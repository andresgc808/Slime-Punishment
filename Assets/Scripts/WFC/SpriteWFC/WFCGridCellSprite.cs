using System.Collections.Generic;

[System.Serializable]
public struct WFCGridCellSprite {
    public List<TileSprite> possibleTiles;
    public bool collapsed;
    public int x;
    public int y;

    public WFCGridCellSprite(int x, int y) {
        this.possibleTiles = new List<TileSprite>();
        this.collapsed = false;
        this.x = x;
        this.y = y;
    }
}