using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ConwaysGameOfLife : MonoBehaviour
{

    [Header("Grid Settings")]

    public int Rows
    {
        get => _rows;
        set
        {
            _rows = value;
            CreateGrid();
        }
    }
    [SerializeField]
    private int _rows = 10;
        
    public int Columns
    {
        get => _columns;
        set
        {
            _columns = value;
            CreateGrid();
        }
    }
    [SerializeField]
    private int _columns = 10;
    public bool PlayGame
    {
        get
        {
            return playGame;
        }
        set
        {
            playGame = value;
        }
    }
    [SerializeField]
    private bool playGame;
    
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
    private void Update()
    {
        if (!playGame) return;
        
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
        for (int x = 0; x < _rows; ++x)
        {
            for (int y = 0; y < _columns; ++y)
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
                
                /*xIndex %= _columns;
                yIndex %= _rows;*/

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
        transform.position = new (Columns * Rows / GRID_DISTANCE, Rows * Rows / GRID_DISTANCE, 0);
        
        //If grid is full, empty before adding more elements
        if (grid.Count > 0)
        {
            ClearGrid();
        }
        
        //Iterate through and populate
        for (int x = 0; x < _rows; ++x)
        {
            for (int y = 0; y < _columns; ++y)
            {
                key.Set(x, y);
                GameObject instance = Instantiate(cubePrefab, new Vector3(x * GRID_DISTANCE, y * GRID_DISTANCE, 0), Quaternion.identity, transform);
                instance.name = $"ConwaysGameOfLife_{x},{y}";
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
        for (int x = 0; x < _rows; ++x)
        {
            for (int y = 0; y < _columns; ++y)
            {
                key.Set(x, y);
                
                //Check if value exists at key
                if (!grid.TryGetValue(key, out GameObject value)) continue;
                
                //Remove
                DestroyImmediate(value);
                grid.Remove(key);
            }
        }
        grid.Clear();
    }
}
