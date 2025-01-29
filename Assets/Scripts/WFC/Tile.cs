using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Tile {
    public TileType tileType;
    public List<GameObject> possiblePrefabs;
    public List<TileType> upConnections;
    public List<TileType> downConnections;
    public List<TileType> leftConnections;
    public List<TileType> rightConnections;

    public override string ToString() {
        return tileType.ToString();
    }
}