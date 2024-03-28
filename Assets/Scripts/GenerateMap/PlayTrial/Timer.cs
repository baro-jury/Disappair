using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    [SerializeField]
    private TextMeshProUGUI timeText;

    public static float time = 0;
    private float timeRun = 1f;
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

    void Update()
    {
        if (!isFreezed)
        {
            time += timeRun * Time.deltaTime;
            _DisplayTime(time);
        }
    }

    void _DisplayTime(float timeToDisplay)
    {
        //timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void _FreezeTime(bool freeze)
    {
        isFreezed = freeze;
    }
}
