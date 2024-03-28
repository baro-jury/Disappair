using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;
    //public static int level = 7;

    [SerializeField]
    private InputField lvInput;
    [SerializeField]
    private Button btPlay;

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

    //void _LoadDataLevel()
    //{
    //    var dataStr = Resources.Load("Levels") as TextAsset;
    //    dataLevels = JsonConvert.DeserializeObject<List<DataLevels>>(dataStr.text);
    //}

    //public DataLevels _GetDataLevel(int level)
    //{
    //    var temp = dataLevels.Find(l => l.level == level);
    //    if (temp != null)
    //    {
    //        return temp;
    //    }
    //    return default;
    //}

    void Start()
    {
        lvInput.text = BoardController.levelData.Level + "";
        lvInput.onEndEdit.AddListener(delegate { _CheckInput(lvInput); });
        btPlay.onClick.AddListener(delegate { _PlayLevel(int.Parse(lvInput.text)); });
    }

    LevelData _GetLevelData(int level)
    {
        var dataStr = Resources.Load("Levels/Level_" + level) as TextAsset;
        return JsonConvert.DeserializeObject<LevelData>(dataStr.text);
    }

    #region Play
    //public void _PlayLvTest()
    //{
    //    var dataStr = Resources.Load("demo") as TextAsset;
    //    BoardController.levelData = JsonConvert.DeserializeObject<LevelData>(dataStr.text);
    //    SceneManager.LoadScene("GamePlay");
    //}
    public void _PlayLevel(int lv)
    {
        //level = lv;
        BoardController.levelData = _GetLevelData(lv);
        SceneManager.LoadScene("GamePlay");
    }

    void _CheckInput(InputField input)
    {
        if (input.text == "" || int.Parse(input.text) <= 0)
        {
            input.text = "1";
        }
        else if (int.Parse(input.text) > Resources.LoadAll("Levels").Length)
        {
            input.text = Resources.LoadAll("Levels").Length + "";
        }
    }

    #endregion

}
