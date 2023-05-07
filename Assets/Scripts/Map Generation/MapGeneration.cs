using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MapGeneration : MonoBehaviour {
    [SerializeField] Vector2Int mapSize;
    [SerializeField] int maxRoomSize;
    [SerializeField] float bonusBranchChance;

    [SerializeField] GameObject wall;
    
    bool[,] grid;

    List<Room> rooms = new();

    List<connection> tempConnections = new();
    List<connection> connections = new();

    void Start() {
        grid = new bool[mapSize.x, mapSize.y];
        Generate();
    }

    async void Generate() {
        await GenerateRooms();
        await GenerateConnections();
        await FindMST();
        await AddBonusBranches();
        await GenerateWalls();
        print("Generation Done.");
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
            room.AdjacentRooms = FindAdjacentRooms(room);
            foreach (Room adjRoom in room.AdjacentRooms) {
                if (adjRoom.Position.x >= room.Position.x && adjRoom.Position.y >= room.Position.y) {
                    tempConnections.Add(new connection(room, adjRoom));
                }
            }
            await Task.Yield();
        }
    }

    async Task FindMST() {
        List<Room> notIncluded = new();
        foreach (Room r in rooms) {
            notIncluded.Add(r);
        }
        List<Room> included = new();
        Room currentRoom = rooms[Random.Range(0, rooms.Count)];
        included.Add(currentRoom);
        notIncluded.Remove(currentRoom);

        while (notIncluded.Count > 0) {
            Room nextRoom = null;
            float minWeight = Mathf.Infinity;
            foreach (Room room in included) {
                foreach (Room adjRoom in room.AdjacentRooms) {
                    if (included.Contains(adjRoom)) {
                        continue;
                    }

                    float weight = Vector3.Distance(room.Position, adjRoom.Position);
                    if (weight < minWeight) {
                        nextRoom = adjRoom;
                        minWeight = weight;
                        currentRoom = room;
                    }
                }
            }

            if (nextRoom == null) {
                continue;
            }

            connections.Add(new connection(currentRoom, nextRoom));
            
            notIncluded.Remove(nextRoom);
            included.Add(nextRoom);
            currentRoom = nextRoom;

            await Task.Yield();
        }
    }

    async Task AddBonusBranches() {
        foreach (connection tCon in tempConnections) {
            if (Random.Range(0f, 1f) < bonusBranchChance) {
                bool connectionExists = false;
                foreach (connection con in connections) {
                    if (tCon.RoomA == con.RoomA && tCon.RoomB == con.RoomB || tCon.RoomB == con.RoomA && tCon.RoomA == con.RoomB) {
                        connectionExists = true;
                        break;
                    }
                }
                if (!connectionExists) {
                    connections.Add(tCon);
                }
            }
            await Task.Yield();
        }
    }

    async Task GenerateWalls() {
        foreach (Room room in rooms) {
            for (int x1 = 0; x1 < room.Size.x; x1++) {
                Vector3 position = new Vector3(
                    room.Position.x - room.Size.x / 2 + x1,
                    0,
                    room.Position.z + room.Size.z / 2
                );
                Instantiate(wall, position, Quaternion.Euler(0, 90, 0));
            }
            for (int x2 = 0; x2 < room.Size.x; x2++) {
                Vector3 position = new Vector3(
                    room.Position.x - room.Size.x / 2 + x2,
                    0,
                    room.Position.z - room.Size.z / 2
                );
                Instantiate(wall, position, Quaternion.Euler(0, 270, 0));
            }
            for (int z1 = 0; z1 < room.Size.z; z1++) {
                Vector3 position = new Vector3(
                    room.Position.x - room.Size.x / 2,
                    0,
                    room.Position.z - room.Size.z / 2 + z1
                );
                Instantiate(wall, position, Quaternion.Euler(0, 0, 0));
            }
            for (int z2 = 0; z2 < room.Size.z; z2++) {
                Vector3 position = new Vector3(
                    room.Position.x + room.Size.x / 2,
                    0,
                    room.Position.z - room.Size.z / 2 + z2
                );
                Instantiate(wall, position, Quaternion.Euler(0, 180, 0));
            }
            await Task.Delay(100);
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
                if (position.x + x >= mapSize.x || position.z + z >= mapSize.y) {
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

        List<Vector3> checkPositions = new();

        int xMin = Mathf.FloorToInt(targetRoom.Position.x - targetRoom.Size.x / 2) - 1;
        int xMax = Mathf.CeilToInt(targetRoom.Position.x + targetRoom.Size.x / 2);

        int zMin = Mathf.FloorToInt(targetRoom.Position.z - targetRoom.Size.z / 2) - 1;
        int zMax = Mathf.CeilToInt(targetRoom.Position.z + targetRoom.Size.z / 2);

        for (int x = xMin; x <= xMax; x++) {
            for (int z = zMin; z <= zMax; z++) {
                if (x == xMax && z == zMax) {
                    continue;
                }
                checkPositions.Add(new Vector3(x, 0, z));
            }
        }
        
        foreach (Room room in rooms) {
            if (room == targetRoom) {
                continue;
            }
            for (int x = Mathf.FloorToInt(room.Position.x - room.Size.x / 2); x < Mathf.CeilToInt(room.Position.x + room.Size.x / 2); x++) {
                for (int z = Mathf.FloorToInt(room.Position.z - room.Size.z / 2); z < Mathf.CeilToInt(room.Position.z + room.Size.z / 2); z++) {
                    if (adjacents.Contains(room)) {
                        break;
                    }
                    Vector3 position = new Vector3(x, 0, z);
                    if (checkPositions.Contains(position)) {
                        adjacents.Add(room);
                        break;
                    }
                }
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
        foreach (Room room in rooms) {
            float grayNess = Random.Range(0f, 1f);
            Gizmos.color = Color.white;
            Gizmos.DrawCube(room.Position, room.Size);

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(room.Position, room.Size);
        }

        Gizmos.color = new Color32(43, 84, 78, 255);
        foreach (connection con in tempConnections) {
            Gizmos.DrawLine(con.RoomA.Position, con.RoomB.Position);
        }

        Gizmos.color = new Color32(12, 247, 213, 255);
        foreach (connection con in connections) {
            Gizmos.DrawLine(con.RoomA.Position, con.RoomB.Position);
        }
    }
}