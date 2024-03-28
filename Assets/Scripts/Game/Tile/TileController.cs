using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public int Id { get; set; }
    public (int, int) Index { get; set; }
    public int Size { get; set; }

    public TileController() { }

    public TileController(int id, int indexRow, int indexCol, int size)
    {
        Id = id;
        Index = (indexRow, indexCol);
        Size = size;
    }


    [SerializeField]
    private AudioClip clickTileClip;

    //object ingame
    public void _TileClicked()
    {
        PlayerPrefsController.instance.audioSource.PlayOneShot(clickTileClip);
        GameplayController.instance._ClickTile(gameObject.transform);
    }

    //object in demo game
    public void _TileDemoClicked()
    {
        PlayerPrefsDemo.instance.audioSource.PlayOneShot(clickTileClip);
        GameplayDemo.instance._ClickTile(gameObject.transform);
    }

    //object in trial game
    public void _TileTrialClicked()
    {
        GameplayTrial.instance.audioSource.PlayOneShot(clickTileClip);
        GameplayTrial.instance._ClickTile(gameObject.transform);
    }
}
