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
        await GenerateConnections();
    }

    async Task GenerateRooms() {
        for (int i = 0; i < mapSize.x * mapSize.y; i++) {
            int m  = (float)i % 2 == 0 ? 1 : -1;
            int zi = Mathf.RoundToInt(Mathf.Floor((i / mapSize.x)));
            int xi = i - zi * mapSize.x;

            if (grid[xi, zi]) {
                continue;
            }
            
            Vector3Int size = FindSize(new Vector3Int(xi, 0, zi));
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.z; y++) {
                    grid[xi + x, zi + y] = true;
                }
            }

            rooms.Add(new Room(size, new Vector3Int(xi, 0, zi)));

            await Task.Delay(100);
        }
    }

    async Task GenerateConnections() {
        
    }

    Vector3Int FindSize(Vector3Int position, bool found = false) {
        Vector3Int size = Vector3Int.zero;
        while (!found) {
            found = true;
            size = new Vector3Int(
                Random.Range(1, maxRoomSize + 1),
                0,
                Random.Range(1, maxRoomSize + 1)
            );
            if (position.x + size.x >= mapSize.x || position.z + size.z >= mapSize.y) {
                found = false;
                continue;
            } 
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.z; z++) {
                    if (grid[position.x + x, position.z + z]) {
                        found = false;
                        break;
                    }
                }
            }
        }
        return size;
    }

    Room[] FindAdjacentRooms(Room targetRoom) {
        foreach (Room room in rooms) {
            if (room == targetRoom) {
                continue;
            }
        }
        return null;
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