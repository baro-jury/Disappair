using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelDemo : MonoBehaviour
{
    public static LevelDemo instance;
    public static int level = 20;

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

    void Start()
    {
        lvInput.text = BoardDemo.levelData.Level + "";
        lvInput.onEndEdit.AddListener(delegate { _CheckInput(lvInput); });
        btPlay.onClick.AddListener(delegate { PlayerPrefsDemo.instance.audioSource.Stop(); _PlayLevel(int.Parse(lvInput.text)); });
    }

    LevelData _GetLevelData(int level)
    {
        var dataStr = Resources.Load("Levels/Level_" + level) as TextAsset;
        return JsonConvert.DeserializeObject<LevelData>(dataStr.text);
    }

    public void _PlayLevel(int lv)
    {
        BoardDemo.levelData = _GetLevelData(lv);
        SceneManager.LoadScene("GamePlayDemo");
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

}
