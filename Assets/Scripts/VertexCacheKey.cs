using UnityEngine;
using System.Collections.Generic;

public struct VertexCacheKey : System.IEquatable<VertexCacheKey> 
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    public VertexCacheKey(Vector3 position, float precision = 0.01f) {

        X = Mathf.RoundToInt(position.x / precision);
        Y = Mathf.RoundToInt(position.y / precision);
        Z = Mathf.RoundToInt(position.z / precision);
    }
    public override int GetHashCode() {
        // TODO - do we need a better hash function?
        unchecked {
            int hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Z;
            return hash;
        }
    }

    public bool Equals(VertexCacheKey other) {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj) {
        return obj is VertexCacheKey other && Equals(other);
    }
}
