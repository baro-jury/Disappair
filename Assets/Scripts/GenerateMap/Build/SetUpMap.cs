using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using static UnityEngine.UI.Dropdown;
using UnityEngine.SceneManagement;

public class SetUpMap : MonoBehaviour
{
    public static SetUpMap instance;
    public AudioSource audioSource;

    private LevelData levelData = new LevelData();
    private List<ProcessData> map = new List<ProcessData>();
    private List<(int, ProcessData)> mapContainExistedShape = new List<(int, ProcessData)>();
    private ProcessData dataMap = new ProcessData();
    private List<string[,]> shapeList = new List<string[,]>();
    private List<OptionData> tileImages = new List<OptionData>();
    private int indexMap = 0, indexShape = 0;
    private bool setIngame, setMoreProp;
    private string[,] matrix;

    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject preSettingPanel;
    [SerializeField]
    private Toggle editCreatedLv, shuffleTile;
    [SerializeField]
    private InputField createdLv, level, process;
    [SerializeField]
    private Dropdown difficulty, theme, themeWhileEditing;

    [SerializeField]
    private InputField time, timestampFor2Star, timestampFor3Star;

    [SerializeField]
    private GameObject ingameSettingPanel, additionalPanel;
    [SerializeField]
    private InputField minId, maxId, totalTile, row, column;
    [SerializeField]
    private Dropdown pullDown, pullUp, pullLeft, pullRight;

    [SerializeField]
    private TextMeshProUGUI titleLv;
    [SerializeField]
    private GameObject optionPanel;

    public Button btNone, btRandom, btBlock;
    [SerializeField]
    private Dropdown ddImage;

    [SerializeField]
    private Button btPrevMap, btNextMap, btPrevShape, btNextShape, btRotateShapeToLeft, btRotateShapeToRight, btPlayTrial;

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
        //List<ProcessData> temp = new List<ProcessData>() { new ProcessData(3, 4, false, false, false, false), new ProcessData(12, 9, true, true, true, true) };
        ////LevelData lv = new LevelData(11, 1, 420, temp);
        ////string json = JsonUtility.ToJson(lv); // <-
        ////LevelData loadLv = JsonUtility.FromJson<LevelData>(json); // <-
        minId.text = 1 + "";
        maxId.text = SpriteController.spritesDict.Count - 1 + "";
        level.text = Resources.LoadAll("Levels").Length + 1 + "";
        _CreateDataTile(1, SpriteController.spritesDict.Count - 1);

        createdLv.onEndEdit.AddListener(delegate { _CheckInput(createdLv, 1, Resources.LoadAll("Levels").Length); });
        level.onEndEdit.AddListener(delegate { _CheckInput(level, Resources.LoadAll("Levels").Length + 1, int.MaxValue); });
        process.onEndEdit.AddListener(delegate { _CheckInput(process); });

        btPrevMap.onClick.AddListener(delegate { _GoToPreviousProcess(); });
        btNextMap.onClick.AddListener(delegate { _GoToNextProcess(); });
        btPrevShape.onClick.AddListener(delegate { indexShape--; _EditBoard(); });
        btNextShape.onClick.AddListener(delegate { indexShape++; _EditBoard(); });
        btRotateShapeToLeft.onClick.AddListener(delegate { _RotateShape(false); });
        btRotateShapeToRight.onClick.AddListener(delegate { _RotateShape(true); });

