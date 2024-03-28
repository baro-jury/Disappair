using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeDemo : MonoBehaviour
{
    public static TimeDemo instance;
    public float time;
    public bool muchTimeLeft;
    public Transform iconClock;

    private float oldTime;
    private bool isFreezed;
    [SerializeField]
    private Text timeText, timeTextComplete;

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
        muchTimeLeft = true;
        _SetTime(BoardDemo.levelData.Time[0], 0);
    }

    void Update()
    {
        if (!isFreezed)
        {
            if (time > 0)
            {
                time -= Time.deltaTime;

                if (muchTimeLeft && time <= 15)
                {
                    muchTimeLeft = false;
                    PlayerPrefsDemo.instance.timeWarningSource.Play();
                    _TweenTimeWarn();
                }
            }
            else
            {
                muchTimeLeft = true;
                foreach (var tile in BoardDemo.buttonList)
                {
                    DOTween.Kill(tile);
                    tile.localScale = Vector3.one;
                }
                LineDemo.instance._EraseLine();
                time = 0;
                GameplayDemo.instance._TimeOut();
            }
            _DisplayTime(time);
        }
    }

    public void _SetTime(float t, int plus)
    {
        if (plus == 1)
        {
            time += t;
            oldTime += t;
        }
        else if (plus == 0)
        {
            time = t;
            oldTime = t;
        }
        else if (plus == -1)
        {
            time -= t;
            oldTime -= t;
        }
    }
    
    void _DisplayTime(float timeToDisplay)
    {
        if (oldTime > timeToDisplay + 1)
        {
            oldTime = timeToDisplay;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            timeTextComplete.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void _FreezeTime(bool freeze)
    {
        isFreezed = freeze;
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
