using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPrefsController : MonoBehaviour
{
    public static PlayerPrefsController instance;
    public AudioSource audioSource;
    public AudioSource musicSource;
    public AudioSource timeWarningSource;

    private const string PROGRESS = "Progress";
    private const string STARS = "Stars", COINS = "Coins";
    private const string SP1 = "Hints", SP2 = "Wands", SP3 = "Freezers", SP4 = "Shufflers";

    void _MakeSingleInstance()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void _CheckFirstTimePlayGame()
    {
        if (!PlayerPrefs.HasKey("_CheckFirstTimePlayGame"))
        {
            PlayerPrefs.SetInt(PROGRESS, 1);
            PlayerPrefs.SetInt(SP1, 3);
            PlayerPrefs.SetInt(SP2, 3);
            PlayerPrefs.SetInt(SP3, 3);
            PlayerPrefs.SetInt(SP4, 3);
            PlayerPrefs.SetInt("_CheckFirstTimePlayGame", 0);
        }
    }

    void Awake()
    {
        _MakeSingleInstance();
        _CheckFirstTimePlayGame();
        BoardController.levelData = JsonConvert.DeserializeObject<LevelData>((Resources.Load("Levels/Level_" + _GetMarkedLevel()) as TextAsset).text);
        //_SetNumOfHint(100, true);
        //_SetNumOfMagicWand(1000, true);
        //_SetNumOfFreezeTime(100, true);
        //_SetNumOfShuffle(100, true);
    }

    public int _GetMarkedLevel()
    {
        return PlayerPrefs.GetInt(PROGRESS);
    }

    public void _SetMarkedLevel(int level)
    {
        PlayerPrefs.SetInt(PROGRESS, level);
    }

    public int _GetStarsAchieved()
    {
        return PlayerPrefs.GetInt(STARS);
    }

    public void _SetStarsAchieved(int star)
    {
        PlayerPrefs.SetInt(STARS, PlayerPrefs.GetInt(STARS) + star);
    }

    public int _GetCoinsInPossession()
    {
        return PlayerPrefs.GetInt(COINS);
    }

    public void _SetCoinsInPossession(int coin, bool isEarning)
    {
        int temp = -1;
        if (isEarning) temp = 1;
        PlayerPrefs.SetInt(COINS, PlayerPrefs.GetInt(COINS) + temp * coin);
    }

    public int _GetNumOfHint()
    {
        return PlayerPrefs.GetInt(SP1);
    }

    public void _SetNumOfHint(int num, bool isBuying)
    {
        int temp = -1;
        if (isBuying) temp = 1;
        PlayerPrefs.SetInt(SP1, PlayerPrefs.GetInt(SP1) + temp * num);
    }

    public int _GetNumOfMagicWand()
    {
        return PlayerPrefs.GetInt(SP2);
    }

    public void _SetNumOfMagicWand(int num, bool isBuying)
    {
        int temp = -1;
        if (isBuying) temp = 1;
        PlayerPrefs.SetInt(SP2, PlayerPrefs.GetInt(SP2) + temp * num);
    }

    public int _GetNumOfFreezeTime()
    {
        return PlayerPrefs.GetInt(SP3);
    }

    public void _SetNumOfFreezeTime(int num, bool isBuying)
    {
        int temp = -1;
        if (isBuying) temp = 1;
        PlayerPrefs.SetInt(SP3, PlayerPrefs.GetInt(SP3) + temp * num);
    }

    public int _GetNumOfShuffle()
    {
        return PlayerPrefs.GetInt(SP4);
    }

    public void _SetNumOfShuffle(int num, bool isBuying)
    {
        int temp = -1;
        if (isBuying) temp = 1;
        PlayerPrefs.SetInt(SP4, PlayerPrefs.GetInt(SP4) + temp * num);
    }
}
