using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MapGeneration : MonoBehaviour {
    [SerializeField] Vector2Int mapSize;
    [SerializeField] int maxRoomSize;
    [SerializeField] float bonusBranchChance;

    [SerializeField] int debugIndex;

    [SerializeField] GameObject wall;
    [SerializeField] GameObject door;
    [SerializeField] GameObject floor;
    
    bool[,] grid;

    List<Room> rooms = new();

    List<connection> tempConnections = new();
    List<connection> connections = new();

    public delegate void onFinished();
    public onFinished OnFinished;

    public async void Generate() {
        print("Generation Started");
        grid = new bool[mapSize.x, mapSize.y];
        await GenerateRooms();
        await GenerateConnections();
        await FindMST();
        await AddBonusBranches();
        await GenerateWalls();
        await GenerateFloor();
        await GenerateSpawn();
        OnFinished?.Invoke();
        print("Generation Finished");
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
        Vector2Int[,] wallGrid = new Vector2Int[mapSize.x + 1, mapSize.y + 1];

        foreach (Room room in rooms) {
            for (int x1 = 0; x1 < room.Size.x; x1++) {
                Vector3Int position = new Vector3Int(
                    Mathf.RoundToInt(room.Position.x - room.Size.x / 2 + x1),
                    0,
                    Mathf.RoundToInt(room.Position.z + room.Size.z / 2)
                );
                wallGrid[position.x, position.z] += new Vector2Int(0, 1);
                room.Walls.Add(position);
            }
            for (int x2 = 0; x2 < room.Size.x; x2++) {
                Vector3Int position = new Vector3Int(
                    Mathf.RoundToInt(room.Position.x - room.Size.x / 2 + x2),
                    0,
                    Mathf.RoundToInt(room.Position.z - room.Size.z / 2)
                );
                wallGrid[position.x, position.z] += new Vector2Int(0, 2);
                room.Walls.Add(position);
            }
            for (int z1 = 0; z1 < room.Size.z; z1++) {
                Vector3Int position = new Vector3Int(
                    Mathf.RoundToInt(room.Position.x - room.Size.x / 2),
                    0,
                    Mathf.RoundToInt(room.Position.z - room.Size.z / 2 + z1)
                );
                wallGrid[position.x, position.z] += new Vector2Int(1, 0);
                room.Walls.Add(position);
            }
            for (int z2 = 0; z2 < room.Size.z; z2++) {
                Vector3Int position = new Vector3Int(
                    Mathf.RoundToInt(room.Position.x + room.Size.x / 2),
                    0,
                    Mathf.RoundToInt(room.Position.z - room.Size.z / 2 + z2)
                );
                wallGrid[position.x, position.z] += new Vector2Int(2, 0);
                room.Walls.Add(position);
            }
        }

        foreach (connection con in connections) {
            Room a = con.RoomA.Position.x < con.RoomB.Position.x ? con.RoomA : con.RoomB;
            Room b = con.RoomA.Position.x < con.RoomB.Position.x ? con.RoomB : con.RoomA;

            List<Vector3Int> commonWalls = new();

            foreach (Vector3Int aWall in a.Walls) {
                foreach (Vector3Int bWall in b.Walls) {
                    if (aWall == bWall) {
                        commonWalls.Add(aWall);
                    }
                }
            }

            Vector3Int wall = commonWalls[Random.Range(0, commonWalls.Count)];

            Vector2Int direction = Vector2Int.right;
            for (int i = Mathf.RoundToInt(a.Position.x - a.Size.x / 2); i < Mathf.RoundToInt(a.Position.x + a.Size.x / 2); i++) {
                for (int j = Mathf.RoundToInt(b.Position.x - b.Size.x / 2); j < Mathf.RoundToInt(b.Position.x + b.Size.x / 2); j++) {
                    if (i == j) {
                        direction = Vector2Int.up;
                    }
                }
            }

            wallGrid[wall.x, wall.z] += direction * 10;
        }

        GameObject parent = new GameObject("Walls");

        for (int x = 0; x < mapSize.x + 1; x++) {
            for (int y = 0; y < mapSize.y + 1; y++) {
                List<GameObject> instantiated = new();
                switch (wallGrid[x, y].x) {
                    case 1:
                        instantiated.Add(Instantiate(wall, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 0, 0)));
                        break;
                    case 2:
                        instantiated.Add(Instantiate(wall, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 180, 0)));
                        break;
                    case 3:
                        instantiated.Add(Instantiate(wall, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 0, 0)));
                        instantiated.Add(Instantiate(wall, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 180, 0)));
                        break;
                    case > 3:
                        instantiated.Add(Instantiate(door, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 0, 0)));
                        instantiated.Add(Instantiate(door, new Vector3(x, 0, y + .5f), Quaternion.Euler(0, 180, 0)));
                        break;
                }
                switch (wallGrid[x, y].y) {
                    case 1:
                        instantiated.Add(Instantiate(wall, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 90, 0)));
                        break;
                    case 2:
                        instantiated.Add(Instantiate(wall, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 270, 0)));
                        break;
                    case 3:
                        instantiated.Add(Instantiate(wall, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 90, 0)));
                        instantiated.Add(Instantiate(wall, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 270, 0)));
                        break;
                    case > 3:
                        instantiated.Add(Instantiate(door, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 90, 0)));
                        instantiated.Add(Instantiate(door, new Vector3(x + .5f, 0, y), Quaternion.Euler(0, 270, 0)));
                        break;
                }

                foreach (GameObject obj in instantiated) {
                    obj.transform.parent = parent.transform;
                }

                await Task.Yield();
            }
        }
    }

    async Task GenerateFloor() {
        Vector3 position = new Vector3((float)mapSize.x / 2, 0, (float)mapSize.y / 2);
        Vector3 scale = new Vector3(mapSize.x, floor.transform.localScale.y, mapSize.y);

        GameObject obj = Instantiate(floor, position, Quaternion.identity);
        obj.transform.localScale = scale;

        await Task.Yield();
    }

    async Task GenerateSpawn() {
        Room spawnRoom = rooms[Random.Range(0, rooms.Count)];
        GameObject obj = new GameObject("Spawn");
        obj.transform.position = spawnRoom.Position;

        await Task.Yield();
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
                if (x == xMax && z == zMax || x == xMin && z == zMin || x == xMin && z == zMax || x == xMax && z == zMin) {
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

    // void OnDrawGizmos() {
    //     foreach (Room room in rooms) {
    //         float grayNess = Random.Range(0f, 1f);
    //         Gizmos.color = Color.white;
    //         Gizmos.DrawCube(room.Position, room.Size);

    //         Gizmos.color = Color.black;
    //         Gizmos.DrawWireCube(room.Position, room.Size);
    //     }

    //     Gizmos.color = new Color32(43, 84, 78, 255);
    //     foreach (connection con in tempConnections) {
    //         Gizmos.DrawLine(con.RoomA.Position, con.RoomB.Position);
    //     }

    //     Gizmos.color = new Color32(12, 247, 213, 255);
    //     foreach (connection con in connections) {
    //         Gizmos.DrawLine(con.RoomA.Position, con.RoomB.Position);
    //     }
    // }
}