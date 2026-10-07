using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCoordinate
{
    public int x;
    public int y;



    public GridCoordinate(int p1, int p2)  {
        x = p1;
        y = p2;
    }



    // This allows us to do explicit type conversions between the GridCoordinate type and other Vector types
    // Implicit type conversions - (float)gridCoordinate.x
    // Explicit type conversions - Vector2 v = new GridCoordinate(1, 23);
    // We want to convert to Vector2 from GridCoordinate
    public static explicit operator Vector2(GridCoordinate gridCoordinate) {
        return new Vector2((float)gridCoordinate.x, (float)gridCoordinate.y);
    }

    public static explicit operator Vector2Int(GridCoordinate gridCoordinate) {
        return new Vector2Int(gridCoordinate.x, gridCoordinate.y);
    }

    public static explicit operator Vector3(GridCoordinate gridCoordinate) {
        return new Vector3((float)gridCoordinate.x, (float)gridCoordinate.y, 0f);
    }

    public static explicit operator Vector3Int(GridCoordinate gridCoordinate) {
        return new Vector3Int(gridCoordinate.x, gridCoordinate.y, 0);
    }
}
