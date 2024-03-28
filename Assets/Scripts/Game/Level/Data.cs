using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class LevelData
{
    private int level;
    private string difficulty;
    private int theme;
    private float[] time = { 1, 0, 0, 0 };
    private List<ProcessData> process;

    public LevelData() { }

    public LevelData(int level, string difficulty, int theme, float[] time, List<ProcessData> process)
    {
        this.level = level;
        this.difficulty = difficulty;
        this.theme = theme;
        this.time = time;
        this.process = process;
    }

    public int Level
    {
        get { return level; }
        set { level = value; }
    }

    public string Difficulty
    {
        get { return difficulty; }
        set { difficulty = value; }
    }

    public int Theme
    {
        get { return theme; }
        set { theme = value; }
    }

    public float[] Time
    {
        get { return time; }
        set { time = value; }
    }

    public List<ProcessData> Process
    {
        get { return process; }
        set { process = value; }
    }
}

public class ProcessData
{
    public int MinID { get; set; }
    public int MaxID { get; set; }
    public int TotalTile { get; set; }
    public bool Shuffle { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string[,] Matrix { get; set; }
    public bool PullDown { get; set; }
    public bool PullUp { get; set; }
    public bool PullLeft { get; set; }
    public bool PullRight { get; set; }

    public ProcessData() { }

    public ProcessData(int minID, int maxID, int totalTile, bool shuffle, int row, int column, string[,] matrix, bool pullDown, bool pullUp, bool pullLeft, bool pullRight)
    {
        MinID = minID;
        MaxID = maxID;
        TotalTile = totalTile;
        Shuffle = shuffle;
        Row = row;
        Column = column;
        Matrix = matrix;
        PullDown = pullDown;
        PullUp = pullUp;
        PullLeft = pullLeft;
        PullRight = pullRight;
    }

    //public ProcessData(int minID, int maxID, int totalTile, bool shuffle, int row, int column, string[,] matrix, bool pullDown, bool pullUp, bool pullLeft, bool pullRight) : this(totalTile, row, column, matrix, pullDown, pullUp, pullLeft, pullRight)
    //{
    //    MinID = minID;
    //    MaxID = maxID;
    //    Shuffle = shuffle;
    //}
}