        timestampFor2Star.onEndEdit.AddListener(delegate { _EditDataMap(); });
        timestampFor3Star.onEndEdit.AddListener(delegate { _EditDataMap(); });
        time.onEndEdit.AddListener(delegate { _EditDataMap(); });
        themeWhileEditing.onValueChanged.AddListener(delegate { _EditDataMap(); });
        minId.onEndEdit.AddListener(delegate
        {
            _CheckInput(minId, 1, int.Parse(maxId.text) - 1);
            _CreateDataTile(int.Parse(minId.text), int.Parse(maxId.text));
            _ScanMatrix();
        });
        maxId.onEndEdit.AddListener(delegate
        {
            _CheckInput(maxId, 2, SpriteController.spritesDict.Count - 1);
            _CreateDataTile(int.Parse(minId.text), int.Parse(maxId.text));
            _ScanMatrix();
        });
        shuffleTile.onValueChanged.AddListener(delegate { if (!preSettingPanel.activeInHierarchy) _EditDataMap(); });
        totalTile.onEndEdit.AddListener(delegate { _CheckEvenInput(totalTile); indexShape = 0; _EditBoard(); });
        row.onEndEdit.AddListener(delegate { _EditBoard(); });
        column.onEndEdit.AddListener(delegate { _EditBoard(); });
        pullDown.onValueChanged.AddListener(delegate { if (!preSettingPanel.activeInHierarchy) _EditDataMap(); });
        pullUp.onValueChanged.AddListener(delegate { if (!preSettingPanel.activeInHierarchy) _EditDataMap(); });
        pullLeft.onValueChanged.AddListener(delegate { if (!preSettingPanel.activeInHierarchy) _EditDataMap(); });
        pullRight.onValueChanged.AddListener(delegate { if (!preSettingPanel.activeInHierarchy) _EditDataMap(); });

