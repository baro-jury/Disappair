using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class TrialBoard : MonoBehaviour
{
    public static TrialBoard instance;
    public static LevelData levelData;
    public static List<Transform> buttonList = new List<Transform>();
    public static List<TileController> buttonListWithoutBlocker = new List<TileController>();
    public static Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
    public static int row, column;
    public int sideMin = 112, sideMax = 180;
    private int minId, maxId, process, orderOfPullingDirection;
    private bool shuffle, pullDown, pullUp, pullLeft, pullRight;
    private string[,] matrix;

    [SerializeField]
    private TileController Tile;
    [SerializeField]
    private GameObject FirstAnchor, LastAnchor;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Keypad0))
    //    {
    //        Debug.Log(JsonConvert.SerializeObject(matrix));
    //        string temp = null;
    //        foreach (var item in buttonList)
    //        {
    //            temp += item.name + " ";
    //        }
    //        Debug.Log(temp);
    //    }
    //}

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

    #region Khởi tạo màn chơi
    void _InstantiateProcess(int orderNumber)
    {
        process = orderNumber;
        minId = levelData.Process[orderNumber - 1].MinID;
        maxId = levelData.Process[orderNumber - 1].MaxID;
        shuffle = levelData.Process[orderNumber - 1].Shuffle;
        row = levelData.Process[orderNumber - 1].Row;
        column = levelData.Process[orderNumber - 1].Column;
        pullDown = levelData.Process[orderNumber - 1].PullDown;
        pullUp = levelData.Process[orderNumber - 1].PullUp;
        pullLeft = levelData.Process[orderNumber - 1].PullLeft;
        pullRight = levelData.Process[orderNumber - 1].PullRight;
        matrix = levelData.Process[orderNumber - 1].Matrix;
    }

    public void _GoToProcess(int order)
    {
        buttonList.Clear();
        buttonListWithoutBlocker.Clear();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        orderOfPullingDirection = 0;
        _InstantiateProcess(order);
        dict = SpriteController.instance._CreateSubDictionary(minId, maxId);
        _GenerateTiles();
        _SpreadTiles();
    }
    #endregion

    #region Tạo Tiles
    void _GenerateTiles()
    {
        int repeat = 0, index = minId;
        SpriteController.instance._ShuffleImage(dict);

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
        Tile.Size = (int)sideTile;
        FirstAnchor.GetComponent<TileController>().Size = (int)sideTile;
        LastAnchor.GetComponent<TileController>().Size = (int)sideTile;

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (matrix[r, c] != "")
                {
                    Vector3 tilePos = _ConvertMatrixIndexToLocalPos(r, c, boardWidth, boardHeight, sideTile) / 40;
                    var objBtn = Instantiate(Tile, tilePos, Quaternion.identity, gameObject.transform);
                    if (matrix[r, c] == "?")
                    {
                        matrix[r, c] = index.ToString();
                        objBtn.Id = index;
                        objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = dict[index.ToString()];
                        buttonListWithoutBlocker.Add(objBtn);
                        repeat++;
                    }
                    else if (matrix[r, c] == "0")
                    {
                        objBtn.Id = 0;
                        objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = dict["0"];
                        objBtn.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        objBtn.Id = int.Parse(matrix[r, c]);
                        objBtn.gameObject.transform.GetChild(1).GetComponent<Image>().sprite = dict[matrix[r, c]];
                        buttonListWithoutBlocker.Add(objBtn);
                    }
                    objBtn.Index = (r, c);
                    objBtn.Size = (int)sideTile;
                    objBtn.name = objBtn.Id + " - " + objBtn.Index;
                    objBtn.transform.localPosition = tilePos * 40;
                    buttonList.Add(objBtn.transform);
                    if (repeat == 2)
                    {
                        repeat = 0;
                        index++;
                        if (index > maxId)
                        {
                            index = minId;
                        }
                    }
                }
            }
        }
    }

    void _SpreadTiles()
    {
        GameplayTrial.instance._EnableSupporter(false);
        List<int> source = new List<int>();
        List<Transform> trsfTiles = new List<Transform>();
        List<Vector3> posTiles = new List<Vector3>();
        List<(int, int)> indexTiles = new List<(int, int)>();

        foreach (var item in buttonListWithoutBlocker)
        {
            source.Add(buttonListWithoutBlocker.FindIndex(x => x == item));
            trsfTiles.Add(item.transform);
            posTiles.Add(item.transform.localPosition);
            indexTiles.Add(item.Index);
        }
        foreach (var item in trsfTiles)
        {
            item.DOLocalMove(Vector3.zero, 0f).SetEase(Ease.InBack).SetUpdate(true);
        }
        if (shuffle)
        {
            _MakeConnectableCouple(source, trsfTiles, posTiles, indexTiles, 0);
            source = Shuffle(source.Count);
        }
        for (int i = 0; i < source.Count; i++)
        {
            trsfTiles[i].GetComponent<TileController>().Index = indexTiles[source[i]];
            trsfTiles[i].name = trsfTiles[i].GetComponent<TileController>().Id
                + " - " + trsfTiles[i].GetComponent<TileController>().Index;
            matrix[indexTiles[source[i]].Item1, indexTiles[source[i]].Item2] = trsfTiles[i].GetComponent<TileController>().Id + "";
            if (i < source.Count - 1)
            {
                trsfTiles[i].DOLocalMove(posTiles[source[i]], 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                trsfTiles[i].DOLocalMove(posTiles[source[i]], 0.4f).SetEase(Ease.OutBack).SetUpdate(true)
                    .OnComplete(() => { GameplayTrial.instance._EnableSupporter(true); });
            }
        }
    }

    int NumberOfSameTiles()
    {
        int num = Random.Range(2, 5);
        while (num % 2 == 1)
        {
            num = Random.Range(2, 5);
        }
        return num;
    }

    (int, int) _ConvertPositionToMatrixIndex(float x, float y, float boardWidth, float boardHeight, float tileSize)
    {
        (int, int) temp = new();
        temp.Item1 = (int)(((boardHeight - tileSize) / 2 - y) / tileSize);  //row
        temp.Item2 = (int)(((boardWidth - tileSize) / 2 + x) / tileSize); //column
        return temp;
    }

    Vector3 _ConvertMatrixIndexToLocalPos(int row, int col, float boardWidth, float boardHeight, float tileSize)
    {
        return new Vector3((col * tileSize - (boardWidth - tileSize) / 2), ((boardHeight - tileSize) / 2 - row * tileSize), 0);
    }

    #endregion

    #region Sắp xếp Tiles
    public void _CheckPossibleConnection()
    {
        bool canConnect = false;
        if (buttonListWithoutBlocker.Count == 0)
        {
            canConnect = true;
        }
        for (int index = 1; index < dict.Count; index++)
        {
            List<Transform> temp = _SearchSameTiles(dict.ElementAt(index).Value);
            if (temp.Count != 0)
            {
                for (int i = 0; i < (temp.Count - 1); i++)
                {
                    for (int j = (i + 1); j < temp.Count; j++)
                    {
                        if (GameplayTrial.instance._HasAvailableConnection(temp[i], temp[j]))
                        {
                            canConnect = true;
                        }
                    }
                }
            }
        }
        if (!canConnect)
        {
            _RearrangeTiles();
        }
    }

    public void _RearrangeTiles()
    {
        GameplayTrial.instance._EnableSupporter(false);
        List<int> source = new List<int>();
        List<Transform> trsfTiles = new List<Transform>();
        List<Vector3> posTiles = new List<Vector3>();
        List<(int, int)> indexTiles = new List<(int, int)>();

        foreach (var item in buttonListWithoutBlocker)
        {
            source.Add(buttonListWithoutBlocker.FindIndex(x => x == item));
            trsfTiles.Add(item.transform);
            posTiles.Add(item.transform.localPosition);
            indexTiles.Add(item.Index);
        }
        foreach (var item in trsfTiles)
        {
            item.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.InBack).SetUpdate(true);
        }
        _MakeConnectableCouple(source, trsfTiles, posTiles, indexTiles, 0.4f);
        source = Shuffle(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            trsfTiles[i].GetComponent<TileController>().Index = indexTiles[source[i]];
            trsfTiles[i].name = trsfTiles[i].GetComponent<TileController>().Id
                + " - " + trsfTiles[i].GetComponent<TileController>().Index;
            matrix[indexTiles[source[i]].Item1, indexTiles[source[i]].Item2] = trsfTiles[i].GetComponent<TileController>().Id + "";
            if (i < source.Count - 1)
            {
                trsfTiles[i].DOLocalMove(posTiles[source[i]], 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(0.4f);
            }
            else
            {
                trsfTiles[i].DOLocalMove(posTiles[source[i]], 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(0.4f)
                    .OnComplete(() => { GameplayTrial.instance._EnableSupporter(true); });
            }
        }
    }

    List<int> Shuffle(int total = 1000)
    {
        int count = 1;
        List<int> temp = new();
        temp.Add(0);
        for (int i = 1; i < total; i++)
        {
            temp.Insert(UnityEngine.Random.Range(0, count), i);
            count++;
        }
        return temp;
    }

    void _MakeConnectableCouple(List<int> source, List<Transform> trsfTiles, List<Vector3> posTiles, List<(int, int)> indexTiles, float delay)
    {
        Transform tile1 = trsfTiles[0];
        source.Remove(source.Count - 1);
        trsfTiles.Remove(tile1);
        Transform tile2 = trsfTiles.First(x => x.GetComponent<TileController>().Id == tile1.GetComponent<TileController>().Id);
        source.Remove(source.Count - 1);
        trsfTiles.Remove(tile2);

        Vector3 tilePos1 = new(), tilePos2 = new();
        (int, int) index1 = new(), index2 = new();

        for (int i = 0; i < (buttonListWithoutBlocker.Count - 1); i++)
        {
            for (int j = (i + 1); j < buttonListWithoutBlocker.Count; j++)
            {
                if (GameplayTrial.instance._HasAvailableConnection(buttonListWithoutBlocker[i].transform, buttonListWithoutBlocker[j].transform))
                {
                    tilePos1 = buttonListWithoutBlocker[i].transform.localPosition;
                    tilePos2 = buttonListWithoutBlocker[j].transform.localPosition;
                    index1 = buttonListWithoutBlocker[i].Index;
                    index2 = buttonListWithoutBlocker[j].Index;
                    goto arrange;
                }
            }
        }
    arrange:
        tile1.DOLocalMove(tilePos1, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(delay);
        posTiles.Remove(tilePos1);
        tile2.DOLocalMove(tilePos2, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(delay);
        posTiles.Remove(tilePos2);

        tile1.GetComponent<TileController>().Index = index1;
        tile1.name = tile1.GetComponent<TileController>().Id + " - " + tile1.GetComponent<TileController>().Index;
        matrix[index1.Item1, index1.Item2] = tile1.GetComponent<TileController>().Id + "";
        indexTiles.Remove(index1);
        tile2.GetComponent<TileController>().Index = index2;
        tile2.name = tile2.GetComponent<TileController>().Id + " - " + tile2.GetComponent<TileController>().Index;
        matrix[index2.Item1, index2.Item2] = tile2.GetComponent<TileController>().Id + "";
        indexTiles.Remove(index2);
    }

    #endregion

    #region Xử lý ingame
    public bool _HasButtonInLocation(float x, float y)
    {
        foreach (var trans in buttonList)
        {
            if (new Vector3(x, y, 0) == trans.localPosition)
            {
                return true;
            }
        }
        return false;
    }

    public void _DeactivateTile(TileController t)
    {
        int rTemp = (int)(
                ((row * t.Size - t.Size) / 2 - t.transform.localPosition.y) / t.Size
                );
        int cTemp = (int)(
            ((column * t.Size - t.Size) / 2 + t.transform.localPosition.x) / t.Size
            );

        matrix[rTemp, cTemp] = "";
        buttonList.Remove(t.transform);
        buttonListWithoutBlocker.Remove(t);
    }

    public void _CheckProcess()
    {
        if (buttonListWithoutBlocker.Count == 0)
        {
            if (process != levelData.Process.Count)
            {
                _GoToProcess(process + 1);
            }
            else
            {
                Time.timeScale = 0;
            }
        }
    }

    public List<Transform> _SearchSameTiles(Sprite sprite)
    {
        List<Transform> list = new List<Transform>();
        foreach (var trans in buttonListWithoutBlocker)
        {
            if (trans.transform.GetChild(1).GetComponent<Image>().sprite == sprite)
            {
                list.Add(trans.transform);
            }
        }
        return list;
    }

    #endregion

    #region Xử lý quy luật sau khi ăn Tiles
    void _SearchAndPullTile(int rowBefore, int colBefore, int rowAfter, int colAfter) // tìm Tile theo index trong ma tran
    {
        Vector3 posAfter = _ConvertMatrixIndexToLocalPos(rowAfter, colAfter, column * Tile.Size, row * Tile.Size, Tile.Size);
        foreach (var t in buttonListWithoutBlocker)
        {
            if (t.Index == (rowBefore, colBefore))
            {
                t.Index = (rowAfter, colAfter);
                t.transform.DOLocalMove(posAfter, 0.2f).SetEase(Ease.InOutQuad).SetUpdate(true);
                t.name = t.Id + " - " + t.Index;

                matrix[rowAfter, colAfter] = matrix[rowBefore, colBefore];
                matrix[rowBefore, colBefore] = "";

                break;
            }
        }
    }

    public void _ActivateGravity()
    {
        if (pullDown && !pullUp && !pullLeft && !pullRight)
        {
            _PullTilesToBottom(0, row - 1);
        }
        else if (!pullDown && pullUp && !pullLeft && !pullRight)
        {
            _PullTilesToTop(0, row - 1);
        }
        else if (!pullDown && !pullUp && pullLeft && !pullRight)
        {
            _PullTilesToLeft(0, column - 1);
        }
        else if (!pullDown && !pullUp && !pullLeft && pullRight)
        {
            _PullTilesToRight(0, column - 1);
        }
        else if (!pullDown && !pullUp && pullLeft && pullRight)
        {
            _PullTilesToLeft(0, column / 2 - 1);
            _PullTilesToRight(column / 2, column - 1);
        }
        else if (pullDown && pullUp && !pullLeft && !pullRight)
        {
            _PullTilesToTop(0, row / 2 - 1);
            _PullTilesToBottom(row / 2, row - 1);
        }
        else if (pullDown && pullUp && pullLeft && pullRight)
        {
            switch (orderOfPullingDirection % 4)
            {
                case 0:
                    _PullTilesToTop(0, row - 1);
                    break;
                case 1:
                    _PullTilesToRight(0, column - 1);
                    break;
                case 2:
                    _PullTilesToBottom(0, row - 1);
                    break;
                case 3:
                    _PullTilesToLeft(0, column - 1);
                    break;
            }
            if (levelData.Level % 2 == 0)
            {
                orderOfPullingDirection++;
            }
            else
            {
                orderOfPullingDirection += 3;
            }
        }
    }

    void _PullTilesToBottom(int rowFirst, int rowLast)
    {
        var tempArr = new string[rowLast - rowFirst + 1];
        int blankTiles = 0;
        for (int c = 0; c < column; c++)
        {
            for (int r = rowFirst; r <= rowLast; r++)
            {
                tempArr[r - rowFirst] = matrix[r, c];
            }

            for (int i = tempArr.Length - 1; i >= 0; i--)
            {
                if (tempArr[i] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (tempArr[i] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(rowFirst + i, c, rowFirst + i + blankTiles, c);
                        }
                    }
                }
            }
            blankTiles = 0;
        }
    }

    void _PullTilesToTop(int rowFirst, int rowLast)
    {
        var tempArr = new string[rowLast - rowFirst + 1];
        int blankTiles = 0;

        for (int c = 0; c < column; c++)
        {
            for (int r = rowFirst; r <= rowLast; r++)
            {
                tempArr[r - rowFirst] = matrix[r, c];
            }

            for (int i = 0; i < tempArr.Length; i++)
            {
                if (tempArr[i] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (tempArr[i] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(rowFirst + i, c, rowFirst + i - blankTiles, c);
                        }
                    }
                }
            }
            blankTiles = 0;
        }
    }

    void _PullTilesToLeft(int colFirst, int colLast)
    {
        var tempArr = new string[colLast - colFirst + 1];
        int blankTiles = 0;

        for (int r = 0; r < row; r++)
        {
            for (int c = colFirst; c <= colLast; c++)
            {
                tempArr[c - colFirst] = matrix[r, c];
            }

            for (int i = 0; i < tempArr.Length; i++)
            {
                if (tempArr[i] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (tempArr[i] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, colFirst + i, r, colFirst + i - blankTiles);
                        }
                    }
                }
            }
            blankTiles = 0;
        }
    }

    void _PullTilesToRight(int colFirst, int colLast)
    {
        var tempArr = new string[colLast - colFirst + 1];
        int blankTiles = 0;

        for (int r = 0; r < row; r++)
        {
            for (int c = colFirst; c <= colLast; c++)
            {
                tempArr[c - colFirst] = matrix[r, c];
            }

            for (int i = tempArr.Length - 1; i >= 0; i--)
            {
                if (tempArr[i] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (tempArr[i] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, colFirst + i, r, colFirst + i + blankTiles);
                        }
                    }
                }
            }
            blankTiles = 0;
        }
    }

    #endregion
}
