using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizedMazeGeneration : MonoBehaviour
{
    #region Properties
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
            ResetMaze();
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
            ResetMaze();
        }
    }
    [SerializeField]
    private int height = 10;

    public float UpdateSpeed
    {
        get => updateSpeed;
        set => updateSpeed = value;
    }
    [SerializeField]
    private float updateSpeed = 0.3f;

    [SerializeField]
    private bool completeMaze;
    
    #endregion

    [Header("Maze References")]
    [SerializeField]
    private GameObject mazeWallPrefab;
    
    private Dictionary<Vector2, Cell> mazeMap = new Dictionary<Vector2, Cell>();
    private Dictionary<Vector2, MazeRoom> rooms = new Dictionary<Vector2, MazeRoom>();
    private readonly Stack<Cell> path = new Stack<Cell>();
    
    private Cell currentCell;
    private Vector2 key;
    private float timer;
    private bool isResetingMaze;
    
    private const float STEP_DURATION = 10f;
    private void Start()
    {
        PopulateMaze();
        timer = STEP_DURATION;
    }
    /// <summary>
    /// Fills the maze with data and rooms
    /// </summary>
    private void PopulateMaze()
    {
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

                GameObject instance = Instantiate(mazeWallPrefab, new Vector3(x * mazeWallPrefab.transform.localScale.x, 0 , y * mazeWallPrefab.transform.localScale.y), Quaternion.identity, transform);
                rooms[key] = instance.GetComponent<MazeRoom>();
            }
        }
        InitializeMaze();
    }
    /// <summary>
    /// Pushes the starting point (0,0) to the path
    /// </summary>
    private void InitializeMaze()
    {
        //Mark top cell as visited
        key.Set(0, 0);
        currentCell = mazeMap[key];
        currentCell.isVisited = true;
        rooms[currentCell.position].SetCurrent(true);
        
        //Add start cell to path
        path.Push(currentCell);
        isResetingMaze = false;
    }
    private void Update()
    {
        if (completeMaze)
        {
            MazeStep();
        }
     
        //Update every step
        if (isResetingMaze) return;
        timer -= Time.deltaTime * UpdateSpeed;
        if (timer <= 0)
        {
            timer = STEP_DURATION;
            MazeStep();
        }

        //Mark end point
        if (path.Count != 0) return;
        key.Set(Width - 1, Height - 1);
        rooms[key].SetEnd();
    }
    /// <summary>
    /// Uses Depth-first search to navigate and generate a randomized maze
    /// </summary>
    private void MazeStep()
    {
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
            if (path.Count <= 0) return;
            currentCell = path.Pop();
            if (IsInBounds(currentCell.position.x, currentCell.position.y))
            {
                rooms[currentCell.position].SetCurrent(false);
            }
        }
    }
    /// <summary>
    /// Destroys the old maze and generates a new one with updated settings
    /// </summary>
    private void ResetMaze()
    {
        isResetingMaze = true;
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        mazeMap.Clear();
        rooms.Clear();
        
        PopulateMaze();
    }
    #region Helper Methods
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
    #endregion
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