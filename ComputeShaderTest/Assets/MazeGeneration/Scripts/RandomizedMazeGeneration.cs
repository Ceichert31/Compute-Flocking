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

    private Dictionary<Vector2, Vertex> mazeMap = new Dictionary<Vector2, Vertex>();
    private Vector2 key;
    
    private Stack<Vertex> path = new Stack<Vertex>();
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
                mazeMap[key] = new Vertex();
            }
        }
    }

    public void CreateMaze()
    {
        //Mark top cell as visited
        key.Set(0, 0);
        Vertex currentVertex = mazeMap[key];
        path.Push(currentVertex);
        
        //Iterate until we have nothing left to add to stack
        while (path.Count > 0)
        {
            var neighbor = GetNeighbors((int)key.x, (int)key.y);

            //Case where we have a neighbor
            if (neighbor != null)
            {
                //neighbor.
            }
            //Backtracking
            else
            {
                path.Pop();
            }
        }
    }

    /// <summary>
    /// Gets all neighbors, and chooses one randomly
    /// </summary>
    /// <returns></returns>
    private Vertex GetNeighbors(int x, int y)
    {
        List<Vertex> neighbors = new List<Vertex>();

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

class Vertex
{
    public Edge vertical;
    public Edge horizontal;
    public Vector2 position;
}

struct Edge
{
    public bool isVisited;
    public bool wallDestroyed;
}