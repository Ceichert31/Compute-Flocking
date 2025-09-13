using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ConwaysGameOfLife : MonoBehaviour
{
    [Header("Grid Settings")]
    public int Rows
    {
        get => rows;
        set
        {
            CreateGrid();
            rows = value;
        }
    }
    [SerializeField]
    private int rows = 10;
    public int Columns
    {
        get => columns;
        set
        {
            CreateGrid();
            columns = value;
        }
    }
    [SerializeField]
    private int columns = 10;
    public bool PauseGame
    {
        get
        {
            return pauseGame;
        }
        set
        {
            pauseGame = value;
        }
    }
    [SerializeField]
    private bool pauseGame;
    public bool EnableWrapping
    {
        get
        {
            return enableWrapping;
        }
        set
        {
            enableWrapping = value;
        }
    }
    [SerializeField]
    private bool enableWrapping;
    public float PlaySpeed
    {
        get
        {
            return playSpeed;
        }
        set
        {
            playSpeed = value;
        }
    }
    [SerializeField]
    private float playSpeed = 1f;

    private float playTimer;
    
    private const float TIME_UNTIL_UPDATE = 5f;
    private const int GRID_DISTANCE = 2;
    
    [Header("Game of Life References")]
    [SerializeField]
    private GameObject cubePrefab;
    
    private readonly Dictionary<Vector2, GameObject> grid = new Dictionary<Vector2, GameObject>();

    private Vector2 key;

    private void Start()
    {
        CreateGrid();
    }
    private int x=0, y=0;
    private void Update()
    {
        if (pauseGame) return;
        
        playTimer -= Time.deltaTime * playSpeed;
        
        //Update timer
        if (playTimer > 0) return;
        
        playTimer = TIME_UNTIL_UPDATE;
        
        GameOfLife();
    }

    [ContextMenu("Next Turn")]
    public void ProgressGame()
    {
        GameOfLife();
    }

    /// <summary>
    /// Logic for the game of life
    /// </summary>
    private void GameOfLife()
    {
        for (int x = 0; x < rows; ++x)
        {
            for (int y = 0; y < columns; ++y)
            {
                int count = CountNeighbors(x, y);

                //Alive case
                if (GetValue(x, y))
                {
                    if (count < 2 || count > 3)
                    {
                        SetValue(x, y, false);
                    }
                    else
                    {
                        SetValue(x, y, true);
                    }
                }
                //Dead case
                else
                {
                    SetValue(x, y, count == 3);
                }
            }
        }
    }

    /// <summary>
    /// Counts neighbors and loops around edges of grid
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <returns>The number of active neighbors</returns>
    int CountNeighbors(int x, int y)
    {
        int count = 0;
        
        for (int lin = -1; lin <= 1; ++lin)
        {
            for (int col = -1; col <= 1; ++col)
            {
                int xIndex = x + col, yIndex = y + lin;
                
                if (lin == x || col == y) continue;

                if (EnableWrapping)
                {
                    if (xIndex < 0)
                    {
                        xIndex = Rows - 1;
                    }
                    else if (xIndex >= rows)
                    {
                        xIndex = 0;
                    }

                    if (yIndex < 0)
                    {
                        yIndex = Columns - 1;
                    }
                    else if (yIndex >= columns)
                    {
                        yIndex = 0;
                    }
                }
                
                if (GetValue(xIndex, yIndex))
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Gets the value of a coordinate
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <returns>The value of the cell at the coordinates</returns>
    bool GetValue(int x, int y)
    {
        key.Set(x, y);
        
        //If it doesn't exsist, return false
        if (!grid.TryGetValue(key, out GameObject cell)) return false;

        return cell.activeInHierarchy;
    }

    /// <summary>
    /// Sets the value of a specified cell
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="value">The value we want to set it as</param>
    void SetValue(int x, int y, bool value)
    {
        if (!grid.TryGetValue(key, out GameObject cell)) return;
        
        cell.SetActive(value);
    }

    /// <summary>
    /// Generates a grid of cubes with given parameters
    /// </summary>
    [ContextMenu("Generate Grid")]
    //Function for creating grid
    public void CreateGrid()
    {
        //transform.position = new (-(Rows / GRID_DISTANCE), -(Columns / GRID_DISTANCE), 0);
        Camera.main.transform.position = new (Rows, Columns, -1 * (Rows + Columns));
        
        //If grid is full, empty before adding more elements
        if (grid.Count > 0)
        {
            ClearGrid();
        }
        
        //Iterate through and populate
        for (int x = 0; x < Rows; ++x)
        {
            for (int y = 0; y < Columns; ++y)
            {
                key.Set(x, y);
                GameObject instance = Instantiate(cubePrefab, new Vector3(x * GRID_DISTANCE, y * GRID_DISTANCE, 0), Quaternion.identity, transform);
                instance.name = $"ConwaysGameOfLife_{x}_{y}";
                instance.SetActive(Random.Range(0, 2) == 1);
                grid.Add(key, instance);
            }
        }
    }

    /// <summary>
    /// Destroys all game objects within the grid and clears it
    /// </summary>
    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        /*for (int x = 0; x < oldRowValue; ++x)
        {
            for (int y = 0; y < oldColumnValue; ++y)
            {
                key.Set(x, y);
                
                //Check if value exists at key
                if (!grid.TryGetValue(key, out GameObject value)) continue;
                
                //Remove
                DestroyImmediate(value);
                grid.Remove(key);
            }
        }*/
        
        //Second clean up to make sure we got everything
        int count = transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        
        grid.Clear();
    }
}
