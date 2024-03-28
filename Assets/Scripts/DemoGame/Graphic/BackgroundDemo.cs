using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundDemo : MonoBehaviour
{
    void Start()
    {
        //GameObject background = transform.GetChild(BoardDemo.levelData.Theme - 1).gameObject;
        //for (int i = 0; i < transform.childCount; i++)
        //{
        //    if (BoardDemo.levelData.Theme - 1 != i)
        //    {
        //        transform.GetChild(i).gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        transform.GetChild(i).gameObject.SetActive(true);
        //    }
        //}
        GameObject background = transform.GetChild(15).gameObject;

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        float height = sr.bounds.size.y;
        float width = sr.bounds.size.x;
        float scaleHeight = Camera.main.orthographicSize * 2f;
        float scaleWidth = scaleHeight * Screen.width / Screen.height;
        background.transform.localScale = new Vector3(scaleWidth / width, scaleHeight / height, 0);
    }
}
