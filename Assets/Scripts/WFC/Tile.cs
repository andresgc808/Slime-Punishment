using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Tile {
    public Sprite sprite;
    public string name;
    public List<string> upConnections;
    public List<string> downConnections;
    public List<string> leftConnections;
    public List<string> rightConnections;

    public override string ToString() {
        return name;
    }
}