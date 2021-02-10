using System;
using UnityEngine;

public static class VectorExtensions {
    public static Vector2 XZ(this Vector3 src) {
        return new Vector2(src.x, src.z);
    }
}
