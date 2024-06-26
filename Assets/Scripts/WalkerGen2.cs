using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGen2 : MonoBehaviour
{
    public enum Grid // specifies which type of tile is in the current grid position
    {
        FLOOR,
        WALL,
        EMPTY
    }

    // map layout
    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;
    public Tilemap tileMap;
    public Tile Floor;
    public Tile Wall;

    // map size
    public int MapWidth = 30;
    public int MapHeight = 30;

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefreshMap();
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
        Walkers.Add(curWalker);

        TileCount++;

        CreateFloors();
        CreateWalls();
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
            List<Vector3Int> newFloors = new List<Vector3Int>();

            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, (int)curWalker.Position.y, 0);

                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    newFloors.Add(curPos);
                    TileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                }
            }

            // Apply all new floor tiles in a single batch
            foreach (Vector3Int pos in newFloors)
            {
                tileMap.SetTile(pos, Floor);
            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
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
        List<Vector3Int> newWalls = new List<Vector3Int>();

        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == Grid.FLOOR)
                {
                    if (gridHandler[x + 1, y] == Grid.EMPTY)
                    {
                        newWalls.Add(new Vector3Int(x + 1, y, 0));
                        gridHandler[x + 1, y] = Grid.WALL;
                    }
                    if (gridHandler[x - 1, y] == Grid.EMPTY)
                    {
                        newWalls.Add(new Vector3Int(x - 1, y, 0));
                        gridHandler[x - 1, y] = Grid.WALL;
                    }
                    if (gridHandler[x, y + 1] == Grid.EMPTY)
                    {
                        newWalls.Add(new Vector3Int(x, y + 1, 0));
                        gridHandler[x, y + 1] = Grid.WALL;
                    }
                    if (gridHandler[x, y - 1] == Grid.EMPTY)
                    {
                        newWalls.Add(new Vector3Int(x, y - 1, 0));
                        gridHandler[x, y - 1] = Grid.WALL;
                    }
                }
            }
        }

        // Apply all new wall tiles in a single batch
        foreach (Vector3Int pos in newWalls)
        {
            tileMap.SetTile(pos, Wall);
        }
    }

    void RefreshMap()
    {
        // Clear the tilemap
        tileMap.ClearAllTiles();

        // Reset TileCount
        TileCount = 0;

        // Re-initialize the grid and walkers
        InitializeGrid();
    }
}
