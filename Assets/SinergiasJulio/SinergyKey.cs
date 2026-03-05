using System;
using UnityEngine;

[Serializable]
public readonly struct SynergyKey : IEquatable<SynergyKey>
{
    public readonly int a;
    public readonly int b;

    public SynergyKey(ItemBase i1, ItemBase i2)
    {
        int id1 = i1 ? i1.GetInstanceID() : 0;
        int id2 = i2 ? i2.GetInstanceID() : 0;

        if (id1 <= id2) { a = id1; b = id2; }
        else { a = id2; b = id1; }
    }

    public bool Equals(SynergyKey other) => a == other.a && b == other.b;
    public override bool Equals(object obj) => obj is SynergyKey other && Equals(other);
    public override int GetHashCode() => (a * 397) ^ b;
}