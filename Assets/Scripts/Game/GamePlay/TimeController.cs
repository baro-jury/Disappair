using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;
    public static bool isSaved, muchTimeLeft;
    public float time;
    public Transform iconClock;

    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Text timeText, timeTextComplete;

    //private float timeBurn = 1f;
    private float timestampFor1Star, timestampFor2Star, timestampFor3Star;
    private bool isFreezed;

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
        isSaved = false;
        muchTimeLeft = true;

        _SetTime(BoardController.levelData.Time[0], 0);
        timestampFor1Star = BoardController.levelData.Time[1];
        timestampFor2Star = BoardController.levelData.Time[2];
        timestampFor3Star = BoardController.levelData.Time[3];
        instance._SetTimeForSlider(false);

        slider.transform.GetChild(3).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            timestampFor1Star / time * slider.GetComponent<RectTransform>().rect.size.x, 0);
        slider.transform.GetChild(4).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            timestampFor2Star / time * slider.GetComponent<RectTransform>().rect.size.x, 0);
        slider.transform.GetChild(5).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            timestampFor3Star / time * slider.GetComponent<RectTransform>().rect.size.x, 0);
    }

    void Update()
    {
        if (isFreezed)
        {
            slider.value = time;
        }
        else
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
                slider.value = time;

                if (timestampFor3Star <= time)
                {
                    slider.transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
                    slider.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
                    slider.transform.GetChild(5).GetChild(0).gameObject.SetActive(true);
                }
                if (timestampFor2Star <= time && time < timestampFor3Star)
                {
                    slider.transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
                    slider.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
                    slider.transform.GetChild(5).GetChild(0).gameObject.SetActive(false);
                }
                else if (timestampFor1Star <= time && time < timestampFor2Star)
                {
                    slider.transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
                    slider.transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
                }
                else if (time < timestampFor1Star)
                {
                    slider.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
                }

                if (muchTimeLeft && time <= 15)
                {
                    muchTimeLeft = false;
                    PlayerPrefsController.instance.timeWarningSource.Play();
                    _TweenTimeWarn();
                }
            }
            else
            {
                muchTimeLeft = true;
                PlayerPrefsController.instance.timeWarningSource.Stop();
                foreach (var tile in BoardController.buttonList)
                {
                    DOTween.Kill(tile);
                    tile.localScale = Vector3.one;
                }
                LineController.instance._EraseLine();
                _SetTime(0, 0);
                if (isSaved)
                {
                    GameplayController.instance._GameOver();
                }
                else
                {
                    GameplayController.instance._TakeTheLastChance();
                }
            }
            _DisplayTime(time);
        }
    }

    public void _SetTime(float t, int plus)
    {
        if (plus == 1)
        {
            time += t;
        }
        else if (plus == 0)
        {
            time = t;
        }
        else if (plus == -1)
        {
            time -= t;
        }
    }
    void _DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timeTextComplete.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void _SetTimeForSlider(bool isBoosted)
    {
        if (isBoosted)
        {
            time += 10;
        }
        slider.minValue = 0f;
        slider.maxValue = time;
        slider.value = slider.maxValue;
    }

    public void _FreezeTime(bool freeze)
    {
        isFreezed = freeze;
    }

    public int _NumberOfStarsAchieved()
    {
        int count = 1;
        for (int i = 4; i <= 5; i++)
        {
            if (slider.transform.GetChild(i).GetChild(0).gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }

    public void _TweenTimeWarn()
    {
        iconClock.DOScale(new Vector3(1.2f, 1.2f, 1), 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        iconClock.DORotate(new Vector3(0, 0, -20), 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        iconClock.GetComponent<Image>().DOColor(Color.red, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    public void _ResetIconState()
    {
        DOTween.Kill(iconClock);
        DOTween.Kill(iconClock.GetComponent<Image>());
        iconClock.localScale = Vector3.one;
        iconClock.localEulerAngles = Vector3.zero;
        iconClock.GetComponent<Image>().color = Color.white;
    }
}
