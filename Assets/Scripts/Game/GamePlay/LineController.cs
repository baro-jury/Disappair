using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    public static LineController instance;

    private LineRenderer line;

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
        line = gameObject.GetComponent<LineRenderer>();
    }

    public void _DrawLine(float width, params Transform[] points)
    {
        line.widthMultiplier = width;

        int n = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                n--;
            }
        }
        line.positionCount = n;

        float ratio = 1.0f / 96;
        if (points[0].localPosition.x != 0)
        {
            ratio = points[0].position.x / points[0].localPosition.x;
        }
        else
        {
            if (points[n - 1].localPosition.x != 0) ratio = points[n - 1].position.x / points[n - 1].localPosition.x;
        }

        for (int i = 0; i < n; i++)
        {
            if (i == 0 || i == n - 1)
            {
                line.SetPosition(i, new Vector3(points[i].position.x, points[i].position.y, 0));
            }
            else
            {
                line.SetPosition(i, new Vector3(points[i].position.x, points[i].position.y + 50 * ratio, 0));
            }
        }
    }

    public void _EraseLine()
    {
        line.positionCount = 0;
    }

}
