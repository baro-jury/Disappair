using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SetUpBoard : MonoBehaviour
{
    public static SetUpBoard instance;
    public static SetUpMap setup;
    public static LevelData levelData;
    public static ProcessData processData;
    private int totalTile, row, column;
    private bool pullDown, pullUp, pullLeft, pullRight;
    private string[,] matrix;
    public int sideMin = 112, sideMax = 180;

    [SerializeField]
    private Button Tile;

    void _MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Awake()
    {
        _MakeInstance();
    }

    public void _StartCreating()
    {
        _ResetBoard();
        _InstantiateLevel();
        _GenerateTiles();
    }

    #region Khởi tạo creative map
    void _ResetBoard()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void _InstantiateLevel()
    {
        Timer.time = levelData.Time[0];

        totalTile = processData.TotalTile;
        row = processData.Row;
        column = processData.Column;
        matrix = processData.Matrix;
        pullDown = processData.PullDown;
        pullUp = processData.PullUp;
        pullLeft = processData.PullLeft;
        pullRight = processData.PullRight;
    }

    void _GenerateTiles()
    {
        float sideTile = gameObject.GetComponent<RectTransform>().sizeDelta.x / column;
        float temp = gameObject.GetComponent<RectTransform>().sizeDelta.y / row;
        sideTile = sideTile < temp ? (int)sideTile : (int)temp;
        if (sideMax < sideTile)
        {
            sideTile = sideMax;
        }
        else if (sideTile < sideMin)
        {
            sideTile = sideMin;
        }
        float boardWidth = column * sideTile;
        float boardHeight = row * sideTile;

        Tile.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(sideTile, sideTile);

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                Vector3 tilePos = _ConvertMatrixIndexToPosition(r, c, boardWidth, boardHeight, sideTile) / 40;
                var objBtn = Instantiate(Tile, tilePos, Quaternion.identity, gameObject.transform);
                objBtn.transform.localPosition = tilePos * 40;
                if (matrix[r, c] == null || matrix[r, c] == "")
                {
                    objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = setup.btNone.transform.GetChild(1).GetComponent<Image>().sprite;
                }
                else if (matrix[r, c] == "?")
                {
                    objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = setup.btRandom.transform.GetChild(1).GetComponent<Image>().sprite;
                }
                else if (matrix[r, c] == "0")
                {
                    objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = setup.btBlock.transform.GetChild(1).GetComponent<Image>().sprite;
                }
                else
                {
                    objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = SpriteController.spritesDict[matrix[r, c]];
                }
            }
        }
    }

    public int[] _ConvertPositionToMatrixIndex(float x, float y, float boardWidth, float boardHeight, float tileSize)
    {
        int[] temp = new int[2];

        temp[0] = (int)(((boardHeight - tileSize) / 2 - y) / tileSize); //row
        temp[1] = (int)(((boardWidth - tileSize) / 2 + x) / tileSize); //column

        return temp;
    }

    public Vector3 _ConvertMatrixIndexToPosition(int row, int col, float boardWidth, float boardHeight, float tileSize)
    {
        return new Vector3((col * tileSize - (boardWidth - tileSize) / 2), ((boardHeight - tileSize) / 2 - row * tileSize), 0);
    }

    #endregion

}