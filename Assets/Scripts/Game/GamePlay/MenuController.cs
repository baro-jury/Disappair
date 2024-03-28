using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [SerializeField]
    private GameObject menuPanel, settingPanel, guidePanel;
    [SerializeField]
    private Text stars, coins;
    [SerializeField]
    private TextMeshProUGUI level;
    [SerializeField]
    private AudioClip clickButtonClip, switchClip;

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
        stars.text = PlayerPrefsController.instance._GetStarsAchieved() + "";
        coins.text = PlayerPrefsController.instance._GetCoinsInPossession() + "";
        level.text = "LEVEL " + PlayerPrefsController.instance._GetMarkedLevel();
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(PlayerPrefsController.instance.audioSource.mute);
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(PlayerPrefsController.instance.musicSource.mute);
    }

    public void _PlayGame()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        LevelController.instance._PlayLevel(PlayerPrefsController.instance._GetMarkedLevel());
    }

    public void _ChooseLevel()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        menuPanel.SetActive(false);
    }

    public void _BackToMenu()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        menuPanel.SetActive(true);
    }

    public void _CreateCustomMap()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene("GenerateMap");
    }

    public void _GoToSetting()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        settingPanel.SetActive(true);
    }

    public void _TurnOffSound()
    {
        PlayerPrefsController.instance.audioSource.mute = true;
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
        //              setting form         body        middle      sound on   sound off
    }

    public void _TurnOnSound()
    {
        PlayerPrefsController.instance.audioSource.mute = false;
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        //              setting form         body        middle      sound on   sound off
    }

    public void _TurnOffMusic()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        PlayerPrefsController.instance.musicSource.mute = true;
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(true);
        //              setting form         body        middle      music on   music off
    }

    public void _TurnOnMusic()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        PlayerPrefsController.instance.musicSource.mute = false;
        settingPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(false);
        //              setting form         body        middle      music on   music off
    }

    public void _ShowHowToPlay()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        guidePanel.SetActive(true);
    }

    public void _TurnOffGuidePanel()
    {
        guidePanel.SetActive(false);
    }

    public void _TurnOffSettingPanel()
    {
        settingPanel.SetActive(false);
    }

    public void _BuyCoins()
    {
        
    }
}
