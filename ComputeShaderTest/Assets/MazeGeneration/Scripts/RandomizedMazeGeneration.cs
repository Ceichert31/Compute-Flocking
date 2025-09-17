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

    private Dictionary<Vector2, Cell> mazeMap = new Dictionary<Vector2, Cell>();
    private Vector2 key;
    
    private Stack<Cell> path = new Stack<Cell>();
    private void Start()
    {
        PopulateMaze();
    }

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
    }

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
            
            List<Cell> neighbors = new List<Cell>();

            Vector2 up = new Vector2(key.x, key.y + 1);
            Vector2 right = new Vector2(key.x + 1, key.y);
            Vector2 down = new Vector2(key.x, key.y - 1);
            Vector2 left = new Vector2(key.x - 1, key.y);

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
            
            //Case where we have a neighbor
            if (neighbors.Count > 0)
            {
                //Get a random neighbor
                int index = Random.Range(0, neighbors.Count);
                Cell oldCell = currentCell;
                currentCell = neighbors[index];
                
                RemoveWalls(oldCell, currentCell);
                currentCell.isVisited = true;
            }
            //Backtracking
            else
            {
                currentCell = path.Pop();
            }
        }
    }
    
    private void RemoveWalls(Cell current, Cell next)
    {
        int distX = (int)(current.position.x - next.position.x);
        int distY = (int)(current.position.y - next.position.y);

        //Moving up
        if (distY == 1)
        {
            current.up = false;
            next.down = false;
        }
        //Moving right
        if (distX == 1)
        {
            current.right = false;
            next.left = false;
        }
        //Moving down
        if (distY == -1)
        {
            current.down = false;
            next.up = false;
        }
        //Moving left
        if (distX == -1)
        {
            current.left = false;
            next.right = false;
        }
    }

    /// <summary>
    /// Gets all neighbors, and chooses one randomly
    /// </summary>
    /// <returns></returns>
    private Cell GetNeighbors(int x, int y)
    {
        List<Cell> neighbors = new List<Cell>();

        Vector2 up = new Vector2(x, y + 1);
        Vector2 down = new Vector2(x, y - 1);
        
        //Up
        if (!mazeMap[up].vertical.wallDestroyed && !mazeMap[up].vertical.isVisited)
        {
            neighbors.Add(mazeMap[up]);
            mazeMap[up].position = up;
        }
        //Right
        if (!mazeMap[up].horizontal.wallDestroyed && !mazeMap[up].horizontal.isVisited)
        {
            neighbors.Add(mazeMap[up]);
            mazeMap[up].position = up;
        }
        //Down
        if (!mazeMap[down].vertical.wallDestroyed && !mazeMap[down].vertical.isVisited)
        {
            neighbors.Add(mazeMap[down]);
            mazeMap[up].position = down;
        }
        //Left
        if (!mazeMap[down].horizontal.wallDestroyed && !mazeMap[down].horizontal.isVisited)
        {
            neighbors.Add(mazeMap[down]);
            mazeMap[up].position = down;
        }

        if (neighbors.Count == 0)
        {
            return null;
        }
        
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