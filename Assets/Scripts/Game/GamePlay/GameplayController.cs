using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;

    public GameObject pause;
    public AudioClip clickButtonClip, matchTileClip, switchClip;
    public AudioClip timeWizardClip, hintClip, magicWandClip, freezeTimeClip, shuffleClip;

    private bool isCoupled = true, isHinted = false;
    private int numberOfStars;
    private Transform[] points = new Transform[4];
    private Transform tile;

    [SerializeField]
    private GameObject boosterPanel, shop;
    [SerializeField]
    private Text titleLv, titleGameover, titleComplete;
    [SerializeField]
    private GameObject pausePanel, quitPanel;
    [SerializeField]
    private Button settingOnButton, settingOffButton;
    [SerializeField]
    private Button btSpHint, btSpMagicWand, btSpFreeze, btSpShuffle;
    [SerializeField]
    private GameObject timeOutPanel, rejectChancePanel, gameOverPanel, winPanel, starsAchieved;
    [SerializeField]
    private Transform tempAlignWithLeft, tempAlignWithRight, tempAlignWithTop, tempAlignWithBottom, tempAlignWithStart, tempAlignWithEnd;

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
        titleLv.text = "LEVEL " + BoardController.levelData.Level;
        titleGameover.text = "LEVEL " + BoardController.levelData.Level;
        titleComplete.text = "LEVEL " + BoardController.levelData.Level;
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(PlayerPrefsController.instance.audioSource.mute);
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(PlayerPrefsController.instance.musicSource.mute);

        Vector3 temp;
        if (BoardController.levelData.Level == 1)
        {
            Time.timeScale = 1;
            TimeController.instance._FreezeTime(true);
        }
        else Time.timeScale = 0;

        if (BoardController.levelData.Level < 3) btSpHint.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardController.levelData.Level == 3)
        {
            btSpHint.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialController.instance.tutorialPanel.transform.GetChild(1).localPosition;
            TutorialController.instance.tutorialPanel.transform.GetChild(1).localPosition = new Vector3
                (temp.x, btSpHint.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsController.instance._GetNumOfHint() == 0) btSpHint.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfHint() + "";
        }

        if (BoardController.levelData.Level < 5) btSpMagicWand.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardController.levelData.Level == 5)
        {
            btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialController.instance.tutorialPanel.transform.GetChild(2).localPosition;
            TutorialController.instance.tutorialPanel.transform.GetChild(2).localPosition = new Vector3
                (temp.x, btSpMagicWand.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsController.instance._GetNumOfMagicWand() == 0) btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfMagicWand() + "";
        }

        if (BoardController.levelData.Level < 7) btSpFreeze.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardController.levelData.Level == 7)
        {
            btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialController.instance.tutorialPanel.transform.GetChild(3).localPosition;
            TutorialController.instance.tutorialPanel.transform.GetChild(3).localPosition = new Vector3
                (temp.x, btSpFreeze.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsController.instance._GetNumOfFreezeTime() == 0) btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfFreezeTime() + "";
        }

        if (BoardController.levelData.Level < 9) btSpShuffle.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardController.levelData.Level == 9)
        {
            btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialController.instance.tutorialPanel.transform.GetChild(4).localPosition;
            TutorialController.instance.tutorialPanel.transform.GetChild(4).localPosition = new Vector3
                (temp.x, btSpShuffle.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsController.instance._GetNumOfShuffle() == 0) btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfShuffle() + "";
        }
    }

    #region Ingame
    public void _Pause()
    {
        PlayerPrefsController.instance.audioSource.Pause();
        PlayerPrefsController.instance.timeWarningSource.Pause();
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        settingOnButton.interactable = false;
        pausePanel.SetActive(true);
        pausePanel.GetComponent<Button>().interactable = false;
        pausePanel.transform.GetComponent<Image>().DOFade(.8f, .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        settingOffButton.transform.DORotate(new Vector3(0, 0, 0), .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(-80, -85, 0);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector3(-80, -85, 0);
        pausePanel.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector3(-80, -85, 0);

        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosY(-235, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-385, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(2).GetComponent<RectTransform>().DOAnchorPosY(-535, .4f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                settingOffButton.interactable = true;
                pausePanel.GetComponent<Button>().interactable = true;
                pausePanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                pausePanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                pausePanel.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                settingOffButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                PlayerPrefsController.instance.audioSource.Pause();
            });

        Time.timeScale = 0;
    }

    public void _Resume()
    {
        PlayerPrefsController.instance.audioSource.UnPause();
        PlayerPrefsController.instance.timeWarningSource.UnPause();
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        settingOffButton.interactable = false;
        pausePanel.GetComponent<Button>().interactable = false;
        pausePanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        settingOffButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetComponent<Image>().DOFade(0f, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);

        settingOffButton.transform.DORotate(new Vector3(0, 0, 180), .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        quitPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosY(-85, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-85, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(2).GetComponent<RectTransform>().DOAnchorPosY(-85, .4f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                pausePanel.SetActive(false);
                settingOnButton.interactable = true;
                quitPanel.SetActive(false);
                quitPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.one;
            });

        Time.timeScale = 1;
    }

    public void _TurnOffSound()
    {
        PlayerPrefsController.instance.audioSource.mute = true;
        PlayerPrefsController.instance.timeWarningSource.mute = true;
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        //             sound on button   sound off button
        pausePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }

    public void _TurnOnSound()
    {
        PlayerPrefsController.instance.audioSource.mute = false;
        PlayerPrefsController.instance.timeWarningSource.mute = false;
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        //            sound on button    sound off button
        pausePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
    }

    public void _TurnOffMusic()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        PlayerPrefsController.instance.musicSource.mute = true;
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        //           music on button     music off button
        pausePanel.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
    }

    public void _TurnOnMusic()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(switchClip);
        PlayerPrefsController.instance.musicSource.mute = false;
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        //           music on button     music off button
        pausePanel.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
    }

    public void _QuitConfirmation()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        quitPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        quitPanel.SetActive(true);
        quitPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    public void _GoToMenu()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene("MainMenu");
    }

    public void _TakeTheLastChance()
    {
        Time.timeScale = 0;
        //timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        timeOutPanel.SetActive(true);
        //timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .15f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    public void _GetMoreTime()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        if (PlayerPrefsController.instance._GetCoinsInPossession() >= 100)
        {
            PlayerPrefsController.instance._SetCoinsInPossession(100, false);
            TimeController.instance._ResetIconState();
            TimeController.instance._SetTime(60, 1);
            timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                timeOutPanel.SetActive(false);
            });
            TimeController.isSaved = true;
        }
        Time.timeScale = 1;
    }

    public void _WatchAds()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        UnityEvent eReward = new UnityEvent();
        eReward.AddListener(() =>
        {
            // luồng game sau khi tắt quảng cáo ( tặng thưởng cho user )
            TimeController.instance._ResetIconState();
            TimeController.instance._SetTime(15, 1);
            timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                timeOutPanel.SetActive(false);
            });
            TimeController.isSaved = true;
            Time.timeScale = 1;
        });
        ACEPlay.Bridge.BridgeController.instance.ShowRewardedAd(eReward, null);
    }

    public void _RejectChance()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        rejectChancePanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        rejectChancePanel.SetActive(true);
        rejectChancePanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    public void _GameOver()
    {
        Time.timeScale = 0;
        //gameOverPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        gameOverPanel.SetActive(true);
        //gameOverPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    public void _Replay()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void _CompleteLevel()
    {
        Time.timeScale = 0;
        PlayerPrefsController.instance.audioSource.Stop();
        numberOfStars = TimeController.instance._NumberOfStarsAchieved();
        for (int i = 0; i < numberOfStars; i++)
        {
            starsAchieved.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
        winPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        winPanel.SetActive(true);
        winPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);

        PlayerPrefsController.instance._SetStarsAchieved(numberOfStars);
        PlayerPrefsController.instance._SetCoinsInPossession(numberOfStars * 50, true);
    }

    public void _GoToNextLevel()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        if (BoardController.levelData.Level == Resources.LoadAll("Levels").Length)
        {
            _GoToMenu();
        }
        else
        {
            BoardController.levelData.Level++;
            //LevelController.instance._PlayLevel(BoardController.levelData.Level);
            var dataStr = Resources.Load("Levels/Level_" + BoardController.levelData.Level) as TextAsset;
            BoardController.levelData = JsonConvert.DeserializeObject<LevelData>(dataStr.text);
            SceneManager.LoadScene("GamePlay");
        }
    }

    #endregion

    #region Click Tiles
    public void _ClickTile(Transform currentTile)
    {
        Color color = new Color(0, 0.9f, 0.08f, 1);
        //Color color = new Color32(0, 230, 20, 255);
        currentTile.DOComplete();
        currentTile.DOKill();
        if (currentTile.GetComponent<TileController>().Id != 0)
        {
            currentTile.GetComponent<RectTransform>().DOScale(new Vector3(.8f, .8f, 1), .1f).SetEase(Ease.InOutQuad).SetUpdate(true)
               .OnComplete(() =>
               {
                   currentTile.GetComponent<RectTransform>().DOScale(Vector3.one, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                   currentTile.GetChild(0).GetComponent<Image>().DOColor(color, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
               });
        }
        Time.timeScale = 1;
        isCoupled = !isCoupled;
        if (!isCoupled)
        {
            tile = currentTile;
        }
        else
        {
            if (tile.GetComponent<TileController>().Index == currentTile.GetComponent<TileController>().Index)
            {
                Debug.Log("Click cung 1 tile");
                foreach (var tile in BoardController.buttonList)
                {
                    if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                }
                isCoupled = !isCoupled;
            }
            else
            {
                if (tile.GetComponent<TileController>().Id != currentTile.GetComponent<TileController>().Id)
                {
                    Debug.Log("Click khac tile khac ID");
                    foreach (var tile in BoardController.buttonList)
                    {
                        if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                    }
                    tile = currentTile;
                    isCoupled = !isCoupled;
                }
                else
                {
                    if (_HasAvailableConnection(tile, currentTile))
                    {
                        Transform[] temp = new Transform[4];
                        for (int i = 0; i < points.Length; i++)
                        {
                            temp[i] = points[i];
                        }
                        StopAllCoroutines();
                        StartCoroutine(MakeConnection(temp));
                        PlayerPrefsController.instance.audioSource.PlayOneShot(matchTileClip);
                        StartCoroutine(DestroyTiles(temp));
                    }
                    else
                    {
                        if (BoardController.levelData.Level == 1)
                        {
                            Handheld.Vibrate();
                            TutorialController.order++;
                            if (TutorialController.order == 4)
                            {
                                TutorialController.instance.connectFailTutorial.transform.GetChild(0).gameObject.SetActive(true);
                            }
                            else if (TutorialController.order == 5)
                            {
                                TutorialController.instance.connectFailTutorial.transform.GetChild(1).gameObject.SetActive(true);
                            }

                        }
                        Debug.Log("Khong the ket noi");
                        foreach (var tile in BoardController.buttonList)
                        {
                            if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                        }
                        tile = currentTile;
                        isCoupled = !isCoupled;
                    }
                }
            }
        }
    }

    void _ResetTileState()
    {
        foreach (var tile in BoardController.buttonList)
        {
            DOTween.Kill(tile);
            tile.localScale = Vector3.one;
        }
        isHinted = false;
    }

    #endregion

    #region Supporter
    void _BuySupporter(int order)
    {
        Time.timeScale = 0;
        GameObject shopPanel = shop.transform.GetChild(order).gameObject;
        shopPanel.SetActive(true);
        shopPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { Time.timeScale = 1; });
        Button btBuy = shopPanel.transform.GetChild(0).GetChild(5).GetComponent<Button>();
        btBuy.onClick.RemoveAllListeners();
        btBuy.onClick.AddListener(delegate
        {
            PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
            if (PlayerPrefsController.instance._GetCoinsInPossession() >= 290)
            {
                PlayerPrefsController.instance._SetCoinsInPossession(290, false);
                switch (order)
                {
                    case 0:
                        PlayerPrefsController.instance._SetNumOfHint(3, true);
                        btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfHint() + "";
                        break;
                    case 1:
                        PlayerPrefsController.instance._SetNumOfMagicWand(3, true);
                        btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfMagicWand() + "";
                        break;
                    case 2:
                        PlayerPrefsController.instance._SetNumOfFreezeTime(3, true);
                        btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfFreezeTime() + "";
                        PlayerPrefsController.instance.timeWarningSource.UnPause();
                        break;
                    case 3:
                        PlayerPrefsController.instance._SetNumOfShuffle(3, true);
                        btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfShuffle() + "";
                        break;
                }
                shopPanel.SetActive(false);
                Time.timeScale = 1;
            }
        });
    }

    public void _EnableSupporter(bool isEnabled)
    {
        btSpHint.interactable = isEnabled;
        btSpMagicWand.interactable = isEnabled;
        //btSpFreeze.interactable = isEnabled;
        btSpShuffle.interactable = isEnabled;
    }

    public void _SupporterHint() //Hint: Khi sử dụng sẽ gợi ý 1 kết quả
    {
        if (PlayerPrefsController.instance._GetNumOfHint() > 0)
        {
            if (BoardController.levelData.Level != 3)
            {
                PlayerPrefsController.instance._SetNumOfHint(1, false);
                if (PlayerPrefsController.instance._GetNumOfHint() == 0) btSpHint.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfHint() + "";
            }
            PlayerPrefsController.instance.audioSource.PlayOneShot(hintClip);
            _ResetTileState();

            while (!isHinted)
            {
                int index = Random.Range(1, BoardController.dict.Count);
                List<Transform> temp = BoardController.instance._SearchSameTiles(BoardController.dict.ElementAt(index).Value);
                if (temp.Count != 0)
                {
                    for (int i = 0; i < (temp.Count - 1); i++)
                    {
                        for (int j = (i + 1); j < temp.Count; j++)
                        {
                            if (_HasAvailableConnection(temp[i], temp[j]))
                            {
                                isHinted = true;
                                goto endloop;
                            }
                        }
                    }
                endloop:;
                }
            }
            int n = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null)
                {
                    n--;
                }
            }
            for (int i = 0; i < n; i++)
            {
                if (i == 0 || i == n - 1)
                {
                    points[i].DOScale(new Vector3(.8f, .8f, .8f), .5f).SetEase(Ease.InOutQuad).SetUpdate(true).SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
        else
        {
            PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(0);
        }

    }

    public void _SupporterMagicWand() //Đũa thần: 2 kết quả hoàn thành
    {
        if (PlayerPrefsController.instance._GetNumOfMagicWand() > 0)
        {
            if (BoardController.levelData.Level != 5)
            {
                PlayerPrefsController.instance._SetNumOfMagicWand(1, false);
                if (PlayerPrefsController.instance._GetNumOfMagicWand() == 0) btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfMagicWand() + "";
            }
            PlayerPrefsController.instance.audioSource.PlayOneShot(magicWandClip);
            Time.timeScale = 1;
            //_ResetTileState();
            isCoupled = true;
            foreach (var tile in BoardController.buttonList)
            {
                tile.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            int numCouple = 0;
            for (int index = 1; index < BoardController.dict.Count; index++)
            {
            check:
                if (numCouple == 2)
                {
                    break;
                }
                List<Transform> temp = BoardController.instance._SearchSameTiles(BoardController.dict.ElementAt(index).Value);
                if (temp.Count != 0)
                {
                    for (int i = 0; i < temp.Count / 2; i++)
                    {
                        if (numCouple == 2)
                        {
                            goto check;
                        }
                        StopAllCoroutines();
                        StartCoroutine(DestroyTiles(temp[2 * i], temp[2 * i + 1]));
                        numCouple++;
                    }
                }
            }
        }
        else
        {
            PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(1);
        }
    }

    public void _SupporterFreezeTime() //Snow: Đóng băng thời gian 10 giây
    {
        PlayerPrefsController.instance.timeWarningSource.Pause();
        if (PlayerPrefsController.instance._GetNumOfFreezeTime() > 0)
        {
            if (BoardController.levelData.Level != 7)
            {
                PlayerPrefsController.instance._SetNumOfFreezeTime(1, false);
                if (PlayerPrefsController.instance._GetNumOfFreezeTime() == 0) btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfFreezeTime() + "";
            }
            PlayerPrefsController.instance.audioSource.PlayOneShot(freezeTimeClip);
            Time.timeScale = 1;
            Color ice = new Color32(0, 221, 255, 255);
            TimeController.instance._FreezeTime(true);
            btSpFreeze.interactable = false;
            DOTween.Kill(TimeController.instance.iconClock);
            DOTween.Kill(TimeController.instance.iconClock.GetComponent<Image>());
            TimeController.instance.iconClock.DOScale(new Vector3(1.2f, 1.2f, 1), 1).SetEase(Ease.InOutQuad);
            TimeController.instance.iconClock.DORotate(new Vector3(0, 0, -20), 1).SetEase(Ease.InOutQuad);
            TimeController.instance.iconClock.GetComponent<Image>().DOColor(ice, 1).SetEase(Ease.InOutQuad).OnComplete(delegate
            {
                TimeController.instance.iconClock.DOScale(Vector3.one, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f);
                TimeController.instance.iconClock.DORotate(Vector3.zero, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f);
                TimeController.instance.iconClock.GetComponent<Image>().DOColor(Color.white, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f)
                .OnComplete(delegate
                {
                    TimeController.instance._FreezeTime(false);
                    if (!TimeController.muchTimeLeft) TimeController.instance._TweenTimeWarn();
                    PlayerPrefsController.instance.timeWarningSource.UnPause();
                    btSpFreeze.interactable = true;
                });
            });
        }
        else
        {
            PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(2);
        }
    }

    public void _SupporterShuffle() //Đổi vị trí: Khi người chơi sử dụng, các item thay đổi vị trí cho nhau.
    {
        if (PlayerPrefsController.instance._GetNumOfShuffle() > 0)
        {
            if (BoardController.levelData.Level != 9)
            {
                PlayerPrefsController.instance._SetNumOfShuffle(1, false);
                if (PlayerPrefsController.instance._GetNumOfShuffle() == 0) btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsController.instance._GetNumOfShuffle() + "";
            }
            PlayerPrefsController.instance.audioSource.PlayOneShot(shuffleClip);
            _ResetTileState();
            isCoupled = true;
            foreach (var tile in BoardController.buttonList)
            {
                tile.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            BoardController.instance._RearrangeTiles();
        }
        else
        {
            PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(3);
        }
    }

    #endregion

    #region Booster
    public void _BoosterTimeWizard() //Time : Tăng 10 giây khi sử dụng
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(timeWizardClip);
        boosterPanel.SetActive(false);
        TimeController.instance._SetTimeForSlider(true);
    }

    public void _BoosterEarthquake() //Remove All: Xoá 1 hình bất kỳ
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        boosterPanel.SetActive(false);
        bool isFounded = false;
        while (!isFounded)
        {
            int index = Random.Range(1, BoardController.dict.Count);
            List<Transform> temp = BoardController.instance._SearchSameTiles(BoardController.dict.ElementAt(index).Value);
            if (temp.Count != 0)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    StartCoroutine(DestroyTiles(temp[i]));
                }
                isFounded = true;
            }
        }
    }

    public void _BoosterMirror() //Mirror: gấp đôi số sao đạt được
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickButtonClip);
        boosterPanel.SetActive(false);

    }

    #endregion

    #region IEnumerator
    IEnumerator MakeConnection(params Transform[] linePositions)
    {
        LineController.instance._DrawLine(0.15f * tile.GetComponent<TileController>().Size / 100, linePositions);
        yield return new WaitForSeconds(0.4f);
        LineController.instance._EraseLine();
    }

    IEnumerator DestroyTiles(params Transform[] linePositions)
    {
        settingOnButton.interactable = false;
        _EnableSupporter(false);
        int n = linePositions.Length;
        for (int i = 0; i < linePositions.Length; i++)
        {
            if (linePositions[i] == null)
            {
                n--;
            }
        }

        for (int i = 0; i < n; i++)
        {
            if (i == 0 || i == n - 1)
            {
                int index = i;
                linePositions[index].GetComponent<Button>().interactable = false;
                linePositions[index].GetComponent<RectTransform>().DOSizeDelta(Vector2.zero, 0.4f).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(delegate { linePositions[index].gameObject.SetActive(false); });
                BoardController.instance._DeactivateTile(linePositions[index].GetComponent<TileController>());
            }
        }
        if (BoardController.levelData.Level == 1)
        {
            TutorialController.order++;
            if (TutorialController.order < 5)
            {
                TutorialController.instance._FocusOnCoupleTile(
                TutorialController.instance._FindTransform(BoardController.buttonListWithoutBlocker, TutorialController.coupleIndex[TutorialController.order].Item1),
                TutorialController.instance._FindTransform(BoardController.buttonListWithoutBlocker, TutorialController.coupleIndex[TutorialController.order].Item2));
            }
            else
            {
                TutorialController.instance.connectFailTutorial.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        yield return new WaitForSeconds(0.4f);
        _ResetTileState();
        BoardController.instance._ActivateGravity();

        yield return new WaitForSeconds(0.2f);
        settingOnButton.interactable = true;
        _EnableSupporter(true);
        BoardController.instance._CheckPossibleConnection();
        BoardController.instance._CheckProcess();
    }

    #endregion

    #region Algorithm
    void _SetLinePositions(Transform t0, Transform t1, Transform t2, Transform t3)
    {
        points[0] = t0;
        points[1] = t1;
        points[2] = t2;
        points[3] = t3;
    }

    public bool _HasAvailableConnection(Transform tile1, Transform tile2)
    {
        #region 1 line
        // check line with x
        if (tile1.localPosition.x == tile2.localPosition.x)
        {
            if (_HasVerticalLine(tile1, tile2))
            {
                _SetLinePositions(tile1, tile2, null, null);
                return true;
            }
        }
        // check line with y
        if (tile1.localPosition.y == tile2.localPosition.y)
        {
            if (_HasHorizontalLine(tile1, tile2))
            {
                _SetLinePositions(tile1, tile2, null, null);
                return true;
            }
        }
        #endregion

        #region 2 lines
        if (_HasHorizontalLshapedPath(tile1, tile2))
        {
            return true;
        }
        if (_HasVerticalLshapedPath(tile1, tile2))
        {
            return true;
        }
        #endregion

        #region 3 lines
        if (_HasHorizontalZshapedPath(tile1, tile2))
        {
            return true;
        }
        if (_HasVerticalZshapedPath(tile1, tile2))
        {
            return true;
        }

        // check more right
        if (_HasHorizontalUshapedPath(tile1, tile2, 1))
        {
            return true;
        }
        // check more left
        if (_HasHorizontalUshapedPath(tile1, tile2, -1))
        {
            return true;
        }
        // check more down
        if (_HasVerticalUshapedPath(tile1, tile2, 1))
        {
            return true;
        }
        // check more up
        if (_HasVerticalUshapedPath(tile1, tile2, -1))
        {
            return true;
        }
        #endregion

        return false;
    }

    //button cung hang
    bool _HasHorizontalLine(Transform tile1, Transform tile2)
    {
        float y = tile1.localPosition.y;
        float min = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float max = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        if (max - min == buttonSide)
        {
            return true;
        }
        else
        {
            for (float i = min + buttonSide; i < max; i += buttonSide)
            {
                if (BoardController.instance._HasButtonInLocation(i, y))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //button cung cot
    bool _HasVerticalLine(Transform tile1, Transform tile2)
    {
        float x = tile1.localPosition.x;
        float min = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        float max = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        if (max - min == buttonSide)
        {
            return true;
        }
        else
        {
            for (float i = min + buttonSide; i < max; i += buttonSide)
            {
                if (BoardController.instance._HasButtonInLocation(x, i))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //2 button trong hinh chu nhat ngang: check hinh chu L
    bool _HasHorizontalLshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform leftTile, rightTile;
        float leftX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float rightX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        if (leftX == tile1.localPosition.x)
        {
            leftTile = tile1;
            rightTile = tile2;
        }
        else
        {
            leftTile = tile2;
            rightTile = tile1;
        }

        #endregion

        for (float i = leftX; i <= rightX; i += (rightX - leftX))
        {
            tempAlignWithLeft.localPosition = new Vector3(i, leftTile.localPosition.y, 0);
            tempAlignWithRight.localPosition = new Vector3(i, rightTile.localPosition.y, 0);
            if (i == leftX)
            {
                if (!BoardController.instance._HasButtonInLocation(i, rightTile.localPosition.y) &&
                    _HasVerticalLine(leftTile, tempAlignWithRight) && _HasHorizontalLine(tempAlignWithRight, rightTile))
                {
                    _SetLinePositions(leftTile, tempAlignWithRight, rightTile, null);
                    return true;
                }
            }
            else
            {
                if (!BoardController.instance._HasButtonInLocation(i, leftTile.localPosition.y) &&
                    _HasHorizontalLine(leftTile, tempAlignWithLeft) && _HasVerticalLine(tempAlignWithLeft, rightTile))
                {
                    _SetLinePositions(leftTile, tempAlignWithLeft, rightTile, null);
                    return true;
                }
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat dung: check hinh chu L
    bool _HasVerticalLshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform topTile, bottomTile;
        float topY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float bottomY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        if (topY == tile1.localPosition.y)
        {
            topTile = tile1;
            bottomTile = tile2;
        }
        else
        {
            topTile = tile2;
            bottomTile = tile1;
        }

        #endregion

        for (float i = topY; i >= bottomY; i -= (topY - bottomY))
        {
            tempAlignWithTop.localPosition = new Vector3(topTile.localPosition.x, i, 0);
            tempAlignWithBottom.localPosition = new Vector3(bottomTile.localPosition.x, i, 0);
            if (i == topY)
            {
                if (!BoardController.instance._HasButtonInLocation(bottomTile.localPosition.x, i) &&
                    _HasHorizontalLine(topTile, tempAlignWithBottom) && _HasVerticalLine(tempAlignWithBottom, bottomTile))
                {
                    _SetLinePositions(topTile, tempAlignWithBottom, bottomTile, null);
                    return true;
                }
            }
            else
            {
                if (!BoardController.instance._HasButtonInLocation(topTile.localPosition.x, i) &&
                        _HasVerticalLine(topTile, tempAlignWithTop) && _HasHorizontalLine(tempAlignWithTop, bottomTile))
                {
                    _SetLinePositions(topTile, tempAlignWithStart, bottomTile, null);
                    return true;
                }
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat ngang: check hinh chu Z
    bool _HasHorizontalZshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform leftTile, rightTile;
        float leftX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float rightX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        if (leftX == tile1.localPosition.x)
        {
            leftTile = tile1;
            rightTile = tile2;
        }
        else
        {
            leftTile = tile2;
            rightTile = tile1;
        }
        float buttonSide = tile1.GetComponent<TileController>().Size;
        bool checkPoint;

        #endregion

        for (float i = (leftX + buttonSide); i < rightX; i += buttonSide)
        {
            tempAlignWithLeft.localPosition = new Vector3(i, leftTile.localPosition.y, 0);
            tempAlignWithRight.localPosition = new Vector3(i, rightTile.localPosition.y, 0);
            checkPoint = BoardController.instance._HasButtonInLocation(i, leftTile.localPosition.y)
                || BoardController.instance._HasButtonInLocation(i, rightTile.localPosition.y);
            if (!checkPoint && _HasHorizontalLine(leftTile, tempAlignWithLeft) &&
                _HasVerticalLine(tempAlignWithLeft, tempAlignWithRight) && _HasHorizontalLine(tempAlignWithRight, rightTile))
            {
                _SetLinePositions(leftTile, tempAlignWithLeft, tempAlignWithRight, rightTile);
                return true;
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat dung: check hinh chu Z
    bool _HasVerticalZshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform topTile, bottomTile;
        float topY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float bottomY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        if (topY == tile1.localPosition.y)
        {
            topTile = tile1;
            bottomTile = tile2;
        }
        else
        {
            topTile = tile2;
            bottomTile = tile1;
        }
        float buttonSide = tile1.GetComponent<TileController>().Size;
        bool checkPoint;

        #endregion

        for (float i = (topY - buttonSide); i > bottomY; i -= buttonSide)
        {
            tempAlignWithTop.localPosition = new Vector3(topTile.localPosition.x, i, 0);
            tempAlignWithBottom.localPosition = new Vector3(bottomTile.localPosition.x, i, 0);
            checkPoint = BoardController.instance._HasButtonInLocation(topTile.localPosition.x, i)
                || BoardController.instance._HasButtonInLocation(bottomTile.localPosition.x, i);
            if (!checkPoint && _HasVerticalLine(topTile, tempAlignWithTop) &&
                _HasHorizontalLine(tempAlignWithTop, tempAlignWithBottom) && _HasVerticalLine(tempAlignWithBottom, bottomTile))
            {
                _SetLinePositions(topTile, tempAlignWithTop, tempAlignWithBottom, bottomTile);
                return true;
            }
        }
        return false;
    }

    //mo rong ra theo chieu ngang: check hinh chu U
    bool _HasHorizontalUshapedPath(Transform tile1, Transform tile2, int direct)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform startTile, endTile;
        float lowerX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float higherX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        float width = BoardController.column * buttonSide;
        bool checkPoint;

        #endregion

        if (direct > 0)
        {
            #region Di tu trai sang phai

            if (lowerX == tile1.localPosition.x)
            {
                startTile = tile1;
                endTile = tile2;
            }
            else
            {
                startTile = tile2;
                endTile = tile1;
            }

            for (float i = (higherX + buttonSide); i <= (width + buttonSide) / 2; i += buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(i, startTile.localPosition.y, 0);
                tempAlignWithEnd.localPosition = new Vector3(i, endTile.localPosition.y, 0);
                checkPoint = BoardController.instance._HasButtonInLocation(i, startTile.localPosition.y)
                    || BoardController.instance._HasButtonInLocation(i, endTile.localPosition.y);
                if (!checkPoint && _HasHorizontalLine(startTile, tempAlignWithStart) &&
                    _HasVerticalLine(tempAlignWithStart, tempAlignWithEnd) && _HasHorizontalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }
        else
        {
            #region Di tu phai sang trai

            if (lowerX == tile1.localPosition.x)
            {
                startTile = tile2;
                endTile = tile1;
            }
            else
            {
                startTile = tile1;
                endTile = tile2;
            }

            for (float i = (lowerX - buttonSide); i >= -(width + buttonSide) / 2; i -= buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(i, startTile.localPosition.y, 0);
                tempAlignWithEnd.localPosition = new Vector3(i, endTile.localPosition.y, 0);
                checkPoint = BoardController.instance._HasButtonInLocation(i, startTile.localPosition.y)
                    || BoardController.instance._HasButtonInLocation(i, endTile.localPosition.y);
                if (!checkPoint && _HasHorizontalLine(startTile, tempAlignWithStart) &&
                    _HasVerticalLine(tempAlignWithStart, tempAlignWithEnd) && _HasHorizontalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }

        return false;
    }

    //mo rong ra theo chieu doc: check hinh chu U
    bool _HasVerticalUshapedPath(Transform tile1, Transform tile2, int direct)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x)
        {
            return false;
        }

        Transform startTile, endTile;
        float higherY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float lowerY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        float height = BoardController.row * buttonSide;
        bool checkPoint;

        #endregion

        if (direct > 0)
        {
            #region Di tu tren xuong duoi

            if (higherY == tile1.localPosition.y)
            {
                startTile = tile1;
                endTile = tile2;
            }
            else
            {
                startTile = tile2;
                endTile = tile1;
            }

            for (float i = (lowerY - buttonSide); i >= -(height + buttonSide) / 2; i -= buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(startTile.localPosition.x, i, 0);
                tempAlignWithEnd.localPosition = new Vector3(endTile.localPosition.x, i, 0);
                checkPoint = BoardController.instance._HasButtonInLocation(startTile.localPosition.x, i)
                    || BoardController.instance._HasButtonInLocation(endTile.localPosition.x, i);
                if (!checkPoint && _HasVerticalLine(startTile, tempAlignWithStart) &&
                    _HasHorizontalLine(tempAlignWithStart, tempAlignWithEnd) && _HasVerticalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }
        else
        {
            #region Di tu duoi len tren

            if (higherY == tile1.localPosition.y)
            {
                startTile = tile2;
                endTile = tile1;
            }
            else
            {
                startTile = tile1;
                endTile = tile2;
            }

            for (float i = (higherY + buttonSide); i <= (height + buttonSide) / 2; i += buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(startTile.localPosition.x, i, 0);
                tempAlignWithEnd.localPosition = new Vector3(endTile.localPosition.x, i, 0);
                checkPoint = BoardController.instance._HasButtonInLocation(startTile.localPosition.x, i)
                    || BoardController.instance._HasButtonInLocation(endTile.localPosition.x, i);
                if (!checkPoint && _HasVerticalLine(startTile, tempAlignWithStart) &&
                    _HasHorizontalLine(tempAlignWithStart, tempAlignWithEnd) && _HasVerticalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }

        return false;
    }

    #endregion

}
