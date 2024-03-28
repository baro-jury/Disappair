using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;
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

    void Start()
    {
        //levelData = JsonConvert.DeserializeObject<LevelData>((Resources.Load("Levels/Level_" + LevelController.level) as TextAsset).text);
        _GoToProcess(1);
    }

    #region Khởi tạo level
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

    void _GoToProcess(int order)
    {
        buttonList.Clear();
        buttonListWithoutBlocker.Clear();
        for (int i = 0; i < gameObject.transform.childCount - 3; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        orderOfPullingDirection = 0;
        _InstantiateProcess(order);
        dict = SpriteController.instance._CreateSubDictionary(minId, maxId);
        _GenerateTiles();
        _SpreadTiles();
        GameplayController.instance.pause.transform.SetAsLastSibling();
        TutorialController.instance.tutorialPanel.transform.SetAsLastSibling();
        TutorialController.instance.connectFailTutorial.transform.SetAsLastSibling();
    }
    #endregion

    #region Tạo Tiles
    void _GenerateTiles()
    {
        int repeat = 0, index = minId;
        SpriteController.instance._ShuffleImage(dict);

        float sideTile = gameObject.GetComponent<RectTransform>().rect.width / column;
        float temp = gameObject.GetComponent<RectTransform>().rect.height / row;
        sideTile = sideTile < temp ? (int)sideTile : (int)temp;
        if (sideMax < sideTile)
        {
            sideTile = sideMax;
        }
        else if (sideTile < sideMin)
        {
            sideTile = sideMin;
        }
        if (sideTile % 2 != 0) sideTile -= 1;
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
                    Vector3 tilePos = _ConvertMatrixIndexToLocalPos(r, c, boardWidth, boardHeight, sideTile);
                    var objBtn = Instantiate(Tile, tilePos / 40, Quaternion.identity, gameObject.transform);
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
                    objBtn.name = objBtn.Id.ToString() + " - " + objBtn.Index.ToString();
                    objBtn.transform.localPosition = tilePos;
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
        GameplayController.instance._EnableSupporter(false);
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
                    .OnComplete(() =>
                    {
                        GameplayController.instance._EnableSupporter(true);
                        switch (levelData.Level)
                        {
                            case 1:
                                TutorialController.instance._ChangeObjectState();
                                TutorialController.instance._FocusOnCoupleTile(
                                    TutorialController.instance._FindTransform(buttonListWithoutBlocker, TutorialController.coupleIndex[TutorialController.order].Item1),
                                    TutorialController.instance._FindTransform(buttonListWithoutBlocker, TutorialController.coupleIndex[TutorialController.order].Item2));
                                break;
                            case 3:
                                TutorialController.instance.tutorialPanel.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(1).gameObject.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
                                {
                                    TutorialController.instance.tutorialPanel.SetActive(false);
                                    TutorialController.instance.tutorialPanel.transform.GetChild(1).gameObject.SetActive(false);
                                });
                                break;
                            case 5:
                                TutorialController.instance.tutorialPanel.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(2).gameObject.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
                                {
                                    TutorialController.instance.tutorialPanel.SetActive(false);
                                    TutorialController.instance.tutorialPanel.transform.GetChild(2).gameObject.SetActive(false);
                                });
                                break;
                            case 7:
                                TutorialController.instance.tutorialPanel.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(3).gameObject.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
                                {
                                    TutorialController.instance.tutorialPanel.SetActive(false);
                                    TutorialController.instance.tutorialPanel.transform.GetChild(3).gameObject.SetActive(false);
                                });
                                break;
                            case 9:
                                TutorialController.instance.tutorialPanel.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(4).gameObject.SetActive(true);
                                TutorialController.instance.tutorialPanel.transform.GetChild(4).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
                                {
                                    TutorialController.instance.tutorialPanel.SetActive(false);
                                    TutorialController.instance.tutorialPanel.transform.GetChild(4).gameObject.SetActive(false);
                                });
                                break;
                        }
                    });
            }
        }
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
            goto shuffle;
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
                        if (GameplayController.instance._HasAvailableConnection(temp[i], temp[j]))
                        {
                            canConnect = true;
                            goto shuffle;
                        }
                    }
                }
            }
        }
    shuffle:
        if (!canConnect)
        {
            _RearrangeTiles();
        }
    }

    public void _RearrangeTiles()
    {
        GameplayController.instance._EnableSupporter(false);
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
                    .OnComplete(() => { GameplayController.instance._EnableSupporter(true); });
            }
        }
    }

    List<int> Shuffle(int length)
    {
        List<int> temp = new();
        temp.Add(0);
        for (int i = 1; i < length; i++)
        {
            temp.Insert(UnityEngine.Random.Range(0, i), i);
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
                if (GameplayController.instance._HasAvailableConnection(buttonListWithoutBlocker[i].transform, buttonListWithoutBlocker[j].transform))
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
        matrix[t.Index.Item1, t.Index.Item2] = "";
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
                if (levelData.Level < Resources.LoadAll("Levels").Length && levelData.Level == PlayerPrefsController.instance._GetMarkedLevel())
                {
                    //LevelController.level = levelData.Level + 1;
                    PlayerPrefsController.instance._SetMarkedLevel(levelData.Level + 1);
                }
                GameplayController.instance._CompleteLevel();
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
    public void _SearchAndPullTile(int rowBefore, int colBefore, int rowAfter, int colAfter) // tìm Tile theo index trong ma tran
    {
        matrix[rowAfter, colAfter] = matrix[rowBefore, colBefore];
        matrix[rowBefore, colBefore] = "";
        foreach (var t in buttonListWithoutBlocker)
        {
            if (t.Index == (rowBefore, colBefore))
            {
                t.Index = (rowAfter, colAfter);
                t.transform.DOLocalMove(_ConvertMatrixIndexToLocalPos(rowAfter, colAfter, column * Tile.Size, row * Tile.Size, Tile.Size), 0.2f)
                    .SetEase(Ease.InOutQuad).SetUpdate(true);
                t.name = t.Id.ToString() + " - " + t.Index.ToString();

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
        //var tempArr = new string[rowLast - rowFirst + 1];
        int blankTiles = 0;
        for (int c = 0; c < column; c++)
        {
            //for (int r = rowFirst; r <= rowLast; r++)
            //{
            //    tempArr[r - rowFirst] = matrix[r, c];
            //}

            //for (int i = tempArr.Length - 1; i >= 0; i--)
            //{
            //    if (tempArr[i] == "")
            //    {
            //        blankTiles++;
            //    }
            //    else
            //    {
            //        if (blankTiles != 0)
            //        {
            //            if (tempArr[i] == "0")
            //            {
            //                blankTiles = 0;
            //            }
            //            else
            //            {
            //                _SearchAndPullTile(rowFirst + i, c, rowFirst + i + blankTiles, c);
            //            }
            //        }
            //    }
            //}

            for (int r = rowLast; r >= rowFirst; r--)
            {
                if (matrix[r, c] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (matrix[r, c] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, c, r + blankTiles, c);
                        }
                    }
                }
            }

            blankTiles = 0;
        }
    }

    void _PullTilesToTop(int rowFirst, int rowLast)
    {
        //var tempArr = new string[rowLast - rowFirst + 1];
        int blankTiles = 0;

        for (int c = 0; c < column; c++)
        {
            //for (int r = rowFirst; r <= rowLast; r++)
            //{
            //    tempArr[r - rowFirst] = matrix[r, c];
            //}

            //for (int i = 0; i < tempArr.Length; i++)
            //{
            //    if (tempArr[i] == "")
            //    {
            //        blankTiles++;
            //    }
            //    else
            //    {
            //        if (blankTiles != 0)
            //        {
            //            if (tempArr[i] == "0")
            //            {
            //                blankTiles = 0;
            //            }
            //            else
            //            {
            //                _SearchAndPullTile(rowFirst + i, c, rowFirst + i - blankTiles, c);
            //            }
            //        }
            //    }
            //}

            for (int r = rowFirst; r <= rowLast; r++)
            {
                if (matrix[r, c] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (matrix[r, c] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, c, r - blankTiles, c);
                        }
                    }
                }
            }

            blankTiles = 0;
        }
    }

    void _PullTilesToLeft(int colFirst, int colLast)
    {
        //var tempArr = new string[colLast - colFirst + 1];
        int blankTiles = 0;

        for (int r = 0; r < row; r++)
        {
            //for (int c = colFirst; c <= colLast; c++)
            //{
            //    tempArr[c - colFirst] = matrix[r, c];
            //}

            //for (int i = 0; i < tempArr.Length; i++)
            //{
            //    if (tempArr[i] == "")
            //    {
            //        blankTiles++;
            //    }
            //    else
            //    {
            //        if (blankTiles != 0)
            //        {
            //            if (tempArr[i] == "0")
            //            {
            //                blankTiles = 0;
            //            }
            //            else
            //            {
            //                _SearchAndPullTile(r, colFirst + i, r, colFirst + i - blankTiles);
            //            }
            //        }
            //    }
            //}

            for (int c = colFirst; c <= colLast; c++)
            {
                if (matrix[r, c] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (matrix[r, c] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, c, r, c - blankTiles);
                        }
                    }
                }
            }

            blankTiles = 0;
        }
    }

    void _PullTilesToRight(int colFirst, int colLast)
    {
        //var tempArr = new string[colLast - colFirst + 1];
        int blankTiles = 0;

        for (int r = 0; r < row; r++)
        {
            //for (int c = colFirst; c <= colLast; c++)
            //{
            //    tempArr[c - colFirst] = matrix[r, c];
            //}

            //for (int i = tempArr.Length - 1; i >= 0; i--)
            //{
            //    if (tempArr[i] == "")
            //    {
            //        blankTiles++;
            //    }
            //    else
            //    {
            //        if (blankTiles != 0)
            //        {
            //            if (tempArr[i] == "0")
            //            {
            //                blankTiles = 0;
            //            }
            //            else
            //            {
            //                _SearchAndPullTile(r, colFirst + i, r, colFirst + i + blankTiles);
            //            }
            //        }
            //    }
            //}

            for (int c = colLast; c >= colFirst; c--)
            {
                if (matrix[r, c] == "")
                {
                    blankTiles++;
                }
                else
                {
                    if (blankTiles != 0)
                    {
                        if (matrix[r, c] == "0")
                        {
                            blankTiles = 0;
                        }
                        else
                        {
                            _SearchAndPullTile(r, c, r, c + blankTiles);
                        }
                    }
                }
            }

            blankTiles = 0;
        }
    }

    #endregion
}

