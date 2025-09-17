using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizedMazeGeneration : MonoBehaviour
{
    public int Width
    {
        get
        {
            return width;
        }
        set
        {
            width = value;
        }
    }
    [SerializeField]
    private int width = 10;
    
    public int Height
    {
        get
        {
            return height;
        }
        set
        {
            height = value;
        }
    }
    [SerializeField]
    private int height = 10;

    private readonly Dictionary<Vector2, Cell> mazeMap = new Dictionary<Vector2, Cell>();
    private Vector2 key;
    
    private readonly Stack<Cell> path = new Stack<Cell>();
    private void Start()
    {
        PopulateMaze();
    }

    /// <summary>
    /// Fill the hashmap with values
    /// </summary>
    private void PopulateMaze()
    {
        mazeMap.Clear();

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                key.Set(x, y);
                mazeMap[key] = new Cell
                {
                    position = new Vector2(x, y)
                };
            }
        }
        
        CreateMaze();
    }

    /// <summary>
    /// Uses Depth-first search to navigate and generate a randomized maze
    /// </summary>
    public void CreateMaze()
    {
        //Mark top cell as visited
        key.Set(0, 0);
        Cell currentCell = mazeMap[key];
        path.Push(currentCell);
        
        //Iterate until we have nothing left to add to stack
        while (path.Count > 0)
        {
            //Mark as visited and add to stack
            currentCell.isVisited = true;
            path.Push(currentCell);
            
            //Cache previous cell and get new cell
            Cell previousCell = currentCell;
            currentCell = GetRandomNeighbor(previousCell);
            
            //Case where we have a neighbor
            if (currentCell != null)
            {
                RemoveWalls(previousCell, currentCell);
                currentCell.isVisited = true;
            }
            //Backtracking
            else
            {
                currentCell = path.Pop();
            }
        }
    }
    /// <summary>
    /// Removes a wall between an old cell and the new cell
    /// </summary>
    /// <param name="previous">The previous cell</param>
    /// <param name="current">The new current cell</param>
    private void RemoveWalls(Cell previous, Cell current)
    {
        int distX = (int)(previous.position.x - current.position.x);
        int distY = (int)(previous.position.y - current.position.y);

        //Moving up
        if (distY == 1)
        {
            previous.up = false;
            current.down = false;
        }
        //Moving right
        if (distX == 1)
        {
            previous.right = false;
            current.left = false;
        }
        //Moving down
        if (distY == -1)
        {
            previous.down = false;
            current.up = false;
        }
        //Moving left
        if (distX == -1)
        {
            previous.left = false;
            current.right = false;
        }
    }

    /// <summary>
    /// Gets all neighbors, and chooses one randomly
    /// </summary>
    /// <returns></returns>
    private Cell GetRandomNeighbor(Cell current)
    {
        List<Cell> neighbors = new List<Cell>();

        Vector2 up = new Vector2(current.position.x, current.position.y + 1);
        Vector2 right = new Vector2(current.position.x + 1, current.position.y);
        Vector2 down = new Vector2(current.position.x, current.position.y - 1);
        Vector2 left = new Vector2(current.position.x - 1, current.position.y);

        //Add all visitable neighbors to list
        if (!mazeMap[up].isVisited && mazeMap[up].down)
        {
            neighbors.Add(mazeMap[up]);
        } 
        if (!mazeMap[right].isVisited && mazeMap[right].left)
        {
            neighbors.Add(mazeMap[right]);
        }
        if (!mazeMap[down].isVisited && mazeMap[down].up)
        {
            neighbors.Add(mazeMap[down]);
        }
        if (!mazeMap[left].isVisited && mazeMap[left].right)
        {
            neighbors.Add(mazeMap[left]);
        }

        if (neighbors.Count == 0)
        {
            return null;
        }
        
        //Get a random neighbor
        int index = Random.Range(0, neighbors.Count);
        return neighbors[index];
    }
}

class Cell
{
    public Vector2 position;
    public bool up = true;
    public bool right = true;
    public bool down = true;
    public bool left = true;
    public bool isVisited;
}