        btPlayTrial.onClick.AddListener(delegate { _PlayTrial(); });
    }

    #region Setup
    void _SelectBackground(Dropdown drop)
    {
        bool hasBackground = false;
        for (int i = 0; i < background.transform.childCount; i++)
        {
            if (drop.value - 1 != i)
            {
                background.transform.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                background.transform.GetChild(i).gameObject.SetActive(true);
                hasBackground = true;
            }
        }
        if (hasBackground)
        {
            GameObject bgr = background.transform.GetChild(drop.value - 1).gameObject;
            SpriteRenderer sr = bgr.GetComponent<SpriteRenderer>();
            float height = sr.bounds.size.y;
            float width = sr.bounds.size.x;
            float scaleHeight = Camera.main.orthographicSize * 2f;
            float scaleWidth = scaleHeight * Screen.width / Screen.height;
            bgr.transform.localScale = new Vector3(scaleWidth / width, scaleHeight / height, 0);
        }
    }

    void _GenerateMap(ProcessData data)
    {
        SetUpBoard.levelData = levelData;
        SetUpBoard.processData = new ProcessData(data.MinID, data.MaxID, data.TotalTile, data.Shuffle,
            data.Row, data.Column, data.Matrix, data.PullDown, data.PullUp, data.PullLeft, data.PullRight);
        SetUpBoard.instance._StartCreating();
    }

    void _AdjustButtonSwitchMap()
    {
        if (map.Count > 1)
        {
            btPlayTrial.interactable = false;
            if (indexMap == 0)
            {
                btPrevMap.gameObject.SetActive(false);
                btNextMap.gameObject.SetActive(true);
            }
            else if (indexMap == map.Count - 1)
            {
                btPrevMap.gameObject.SetActive(true);
                btNextMap.gameObject.SetActive(false);
                btPlayTrial.interactable = true;
            }
            else
            {
                btPrevMap.gameObject.SetActive(true);
                btNextMap.gameObject.SetActive(true);
            }
        }
        else
        {
            btPrevMap.gameObject.SetActive(false);
            btNextMap.gameObject.SetActive(false);
            btPlayTrial.interactable = true;
        }
    }

    void _CreateDataTile(int min, int max)
    {
        tileImages.Clear();
        ddImage.ClearOptions();
        tileImages.Add(new OptionData("", btNone.transform.GetChild(1).GetComponent<Image>().sprite));
        for (int i = min; i <= max; i++)
        {
            tileImages.Add(new OptionData(i.ToString(), SpriteController.spritesDict[i.ToString()]));
        }
        ddImage.AddOptions(tileImages);
    }

    void _ScanMatrix()
    {
        bool isChanged = false;
        for (int r = 0; r < int.Parse(row.text); r++)
        {
            for (int c = 0; c < int.Parse(column.text); c++)
            {
                if (matrix[r, c] != null && matrix[r, c] != "" && matrix[r, c] != "?" && matrix[r, c] != "0")
                {
                    if (int.Parse(matrix[r, c]) < int.Parse(minId.text) || int.Parse(matrix[r, c]) > int.Parse(maxId.text))
                    {
                        matrix[r, c] = "?";
                        isChanged = true;
                    }
                }
            }
        }
        _EditDataMap();
        if (isChanged) _GenerateMap(dataMap);
    }

    void _CheckShape(ProcessData data, List<string[,]> listShape)
    {
        string path = Application.dataPath + "/Resources/Shapes/" + data.TotalTile + "/" + data.Row + "x" + data.Column + ".json";
        StreamReader reader = new(path);
        string temp = reader.ReadToEnd();
        reader.Close();
        listShape = JsonConvert.DeserializeObject<List<string[,]>>(temp);
        mapContainExistedShape.Add((_FindIndexShape(data.Matrix, listShape), data));
    }

    public void _SetBaseProperties()
    {
        map.Clear();
        mapContainExistedShape.Clear();
        indexMap = 0;
        indexShape = 0;
        if (editCreatedLv.isOn)
        {
            var temp = Resources.Load("Levels/Level_" + createdLv.text) as TextAsset;
            //var temp = Resources.Load("demo") as TextAsset;
            levelData = JsonConvert.DeserializeObject<LevelData>(temp.text);
            time.text = levelData.Time[0] + "";
            timestampFor2Star.text = levelData.Time[2] + "";
            timestampFor3Star.text = levelData.Time[3] + "";
            themeWhileEditing.value = levelData.Theme;

            foreach (var item in levelData.Process) map.Add(item);

            dataMap = new ProcessData(map[indexMap].MinID, map[indexMap].MaxID, map[indexMap].TotalTile, map[indexMap].Shuffle, map[indexMap].Row, map[indexMap].Column,
                map[indexMap].Matrix, map[indexMap].PullDown, map[indexMap].PullUp, map[indexMap].PullLeft, map[indexMap].PullRight);

            minId.text = dataMap.MinID + "";
            maxId.text = dataMap.MaxID + "";
            shuffleTile.isOn = dataMap.Shuffle;
            totalTile.text = dataMap.TotalTile + "";
            row.text = dataMap.Row + "";
            column.text = dataMap.Column + "";
            matrix = dataMap.Matrix;
            pullDown.value = dataMap.PullDown == true ? 1 : 0;
            pullUp.value = dataMap.PullUp == true ? 1 : 0;
            pullLeft.value = dataMap.PullLeft == true ? 1 : 0;
            pullRight.value = dataMap.PullRight == true ? 1 : 0;

            string path = Application.dataPath + "/Resources/Shapes/" + dataMap.TotalTile + "/" + dataMap.Row + "x" + dataMap.Column + ".json";
            StreamReader reader = new(path);
            string data = reader.ReadToEnd();
            reader.Close();
            shapeList = JsonConvert.DeserializeObject<List<string[,]>>(data);
            indexShape = _FindIndexShape(matrix, shapeList);
            mapContainExistedShape.Add((indexShape, dataMap));
            List<string[,]> list = new List<string[,]>();
            for (int i = 1; i < map.Count; i++)
            {
                _CheckShape(map[i], list);
            }
        }
        else
        {
            levelData.Level = int.Parse(level.text);
            levelData.Difficulty = difficulty.options[difficulty.value].text;
            levelData.Theme = theme.value;
            matrix = new string[1, 1];
            dataMap = new ProcessData(1, SpriteController.spritesDict.Count - 1, 0, false, 1, 1, matrix, false, false, false, false);
            for (int i = 0; i < int.Parse(process.text); i++)
            {
                map.Add(dataMap);
                mapContainExistedShape.Add((indexShape, map[i]));
            }
            themeWhileEditing.value = levelData.Theme;
        }
        _EnableSwitchShape();
        SetUpBoard.setup = this;
        _GenerateMap(dataMap);
        _SelectBackground(themeWhileEditing);

        titleLv.text = "LEVEL " + levelData.Level;
        preSettingPanel.SetActive(false);
        _AdjustButtonSwitchMap();
    }

    public void _GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void _ChangeSetting()
    {
        preSettingPanel.SetActive(true);
    }

    public void _SetProperties()
    {
        setIngame = ingameSettingPanel.activeInHierarchy;
        setIngame = !setIngame;
        ingameSettingPanel.SetActive(setIngame);
    }

    public void _AdditionalSetting()
    {
        setMoreProp = additionalPanel.activeInHierarchy;
        setMoreProp = !setMoreProp;
        additionalPanel.SetActive(setMoreProp);
    }

    #endregion

    #region Edit Map
    void _EditBoard()
    {
        _CheckInput(row, 1, 12);
        _CheckInput(column, 1, 9);
        _CheckInput(totalTile, 0, int.Parse(row.text) * int.Parse(column.text));

        _EditShape();
        _EditDataMap();
        _GenerateMap(dataMap);
    }

    void _EditDataMap()
    {
        _CheckInput(row, 1, 12);
        _CheckInput(column, 1, 9);
        _CheckInput(totalTile, 0, int.Parse(row.text) * int.Parse(column.text));
        _CheckInput(time);
        _CheckInput(timestampFor3Star, 0, int.Parse(time.text));
        _CheckInput(timestampFor2Star, 0, int.Parse(timestampFor3Star.text));

        levelData.Time[0] = float.Parse(time.text);
        levelData.Time[1] = 0;
        levelData.Time[2] = float.Parse(timestampFor2Star.text);
        levelData.Time[3] = float.Parse(timestampFor3Star.text);
        levelData.Theme = themeWhileEditing.value;
        dataMap.MinID = int.Parse(minId.text);
        dataMap.MaxID = int.Parse(maxId.text);
        dataMap.TotalTile = int.Parse(totalTile.text);
        dataMap.Shuffle = shuffleTile.isOn;
        dataMap.Row = int.Parse(row.text);
        dataMap.Column = int.Parse(column.text);
        dataMap.Matrix = matrix;
        dataMap.PullDown = bool.Parse(pullDown.options[pullDown.value].text);
        dataMap.PullUp = bool.Parse(pullUp.options[pullUp.value].text);
        dataMap.PullLeft = bool.Parse(pullLeft.options[pullLeft.value].text);
        dataMap.PullRight = bool.Parse(pullRight.options[pullRight.value].text);

        _SelectBackground(themeWhileEditing);
    }

    #endregion

    #region Edit Tile
    public void _EditTile(Transform currentTile)
    {
        float tileSize = currentTile.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        int[] temp = SetUpBoard.instance._ConvertPositionToMatrixIndex(
            currentTile.localPosition.x, currentTile.localPosition.y,
            tileSize * dataMap.Column, tileSize * dataMap.Row, tileSize);

        optionPanel.SetActive(true);

        btNone.onClick.RemoveAllListeners();
        btRandom.onClick.RemoveAllListeners();
        btBlock.onClick.RemoveAllListeners();
        ddImage.onValueChanged.RemoveAllListeners();
        ddImage.value = 0;

        if (matrix[temp[0], temp[1]] == null || matrix[temp[0], temp[1]] == "" || matrix[temp[0], temp[1]] == "0")
        {
            btNone.onClick.AddListener(delegate
            {
                _NoneTile(currentTile);
                matrix[temp[0], temp[1]] = "";
            });
            btRandom.onClick.AddListener(delegate
            {
                dataMap.TotalTile++;
                _RandomTile(currentTile);
                matrix[temp[0], temp[1]] = "?";
            });
            btBlock.onClick.AddListener(delegate
            {
                _BlockTile(currentTile);
                matrix[temp[0], temp[1]] = "0";
            });
            ddImage.onValueChanged.AddListener(delegate
            {
                dataMap.TotalTile++;
                _ChooseTile(currentTile);
                matrix[temp[0], temp[1]] = ddImage.options[ddImage.value].text;
            });
        }
        else
        {
            btNone.onClick.AddListener(delegate
            {
                dataMap.TotalTile--;
                _NoneTile(currentTile);
                matrix[temp[0], temp[1]] = "";
            });
            btRandom.onClick.AddListener(delegate
            {
                _RandomTile(currentTile);
                matrix[temp[0], temp[1]] = "?";
            });
            btBlock.onClick.AddListener(delegate
            {
                dataMap.TotalTile--;
                _BlockTile(currentTile);
                matrix[temp[0], temp[1]] = "0";
            });
            ddImage.onValueChanged.AddListener(delegate
            {
                _ChooseTile(currentTile);
                matrix[temp[0], temp[1]] = ddImage.options[ddImage.value].text;
            });
        }
    }

    void _NoneTile(Transform currentTile)
    {
        totalTile.text = dataMap.TotalTile + "";
        currentTile.GetChild(1).GetComponent<Image>().sprite = btNone.transform.GetChild(1).GetComponent<Image>().sprite;
        optionPanel.SetActive(false);
    }

    void _BlockTile(Transform currentTile)
    {
        totalTile.text = dataMap.TotalTile + "";
        currentTile.GetChild(1).GetComponent<Image>().sprite = btBlock.transform.GetChild(1).GetComponent<Image>().sprite;
        optionPanel.SetActive(false);
    }

    void _RandomTile(Transform currentTile)
    {
        totalTile.text = dataMap.TotalTile + "";
        currentTile.GetChild(1).GetComponent<Image>().sprite = btRandom.transform.GetChild(1).GetComponent<Image>().sprite;
        optionPanel.SetActive(false);
    }

    void _ChooseTile(Transform currentTile)
    {
        totalTile.text = dataMap.TotalTile + "";
        currentTile.GetChild(1).GetComponent<Image>().sprite = ddImage.options[ddImage.value].image;
        optionPanel.SetActive(false);
    }

    #endregion

    #region Validation
    void _CheckInput(InputField input)
    {
        if (input.text == "" || int.Parse(input.text) <= 0)
        {
            input.text = "1";
        }
    }

    void _CheckInput(InputField input, int minValue, int maxValue)
    {
        if (input.text == "" || int.Parse(input.text) < minValue)
        {
            input.text = minValue + "";
        }
        if (int.Parse(input.text) > maxValue)
        {
            input.text = maxValue + "";
        }
    }

    void _CheckEvenInput(InputField input)
    {
        if (int.Parse(input.text) % 2 != 0)
        {
            input.text = int.Parse(input.text) - 1 + "";
        }
    }

    void _CheckMatrix()
    {
        for (int r = 0; r < matrix.GetLength(0); r++)
        {
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                if (matrix[r, c] == null)
                {
                    matrix[r, c] = "";
                }
            }
        }
    }

    #endregion

    #region Shape
    bool _SuitableShape()
    {
        return map[indexMap].TotalTile == int.Parse(totalTile.text)
            && map[indexMap].Row == int.Parse(row.text)
            && map[indexMap].Column == int.Parse(column.text);
    }

    void _EditShape()
    {
        string folderName = Application.dataPath + "/Resources/Shapes/" + totalTile.text;
        string fullPath = folderName + "/" + row.text + "x" + column.text + ".json";
        if (File.Exists(fullPath))
        {
            //string temp = fullPath[fullPath.IndexOf("Shapes")..];
            string temp = fullPath.Substring(fullPath.IndexOf("Shapes"));
            string path = temp.Remove(temp.IndexOf("."));
            var data = Resources.Load(path) as TextAsset;
            shapeList = JsonConvert.DeserializeObject<List<string[,]>>(data.text);
            if (_SuitableShape() && indexShape == mapContainExistedShape[indexMap].Item1)
            {
                matrix = map[indexMap].Matrix;
            }
            else
            {
                matrix = shapeList[indexShape];
            }
        }
        else
        {
            shapeList = new();
            if (_SuitableShape())
            {
                matrix = map[indexMap].Matrix;
            }
            else
            {
                matrix = new string[int.Parse(row.text), int.Parse(column.text)];
                int temp = 0;
                for (int r = 0; r < int.Parse(row.text); r++)
                {
                    for (int c = 0; c < int.Parse(column.text); c++)
                    {
                        if (temp < int.Parse(totalTile.text))
                        {
                            matrix[r, c] = "?";
                            temp++;
                        }
                    }
                }
            }
        }


        _EnableSwitchShape();
    }

    void _EnableSwitchShape()
    {
        if (shapeList.Count > 1)
        {
            if (indexShape == 0)
            {
                btPrevShape.interactable = false;
                btNextShape.interactable = true;
            }
            else if (indexShape == shapeList.Count - 1)
            {
                btPrevShape.interactable = true;
                btNextShape.interactable = false;
            }
            else
            {
                btPrevShape.interactable = true;
                btNextShape.interactable = true;
            }
        }
        else
        {
            btPrevShape.interactable = false;
            btNextShape.interactable = false;
        }
    }

    void _RotateShape(bool rotateRight)
    {
        int oldRow = matrix.GetLength(0);
        int oldCol = matrix.GetLength(1);
        string[,] temp = new string[oldCol, oldRow];
        if (rotateRight)
        {
            for (int c = 0; c < oldCol; c++)
            {
                for (int r = 0; r < oldRow; r++)
                {
                    temp[c, r] = matrix[oldRow - 1 - r, c];
                }
            }
        }
        else
        {
            for (int c = 0; c < oldCol; c++)
            {
                for (int r = 0; r < oldRow; r++)
                {
                    temp[c, r] = matrix[r, oldCol - 1 - c];
                }
            }
        }
        matrix = temp;
        row.text = oldCol + "";
        column.text = oldRow + "";
        _EditDataMap();
        _GenerateMap(dataMap);
    }

    int _FindIndexShape(string[,] shape, List<string[,]> list)
    {
        if (list.Count != 0)
        {
            foreach (var item in list)
            {
                if (_AreTheSameShapes(shape, item)) return list.IndexOf(item);
            }
        }
        return 0;
    }

    void _StoreShapes(ProcessData data)
    {
        //string[,] shape = data.Matrix;
        string[,] shape = new string[data.Row, data.Column];
        for (int r = 0; r < data.Row; r++)
        {
            for (int c = 0; c < data.Column; c++)
            {
                shape[r, c] = data.Matrix[r, c];
            }
        }
        for (int r = 0; r < data.Row; r++)
        {
            for (int c = 0; c < data.Column; c++)
            {
                if (shape[r, c] != "" && shape[r, c] != "0")
                {
                    shape[r, c] = "?";
                }
            }
        }

        string folderName = Application.dataPath + "/Resources/Shapes/" + data.TotalTile;
        string fullPath = Path.Combine(folderName, data.Row + "x" + data.Column + ".json");

        if (File.Exists(fullPath))
        {
            StreamReader reader = new(fullPath);
            string temp = reader.ReadToEnd();
            reader.Close();
            List<string[,]> shapeList = JsonConvert.DeserializeObject<List<string[,]>>(temp);
            bool shapeDuplicated = false, rndTileShapeDuplicated = false;
            foreach (var item in shapeList)
            {
                if (_AreTheSameShapes(data.Matrix, item)) shapeDuplicated = true;
                if (_AreTheSameShapes(shape, item)) rndTileShapeDuplicated = true;
            }
            if (!shapeDuplicated) shapeList.Add(data.Matrix);
            if (!rndTileShapeDuplicated)
            {
                if(!_AreTheSameShapes(data.Matrix, shape)) shapeList.Add(shape);
            }
            string json = JsonConvert.SerializeObject(shapeList);
            File.WriteAllText(fullPath, json);
        }
        else
        {
            List<string[,]> temp = new() { shape };
            Directory.CreateDirectory(folderName);
            string json = JsonConvert.SerializeObject(temp);
            File.WriteAllText(fullPath, json);
        }
    }

    bool _AreTheSameShapes(string[,] item1, string[,] item2)
    {
        if (item1.GetLength(0) != item2.GetLength(0) || item1.GetLength(1) != item2.GetLength(1)) return false;
        else
        {
            for (int r = 0; r < item1.GetLength(0); r++)
            {
                for (int c = 0; c < item2.GetLength(1); c++)
                {
                    if (item1[r, c] != item2[r, c])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    #endregion

    #region Interact
    void _GoToNextProcess()
    {
        if (dataMap.TotalTile % 2 == 0)
        {
            _CheckMatrix();
            map[indexMap] = new ProcessData(dataMap.MinID, dataMap.MaxID, dataMap.TotalTile, dataMap.Shuffle, dataMap.Row, dataMap.Column,
                matrix, dataMap.PullDown, dataMap.PullUp, dataMap.PullLeft, dataMap.PullRight);
            mapContainExistedShape[indexMap] = (indexShape, map[indexMap]);
            indexMap++;
            indexShape = mapContainExistedShape[indexMap].Item1;
            _AdjustButtonSwitchMap();

            minId.text = map[indexMap].MinID + "";
            maxId.text = map[indexMap].MaxID + "";
            shuffleTile.isOn = map[indexMap].Shuffle;
            totalTile.text = map[indexMap].TotalTile + "";
            row.text = map[indexMap].Row + "";
            column.text = map[indexMap].Column + "";
            matrix = map[indexMap].Matrix;
            pullDown.value = map[indexMap].PullDown == true ? 1 : 0;
            pullUp.value = map[indexMap].PullUp == true ? 1 : 0;
            pullLeft.value = map[indexMap].PullLeft == true ? 1 : 0;
            pullRight.value = map[indexMap].PullRight == true ? 1 : 0;
            _EditBoard();

            Debug.Log("Tien trinh " + (indexMap + 1));
        }
        else
        {
            Debug.Log("Chỉnh tổng ô về số chẵn đi đã !!!");
        }

    }

    void _GoToPreviousProcess()
    {
        if (dataMap.TotalTile % 2 == 0)
        {
            _CheckMatrix();
            map[indexMap] = new ProcessData(dataMap.MinID, dataMap.MaxID, dataMap.TotalTile, dataMap.Shuffle, dataMap.Row, dataMap.Column,
                matrix, dataMap.PullDown, dataMap.PullUp, dataMap.PullLeft, dataMap.PullRight);
            mapContainExistedShape[indexMap] = (indexShape, map[indexMap]);
            indexMap--;
            indexShape = mapContainExistedShape[indexMap].Item1;
            _AdjustButtonSwitchMap();

            minId.text = map[indexMap].MinID + "";
            maxId.text = map[indexMap].MaxID + "";
            shuffleTile.isOn = map[indexMap].Shuffle;
            totalTile.text = map[indexMap].TotalTile + "";
            row.text = map[indexMap].Row + "";
            column.text = map[indexMap].Column + "";
            matrix = map[indexMap].Matrix;
            pullDown.value = map[indexMap].PullDown == true ? 1 : 0;
            pullUp.value = map[indexMap].PullUp == true ? 1 : 0;
            pullLeft.value = map[indexMap].PullLeft == true ? 1 : 0;
            pullRight.value = map[indexMap].PullRight == true ? 1 : 0;
            _EditBoard();

            Debug.Log("Tien trinh " + (indexMap + 1));
        }
        else
        {
            Debug.Log("Chỉnh tổng ô về số chẵn đi đã !!!");
        }
    }

    public void _Complete()
    {
        if (dataMap.TotalTile % 2 == 0)
        {
            _CheckMatrix();
            map[indexMap] = new ProcessData(dataMap.MinID, dataMap.MaxID, dataMap.TotalTile, dataMap.Shuffle, dataMap.Row, dataMap.Column,
                matrix, dataMap.PullDown, dataMap.PullUp, dataMap.PullLeft, dataMap.PullRight);
            levelData.Process = map;
            foreach (var item in map)
            {
                _StoreShapes(item);
                Debug.Log("Level " + levelData.Level + " - " + "Map " + (map.IndexOf(item) + 1) + " - " + item.TotalTile + " tiles");
            }
            string json = JsonConvert.SerializeObject(levelData);
            File.WriteAllText(Application.dataPath + "/Resources/Levels/Level_" + levelData.Level + ".json", json);
            //File.WriteAllText(Application.dataPath + "/Resources/demo.json", json);

            Debug.Log("Done");
        }
        else
        {
            Debug.Log("Chỉnh tổng ô về số chẵn đi đã !!!");
        }
    }

    void _PlayTrial()
    {
        SceneManager.LoadScene("PlayTrial");
    }

    #endregion
}
