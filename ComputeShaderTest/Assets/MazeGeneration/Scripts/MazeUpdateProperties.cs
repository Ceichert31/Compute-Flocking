using System;
using UnityEngine;

public class MazeUpdateProperties : MonoBehaviour
{
    private RandomizedMazeGeneration mazeGeneration;
    private void Start()
    {
        mazeGeneration = GetComponent<RandomizedMazeGeneration>();
    }
    public void UpdateSpeed(FloatEvent ctx)
    {
        mazeGeneration.UpdateSpeed = ctx.FloatValue;
    }
    public void UpdateWidth(FloatEvent ctx)
    {
        mazeGeneration.Width = (int)ctx.FloatValue;
    }
    public void UpdateHeight(FloatEvent ctx)
    {
        mazeGeneration.Height = (int)ctx.FloatValue;
    }
}
