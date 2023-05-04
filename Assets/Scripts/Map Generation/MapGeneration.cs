using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MapGeneration : MonoBehaviour {
    [SerializeField] Vector2Int mapSize;
    [SerializeField] int maxRoomSize;
    
    bool[,] grid;

    List<Room> rooms = new();

    connection[] connections;

    void Start() {
        grid = new bool[mapSize.x, mapSize.y];
        Generate();
    }

    async void Generate() {
        await GenerateRooms();
    }

    async Task GenerateRooms() {
        for (int i = 0; i < mapSize.x * mapSize.y; i++) {
            int m  = (float)i % 2 == 0 ? 1 : -1;
            int zi = Mathf.RoundToInt(Mathf.Floor((i / mapSize.x)));
            int xi = i - zi * mapSize.x;

            if (grid[xi, zi]) {
                continue;
            }

            Vector3Int size = FindRoomSize(new Vector3Int(xi, 0, zi));
            while (size == Vector3Int.zero) {
                size = FindRoomSize(new Vector3Int(xi, 0, zi));
            }

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.z; y++) {
                    grid[xi + x, zi + y] = true;
                }
            }

            rooms.Add(new Room(size, new Vector3Int(xi, 0, zi)));

            await Task.Yield();
        }
    }

    async Task GenerateConnections() {
        foreach (Room room in rooms) {

        }
    }

    Vector3Int FindRoomSize(Vector3Int position) {
        Vector3Int size = new Vector3Int(
            Random.Range(1, maxRoomSize + 1),
            0,
            Random.Range(1, maxRoomSize + 1)
        );
        for (int x = 0; x < size.x; x++) {
            for (int z = 0; z < size.z; z++) {
                if (position.x + x >= mapSize.x - 2 || position.z + z >= mapSize.y - 2) {
                    return Vector3Int.zero;
                }
                else if (grid[position.x + x, position.z + z]) {
                    return Vector3Int.zero;
                }
            }
        }
        return size;
    }

    Room[] FindAdjacentRooms(Room targetRoom) {
        List<Room> adjacents = new();

        List<Vector3> checkPosition = new();

        for (int x = 0; x < targetRoom.Size.x + 2; x++) {
            for (int z = 0; z < targetRoom.Size.z + 2; z++) {

            }
        }

        foreach (Room room in rooms) {
            if (room == targetRoom) {
                continue;
            }


        }
        return adjacents.ToArray();
    }

    struct connection {
        public Room RoomA;
        public Room RoomB;

        public connection(Room roomA, Room roomB) {
            RoomA = roomA;
            RoomB = roomB;
        }
    }

    void OnDrawGizmos() {
        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                if (grid[x, y]) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(x + .5f, 0, y + .5f), new Vector3(1, 0, 1));
                }
                else {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(x + .5f, 0, y + .5f), new Vector3(1, 0, 1));
                }
            }
        }
        
        Gizmos.color = Color.white;
        foreach (Room room in rooms) {
            Gizmos.DrawWireCube(room.Position, room.Size);
        }
    }
}