using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public Vector3 Size;
    public Vector3 Position;
    public Room[] AdjacentRooms;
    public List<Vector3Int> Walls = new();

    public Room(Vector3Int size, Vector3Int position) {
        Size = size;
        Position = new Vector3(
            position.x + (float)size.x / 2,
            0,
            position.z + (float)size.z / 2
        );
    }
}