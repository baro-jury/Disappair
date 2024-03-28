using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayDemo : MonoBehaviour
{
    public static GameplayDemo instance;

    public GameObject pause, shop;
    public AudioClip clickButtonClip, matchTileClip, switchClip, passLevelClip;
    public AudioClip timeWizardClip, hintClip, magicWandClip, freezeTimeClip, shuffleClip;
    public Transform startCoinAnimPos, endCoinAnimPos;

    private bool isCoupled = true, isHinted = false;
    private float numberOfRescue = 1;
    private Transform[] points = new Transform[4];
    private Transform tile;

    [SerializeField]
    private Text coins, titleLv, titleComplete;
    [SerializeField]
    private TextMeshProUGUI lvComplete;
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private Button settingOnButton, settingOffButton;
    [SerializeField]
    private Button btSpHint, btSpMagicWand, btSpFreeze, btSpShuffle;
    [SerializeField]
    private GameObject timeOutPanel, winPanel;
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
        titleLv.text = "LEVEL " + BoardDemo.levelData.Level;
        titleComplete.text = "LEVEL " + BoardDemo.levelData.Level;
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(PlayerPrefsDemo.instance.audioSource.mute);
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(PlayerPrefsDemo.instance.musicSource.mute);
        coins.text = PlayerPrefsDemo.instance._GetCoinsInPossession() + "";

        Vector3 temp;
        if (BoardDemo.levelData.Level == 1)
        {
            Time.timeScale = 1;
            TimeDemo.instance._FreezeTime(true);
        }
        else Time.timeScale = 0;

        if (BoardDemo.levelData.Level < 3) btSpHint.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardDemo.levelData.Level == 3)
        {
            btSpHint.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialDemo.instance.tutorialPanel.transform.GetChild(1).localPosition;
            TutorialDemo.instance.tutorialPanel.transform.GetChild(1).localPosition = new Vector3
                (temp.x, btSpHint.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else 
        {
            if (PlayerPrefsDemo.instance._GetNumOfHint() == 0) btSpHint.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfHint() + ""; 
        }


        if (BoardDemo.levelData.Level < 5) btSpMagicWand.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardDemo.levelData.Level == 5)
        {
            btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialDemo.instance.tutorialPanel.transform.GetChild(2).localPosition;
            TutorialDemo.instance.tutorialPanel.transform.GetChild(2).localPosition = new Vector3
                (temp.x, btSpMagicWand.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsDemo.instance._GetNumOfMagicWand() == 0) btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfMagicWand() + "";
        }

        if (BoardDemo.levelData.Level < 7) btSpFreeze.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardDemo.levelData.Level == 7)
        {
            btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialDemo.instance.tutorialPanel.transform.GetChild(3).localPosition;
            TutorialDemo.instance.tutorialPanel.transform.GetChild(3).localPosition = new Vector3
                (temp.x, btSpFreeze.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else
        {
            if (PlayerPrefsDemo.instance._GetNumOfFreezeTime() == 0) btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfFreezeTime() + "";
        }

        if (BoardDemo.levelData.Level < 9) btSpShuffle.transform.parent.GetChild(1).gameObject.SetActive(true);
        else if (BoardDemo.levelData.Level == 9)
        {
            btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "∞";
            temp = TutorialDemo.instance.tutorialPanel.transform.GetChild(4).localPosition;
            TutorialDemo.instance.tutorialPanel.transform.GetChild(4).localPosition = new Vector3
                (temp.x, btSpShuffle.transform.parent.parent.parent.localPosition.y + 105 + 185, temp.z);
        }
        else {
            if (PlayerPrefsDemo.instance._GetNumOfShuffle() == 0) btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "+";
            else btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfShuffle() + "";
        }
    }

    #region Ingame
    public void _Pause()
    {
        PlayerPrefsDemo.instance.audioSource.Pause();
        settingOnButton.interactable = false;
        pausePanel.SetActive(true);
        pausePanel.GetComponent<Button>().interactable = false;
        pausePanel.transform.GetComponent<Image>().DOFade(.5f, .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        settingOffButton.transform.DORotate(new Vector3(0, 0, 0), .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(-80, -85, 0);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector3(-80, -85, 0);

        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosY(-235, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-385, .4f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                settingOffButton.interactable = true;
                pausePanel.GetComponent<Button>().interactable = true;
                pausePanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                pausePanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                settingOffButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
            });

        Time.timeScale = 0;
    }

    public void _Resume()
    {
        PlayerPrefsDemo.instance.audioSource.UnPause();
        settingOffButton.interactable = false;
        pausePanel.GetComponent<Button>().interactable = false;
        pausePanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        settingOffButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetComponent<Image>().DOFade(0f, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);

        settingOffButton.transform.DORotate(new Vector3(0, 0, 180), .5f).SetEase(Ease.InOutQuad).SetUpdate(true);

        pausePanel.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosY(-85, .4f).SetEase(Ease.InOutQuad).SetUpdate(true);
        pausePanel.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-85, .4f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                pausePanel.SetActive(false);
                settingOnButton.interactable = true;
            });

        Time.timeScale = 1;
    }

    public void _TurnOffSound()
    {
        PlayerPrefsDemo.instance.audioSource.mute = true;
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        pausePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }

    public void _TurnOnSound()
    {
        PlayerPrefsDemo.instance.audioSource.mute = false;
        pausePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        pausePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
    }

    public void _TurnOffMusic()
    {
        PlayerPrefsDemo.instance.musicSource.mute = true;
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        pausePanel.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
    }

    public void _TurnOnMusic()
    {
        PlayerPrefsDemo.instance.musicSource.mute = false;
        pausePanel.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        pausePanel.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
    }

    public void _OpenShopCoins()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
        Time.timeScale = 0;
        shop.transform.GetChild(4).gameObject.SetActive(true);
        shop.transform.GetChild(4).GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { Time.timeScale = 1; });
    }
    public void _UpdateCoin()
    {
        coins.text = PlayerPrefsDemo.instance._GetCoinsInPossession() + "";
    }

    public void _TimeOut()
    {
        //Time.timeScale = 0;
        PlayerPrefsDemo.instance.audioSource.Stop();
        //timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.one;
        timeOutPanel.SetActive(true);
        timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .15f).SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    public void _BuyMoreTime()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
        if (PlayerPrefsDemo.instance._GetCoinsInPossession() >= numberOfRescue * 100)
        {
            PlayerPrefsDemo.instance._SetCoinsInPossession((int)(numberOfRescue * 100), false);
            TimeDemo.instance._ResetIconState();
            TimeDemo.instance._SetTime(60, 1);
            coins.text = PlayerPrefsDemo.instance._GetCoinsInPossession() + "";
            timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                timeOutPanel.SetActive(false);
                numberOfRescue += 0.5f;
                if (numberOfRescue > 2) numberOfRescue = 2;
                timeOutPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "PLAY      " + numberOfRescue * 100;
            });
            Time.timeScale = 1;
        }
        else
        {
            _OpenShopCoins();
        }

    }

    public void _WatchAds()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
        UnityEvent eReward = new UnityEvent();
        eReward.AddListener(() =>
        {
            // luồng game sau khi tắt quảng cáo ( tặng thưởng cho user )
            TimeDemo.instance._ResetIconState();
            TimeDemo.instance._SetTime(15, 1);
            timeOutPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                timeOutPanel.SetActive(false);
            });
            Time.timeScale = 1;
        });
        ACEPlay.Bridge.BridgeController.instance.ShowRewardedAd(eReward, null);
    }

    public void _Replay()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void _CompleteLevel()
    {
        Time.timeScale = 0;
        PlayerPrefsDemo.instance.audioSource.Stop();
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(passLevelClip);
        winPanel.transform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.zero;
        winPanel.SetActive(true);
        winPanel.transform.GetChild(0).GetComponent<RectTransform>().DOScale(Vector3.one, .25f).SetEase(Ease.InOutQuad).SetUpdate(true);
        if (BoardDemo.levelData.Level % 10 == 0)
        {
            PlayerPrefsDemo.instance._SetCoinsInPossession(250, true);
            lvComplete.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void _GoToNextLevel()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
        if (BoardDemo.levelData.Level != Resources.LoadAll("Levels").Length)
        {
            BoardDemo.levelData.Level++;
            LevelDemo.instance._PlayLevel(BoardDemo.levelData.Level);
        }
    }

    #endregion

    #region Click Tiles
    public void _ClickTile(Transform currentTile)
    {
        Color color = new Color32(0, 220, 255, 255); //blue
        currentTile.DOComplete();
        currentTile.DOKill();
        currentTile.GetComponent<RectTransform>().DOScale(new Vector3(.8f, .8f, 1), .1f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                currentTile.GetComponent<RectTransform>().DOScale(Vector3.one, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                currentTile.GetChild(0).GetComponent<Image>().DOColor(color, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
            });
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
                //Debug.Log("Click cung 1 tile");
                foreach (var tile in BoardDemo.buttonList)
                {
                    if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                }
                isCoupled = !isCoupled;
            }
            else
            {
                if (tile.GetComponent<TileController>().Id != currentTile.GetComponent<TileController>().Id)
                {
                    //Debug.Log("Click khac tile khac ID");
                    foreach (var tile in BoardDemo.buttonList)
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
                        for(int i = 0; i< points.Length;i++)
                        {
                            temp[i] = points[i];
                        }
                        StopAllCoroutines();
                        StartCoroutine(MakeConnection(temp));
                        PlayerPrefsDemo.instance.audioSource.PlayOneShot(matchTileClip);
                        StartCoroutine(DestroyTiles(temp));
                    }
                    else
                    {
                        if (BoardDemo.levelData.Level == 1)
                        {
                            Handheld.Vibrate();
                            TutorialDemo.order++;
                            if (TutorialDemo.order == 4)
                            {
                                TutorialDemo.instance.connectFailTutorial.transform.GetChild(0).gameObject.SetActive(true);
                            }
                            else if (TutorialDemo.order == 5)
                            {
                                TutorialDemo.instance.connectFailTutorial.transform.GetChild(1).gameObject.SetActive(true);
                            }
                        }
                        //Debug.Log("Khong the ket noi");
                        foreach (var tile in BoardDemo.buttonList)
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
        foreach (var tile in BoardDemo.buttonList)
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
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
            if (PlayerPrefsDemo.instance._GetCoinsInPossession() >= 290)
            {
                PlayerPrefsDemo.instance._SetCoinsInPossession(290, false);
                switch (order)
                {
                    case 0:
                        PlayerPrefsDemo.instance._SetNumOfHint(3, true);
                        btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfHint() + "";
                        break;
                    case 1:
                        PlayerPrefsDemo.instance._SetNumOfMagicWand(3, true);
                        btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfMagicWand() + "";
                        break;
                    case 2:
                        PlayerPrefsDemo.instance._SetNumOfFreezeTime(3, true);
                        btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfFreezeTime() + "";
                        PlayerPrefsDemo.instance.timeWarningSource.UnPause();
                        break;
                    case 3:
                        PlayerPrefsDemo.instance._SetNumOfShuffle(3, true);
                        btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfShuffle() + "";
                        break;
                }
                shopPanel.SetActive(false);
                Time.timeScale = 1;
                coins.text = PlayerPrefsDemo.instance._GetCoinsInPossession() + "";
            }
            else
            {
                _OpenShopCoins();
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
        if (PlayerPrefsDemo.instance._GetNumOfHint() > 0)
        {
            if (BoardDemo.levelData.Level != 3)
            {
                PlayerPrefsDemo.instance._SetNumOfHint(1, false);
                if (PlayerPrefsDemo.instance._GetNumOfHint() == 0) btSpHint.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpHint.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfHint() + "";
            }
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(hintClip);
            _ResetTileState();

            while (!isHinted)
            {
                int index = Random.Range(1, BoardDemo.dict.Count);
                List<Transform> temp = BoardDemo.instance._SearchSameTiles(BoardDemo.dict.ElementAt(index).Value);
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
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(0);
        }

    }

    public void _SupporterMagicWand() //Đũa thần: 2 kết quả hoàn thành
    {
        if (PlayerPrefsDemo.instance._GetNumOfMagicWand() > 0)
        {
            if (BoardDemo.levelData.Level != 5)
            {
                PlayerPrefsDemo.instance._SetNumOfMagicWand(1, false);
                if (PlayerPrefsDemo.instance._GetNumOfMagicWand() == 0) btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpMagicWand.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfMagicWand() + "";
            }
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(magicWandClip);
            Time.timeScale = 1;
            isCoupled = true;
            foreach (var tile in BoardDemo.buttonList)
            {
                tile.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            int numCouple = 0;
            for (int index = 1; index < BoardDemo.dict.Count; index++)
            {
            check:
                if (numCouple == 2)
                {
                    break;
                }
                List<Transform> temp = BoardDemo.instance._SearchSameTiles(BoardDemo.dict.ElementAt(index).Value);
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
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(1);
        }
    }

    public void _SupporterFreezeTime() //Snow: Đóng băng thời gian 10 giây
    {
        PlayerPrefsDemo.instance.timeWarningSource.Pause();
        if (PlayerPrefsDemo.instance._GetNumOfFreezeTime() > 0)
        {
            if (BoardDemo.levelData.Level != 7)
            {
                PlayerPrefsDemo.instance._SetNumOfFreezeTime(1, false);
                if (PlayerPrefsDemo.instance._GetNumOfFreezeTime() == 0) btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpFreeze.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfFreezeTime() + "";
            }
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(freezeTimeClip);
            Time.timeScale = 1;
            Color ice = new Color32(0, 221, 255, 255);
            TimeDemo.instance._FreezeTime(true);
            btSpFreeze.interactable = false;
            DOTween.Kill(TimeDemo.instance.iconClock);
            DOTween.Kill(TimeDemo.instance.iconClock.GetComponent<Image>());
            TimeDemo.instance.iconClock.DOScale(new Vector3(1.2f, 1.2f, 1), 1).SetEase(Ease.InOutQuad);
            TimeDemo.instance.iconClock.DORotate(new Vector3(0, 0, -20), 1).SetEase(Ease.InOutQuad);
            TimeDemo.instance.iconClock.GetComponent<Image>().DOColor(ice, 1).SetEase(Ease.InOutQuad).OnComplete(delegate
            {
                TimeDemo.instance.iconClock.DOScale(Vector3.one, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f);
                TimeDemo.instance.iconClock.DORotate(Vector3.zero, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f);
                TimeDemo.instance.iconClock.GetComponent<Image>().DOColor(Color.white, 1).SetEase(Ease.InOutQuad).SetDelay(8.5f)
                .OnComplete(delegate
                {
                    TimeDemo.instance._FreezeTime(false);
                    if (!TimeDemo.instance.muchTimeLeft) TimeDemo.instance._TweenTimeWarn();
                    PlayerPrefsDemo.instance.timeWarningSource.UnPause();
                    btSpFreeze.interactable = true;
                });
            });
        }
        else
        {
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(2);
        }
    }

    public void _SupporterShuffle() //Đổi vị trí: Khi người chơi sử dụng, các item thay đổi vị trí cho nhau.
    {
        if (PlayerPrefsDemo.instance._GetNumOfShuffle() > 0)
        {
            if (BoardDemo.levelData.Level != 9)
            {
                PlayerPrefsDemo.instance._SetNumOfShuffle(1, false);
                if (PlayerPrefsDemo.instance._GetNumOfShuffle() == 0) btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = "+";
                else btSpShuffle.transform.GetChild(0).GetComponent<Text>().text = PlayerPrefsDemo.instance._GetNumOfShuffle() + "";
            }
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(shuffleClip);
            _ResetTileState();
            isCoupled = true;
            foreach (var tile in BoardDemo.buttonList)
            {
                tile.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            BoardDemo.instance._RearrangeTiles();
        }
        else
        {
            PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickButtonClip);
            _BuySupporter(3);
        }
    }

    #endregion

    #region IEnumerator
    IEnumerator MakeConnection(params Transform[] linePositions)
    {
        LineDemo.instance._DrawLine(0.15f * tile.GetComponent<TileController>().Size / 100, linePositions);
        yield return new WaitForSeconds(0.4f);
        LineDemo.instance._EraseLine();
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
                BoardDemo.instance._DeactivateTile(linePositions[index].GetComponent<TileController>());
            }
        }
        if (BoardDemo.levelData.Level == 1)
        {
            TutorialDemo.order++;
            if (TutorialDemo.order < 5)
            {
                TutorialDemo.instance._FocusOnCoupleTile(
                TutorialDemo.instance._FindTransform(BoardDemo.buttonListWithoutBlocker, TutorialDemo.coupleIndex[TutorialDemo.order].Item1),
                TutorialDemo.instance._FindTransform(BoardDemo.buttonListWithoutBlocker, TutorialDemo.coupleIndex[TutorialDemo.order].Item2));
            }
            else
            {
                TutorialDemo.instance.connectFailTutorial.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        yield return new WaitForSeconds(0.4f);
        _ResetTileState();
        BoardDemo.instance._ActivateGravity();

        yield return new WaitForSeconds(0.2f);
        settingOnButton.interactable = true;
        _EnableSupporter(true);
        BoardDemo.instance._CheckPossibleConnection();
        BoardDemo.instance._CheckProcess();
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
                if (BoardDemo.instance._HasButtonInLocation(i, y))
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
                if (BoardDemo.instance._HasButtonInLocation(x, i))
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
                if (!BoardDemo.instance._HasButtonInLocation(i, rightTile.localPosition.y) &&
                    _HasVerticalLine(leftTile, tempAlignWithRight) && _HasHorizontalLine(tempAlignWithRight, rightTile))
                {
                    _SetLinePositions(leftTile, tempAlignWithRight, rightTile, null);
                    return true;
                }
            }
            else
            {
                if (!BoardDemo.instance._HasButtonInLocation(i, leftTile.localPosition.y) &&
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
                if (!BoardDemo.instance._HasButtonInLocation(bottomTile.localPosition.x, i) &&
                    _HasHorizontalLine(topTile, tempAlignWithBottom) && _HasVerticalLine(tempAlignWithBottom, bottomTile))
                {
                    _SetLinePositions(topTile, tempAlignWithBottom, bottomTile, null);
                    return true;
                }
            }
            else
            {
                if (!BoardDemo.instance._HasButtonInLocation(topTile.localPosition.x, i) &&
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
            checkPoint = BoardDemo.instance._HasButtonInLocation(i, leftTile.localPosition.y)
                || BoardDemo.instance._HasButtonInLocation(i, rightTile.localPosition.y);
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
            checkPoint = BoardDemo.instance._HasButtonInLocation(topTile.localPosition.x, i)
                || BoardDemo.instance._HasButtonInLocation(bottomTile.localPosition.x, i);
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
        float width = BoardDemo.column * buttonSide;
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
                checkPoint = BoardDemo.instance._HasButtonInLocation(i, startTile.localPosition.y)
                    || BoardDemo.instance._HasButtonInLocation(i, endTile.localPosition.y);
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
                checkPoint = BoardDemo.instance._HasButtonInLocation(i, startTile.localPosition.y)
                    || BoardDemo.instance._HasButtonInLocation(i, endTile.localPosition.y);
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
        float height = BoardDemo.row * buttonSide;
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
                checkPoint = BoardDemo.instance._HasButtonInLocation(startTile.localPosition.x, i)
                    || BoardDemo.instance._HasButtonInLocation(endTile.localPosition.x, i);
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
                checkPoint = BoardDemo.instance._HasButtonInLocation(startTile.localPosition.x, i)
                    || BoardDemo.instance._HasButtonInLocation(endTile.localPosition.x, i);
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
