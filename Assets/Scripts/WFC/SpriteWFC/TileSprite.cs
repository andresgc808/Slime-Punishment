using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct TileSprite {
    public TileType tileType;
    public List<Sprite> possibleSprites;
    public List<TileType> upConnections;
    public List<TileType> downConnections;
    public List<TileType> leftConnections;
    public List<TileType> rightConnections;

    public override string ToString() {
        return tileType.ToString();
    }
}