using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGen3 : MonoBehaviour
{
    public enum Grid // specifies which type of tile is in the current grid position
    {
        FLOOR,
        WALL,
        EMPTY,
        WATER
    }

    // map layout
    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;
    public Tilemap tileMap;
    public Tile Floor;
    public Tile Wall;
    public Tile Water;

    // map size
    public int MapWidth = 50; // Adjusted for larger map
    public int MapHeight = 50; // Adjusted for larger map

    // map walkers
    public int MaximumWalkers = 10;
    public int TileCount = default;

    // fill in space...try with lakes or any body of water
    public float FillPercentage = 0.4f;

    void Start()
    {
        // starts the grid
        InitializeGrid();
    }

    void Update()
    {
        // Regenerate the map when 'R' key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearMap();
            InitializeGrid();
        }
    }

    void InitializeGrid()
    {
        // intiliaze tilemap
        gridHandler = new Grid[MapWidth, MapHeight];

        // every tile is empty to start with generation
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }
        }

        // create a walker 
        Walkers = new List<WalkerObject>();

        // makes sure its using int instead of float for exact placement....stupid thing
        Vector3Int TileCenter = new Vector3Int(gridHandler.GetLength(0) / 2, gridHandler.GetLength(1) / 2, 0);

        // create new walker and use conuctor to set values
        WalkerObject curWalker = new WalkerObject(new Vector2(TileCenter.x, TileCenter.y), GetDirection(), 0.5f);
        gridHandler[TileCenter.x, TileCenter.y] = Grid.FLOOR;
        tileMap.SetTile(TileCenter, Floor);
        Walkers.Add(curWalker);

        TileCount++;

        CreateFloors();
        CreateWalls();
        CreateWaterBodies();
    }

    Vector2 GetDirection()
    {
        int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    void CreateFloors()
    {
        while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
        {
            bool hasCreatedFloor = false;
            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, (int)curWalker.Position.y, 0);

                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    tileMap.SetTile(curPos, Floor);
                    TileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                    hasCreatedFloor = true;
                }
            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();

            // if a tile is placed, wait a bit to avoid overlapping and bugs
            if (hasCreatedFloor)
            {
                // Removed yield return new WaitForSeconds(WaitTime);
            }
        }
    }

    void ChanceToRemove()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
            {
                Walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = Walkers[i];
                curWalker.Direction = GetDirection();
                Walkers[i] = curWalker;
            }
        }
    }

    void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = Walkers[i].Position;

                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                Walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject FoundWalker = Walkers[i];
            FoundWalker.Position += FoundWalker.Direction;
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, gridHandler.GetLength(0) - 2);
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, gridHandler.GetLength(1) - 2);
            Walkers[i] = FoundWalker;
        }
    }

    void CreateWalls()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == Grid.FLOOR)
                {
                    if (gridHandler[x + 1, y] == Grid.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x + 1, y, 0), Wall);
                        gridHandler[x + 1, y] = Grid.WALL;
                    }

                    if (gridHandler[x - 1, y] == Grid.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x - 1, y, 0), Wall);
                        gridHandler[x - 1, y] = Grid.WALL;
                    }

                    if (gridHandler[x, y + 1] == Grid.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x, y + 1, 0), Wall);
                        gridHandler[x, y + 1] = Grid.WALL;
                    }
                    if (gridHandler[x, y - 1] == Grid.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x, y - 1, 0), Wall);
                        gridHandler[x, y - 1] = Grid.WALL;
                    }
                }
            }
        }
    }

    void CreateWaterBodies()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == Grid.FLOOR && UnityEngine.Random.value < 0.05f)
                { // 5% chance to turn floor into water
                    tileMap.SetTile(new Vector3Int(x, y, 0), Water);
                    gridHandler[x, y] = Grid.WATER;
                }
            }
        }
    }

    void ClearMap()
    {
        tileMap.ClearAllTiles();
        TileCount = 0;
        Walkers.Clear();
    }
}
