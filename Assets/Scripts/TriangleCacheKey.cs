using UnityEngine;
using System.Collections.Generic;

public struct TriangleCacheKey : System.IEquatable<TriangleCacheKey>
{
    public readonly int Depth;
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public TriangleCacheKey(Vector3 position, int depth, float precision = 0.01f)
    {
        Depth = depth;
        X = Mathf.RoundToInt(position.x / precision);
        Y = Mathf.RoundToInt(position.y / precision);
        Z = Mathf.RoundToInt(position.z / precision);
    }

    public override int GetHashCode()
    {
        // TODO - do we need a better hash function?
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Depth;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Z;
            return hash;
        }
    }

    public bool Equals(TriangleCacheKey other)
    {
        return Depth == other.Depth && X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj)
    {
        return obj is TriangleCacheKey other && Equals(other);
    }
}
