using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizedMazeGeneration : MonoBehaviour
{
    
    [Header("Maze Settings")]
    
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

    public float StepSpeed
    {
        get => stepSpeed;
        set
        {
            stepSpeed = value;
            waitForSeconds = new WaitForSeconds(stepSpeed);
        }
    }
    [SerializeField]
    private float stepSpeed = 0.3f;

    [Header("Maze References")]
    [SerializeField]
    private GameObject mazeWallPrefab;
    
    private Dictionary<Vector2, Cell> mazeMap = new Dictionary<Vector2, Cell>();
    private Dictionary<Vector2, MazeRoom> rooms = new Dictionary<Vector2, MazeRoom>();
    private Vector2 key;
    
    private readonly Stack<Cell> path = new Stack<Cell>();

    private bool canStep;
    private WaitForSeconds waitForSeconds;
    private void Start()
    {
        waitForSeconds = new WaitForSeconds(stepSpeed);
        PopulateMaze();
    }

    [Button("Step")]
    public void NextStep()
    {
        canStep = true;
    }

    /// <summary>
    /// Fill the hashmap with values
    /// </summary>
    private void PopulateMaze()
    {
        mazeMap.Clear();
        rooms.Clear();
        mazeMap = new Dictionary<Vector2, Cell>(Width * Height);
        rooms = new Dictionary<Vector2, MazeRoom>(Width * Height);

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                key.Set(x, y);
                mazeMap[key] = new Cell
                {
                    position = new Vector2(x, y)
                };

                GameObject instance = Instantiate(mazeWallPrefab, new Vector3(x, 0 , y), Quaternion.identity, transform);
                rooms[key] = instance.GetComponent<MazeRoom>();
            }
        }
        
        CreateMaze();
    }

    /// <summary>
    /// Uses Depth-first search to navigate and generate a randomized maze
    /// </summary>
    public void CreateMaze()
    {

        StartCoroutine(Step());

    }
    IEnumerator Step()
    {
        //Mark top cell as visited
        key.Set(0, 0);
        Cell currentCell = mazeMap[key];
        currentCell.isVisited = true;
        rooms[currentCell.position].SetCurrent(true);
        path.Push(currentCell);
        
        //Iterate until we have nothing left to add to stack
        while (path.Count > 0)
        {
            if (!canStep)
            {
                yield return waitForSeconds;    
            }
            else
            {
                canStep = false;
            }
            
            Cell nextCell = GetRandomNeighbor(currentCell);
            
            //Case where we have a neighbor
            if (nextCell != null)
            {
                RemoveWalls(currentCell, nextCell);
                //Visually mark current cell as visited
                rooms[nextCell.position].SetCurrent(true);
                rooms[currentCell.position].SetCurrent(false);
                nextCell.isVisited = true;
                currentCell = nextCell;
                path.Push(currentCell);
            }
            //Backtracking
            else
            {
                //If path still has values, remove latest value
                if (path.Count <= 0) continue;
                currentCell = path.Pop();
                rooms[currentCell.position].SetCurrent(false);
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
        if (distY == -1)
        {
            previous.up = false;
            current.down = false;
            rooms[previous.position].top.SetActive(false);
            rooms[current.position].bottom.SetActive(false);
        }
        //Moving right
        if (distX == -1)
        {
            previous.right = false;
            current.left = false;
            rooms[previous.position].right.SetActive(false);
            rooms[current.position].left.SetActive(false);
        }
        //Moving down
        if (distY == 1)
        {
            previous.down = false;
            current.up = false;
            rooms[previous.position].bottom.SetActive(false);
            rooms[current.position].top.SetActive(false);
        }
        //Moving left
        if (distX == 1)
        {
            previous.left = false;
            current.right = false;
            rooms[previous.position].left.SetActive(false);
            rooms[current.position].right.SetActive(false);
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
        if (IsInBounds(up.x, up.y))
        {
            if (!mazeMap[up].isVisited && mazeMap[up].down)
            {
                neighbors.Add(mazeMap[up]);
            } 
        }
        if (IsInBounds(right.x, right.y))
        {
            if (!mazeMap[right].isVisited && mazeMap[right].left)
            {
                neighbors.Add(mazeMap[right]);
            }
        }
        if (IsInBounds(down.x, down.y))
        {
            if (!mazeMap[down].isVisited && mazeMap[down].up)
            {
                neighbors.Add(mazeMap[down]);
            }
        }
        if (IsInBounds(left.x, left.y))
        {
            if (!mazeMap[left].isVisited && mazeMap[left].right)
            {
                neighbors.Add(mazeMap[left]);
            }
        }

        if (neighbors.Count == 0)
        {
            return null;
        }
        
        //Get a random neighbor
        int index = Random.Range(0, neighbors.Count);
        return neighbors[index];
    }

    /// <summary>
    /// Checks whether a xy coordinate is within the matrix
    /// </summary>
    /// <param name="x">The X coordinate</param>
    /// <param name="y">The Y coordinate</param>
    /// <returns>True if within bounds</returns>
    private bool IsInBounds(float x, float y)
    {
        return x > -1 && x < Width
            && y > -1 && y < Height;
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