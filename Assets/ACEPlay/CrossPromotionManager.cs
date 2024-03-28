using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CrossPromotionManager : MonoBehaviour
{

	public static CrossPromotionManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else Destroy(this.gameObject);
	}

	public Image sprBannerSetting;
	public VideoPlayer videoPlayer, videoPlayerEndGame;
	public MeshRenderer meshVideo, meshVideoEndGame;

	private void Start()
	{
		 //ACEPlay.CrossPromotion.CrossPromotionController.instance.SetUrl("https://pbs.twimg.com/media/D36ubMfUwAEYYJD.jpg", "https://pbs.twimg.com/media/ENvmSWpUcAA9pYK.png", "https://video.twimg.com/ext_tw_video/1214764133536555008/pu/vid/720x720/tMOHyYzFYMaaEL2_.mp4", "https://video.twimg.com/ext_tw_video/1214764133536555008/pu/vid/720x720/tMOHyYzFYMaaEL2_.mp4", "https://play.google.com/store/apps/details?id=com.vicenter.hexa.puzzle.jigsaw", "1445996327", 1, 1, 1, 1, 1, 1, 1);
		if (videoPlayer.gameObject == null) return;
		if (videoPlayerEndGame.gameObject == null) return;
		videoPlayer.transform.parent.gameObject.SetActive(false);
		videoPlayerEndGame.transform.parent.gameObject.SetActive(false);
		if (!videoPlayer.transform.parent.gameObject.activeSelf)
			ShowVideo111();
	}

	public void ShowVideo111()
	{
		if (videoPlayer.gameObject == null) return;
		if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableCrossPromotion)
		{
			if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableVideoOnStart != 0)
			{
				ShowVideo();
			}
		}
		else
		{
			videoPlayer.transform.parent.gameObject.SetActive(false);
		}
	}

	public void _ShowBannerInSettings()
	{
		if (sprBannerSetting.gameObject.activeInHierarchy) return;

		if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableCrossPromotion)
		{
			if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableBannerOnSetting != 0)
			{
				ShowBanner(sprBannerSetting);
			}
		}
		else
		{
			sprBannerSetting.transform.gameObject.SetActive(false);
		}
	}

	public void _ShowVideoEndGame()
	{
		if (videoPlayerEndGame.gameObject == null) return;
		if (videoPlayerEndGame.gameObject.activeInHierarchy) return;

		if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableCrossPromotion)
		{
			if (ACEPlay.CrossPromotion.CrossPromotionController.instance.EnableVideoOnEndgame != 0)
			{
				ShowVideoEndGame();
			}
		}
		else
		{
			videoPlayerEndGame.transform.parent.gameObject.SetActive(false);
		}
	}

	public void ShowIcon(Image icon)
	{
		Debug.Log("Show Icon");
		Texture2D texture = ACEPlay.CrossPromotion.CrossPromotionController.instance.appIcon;
		if (texture == null)
		{
			Debug.Log("No Icon");
			icon.transform.parent.gameObject.SetActive(false);
			return;
		}
		Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		icon.sprite = image;
		icon.transform.parent.gameObject.SetActive(true);
	}

	public void ShowBanner(Image banner)
	{
		Debug.Log("Show Banner Cross");
		Texture2D texture = ACEPlay.CrossPromotion.CrossPromotionController.instance.appBanner;
		if (texture == null)
		{
			Debug.Log("No Banner Cross");
			banner.transform.gameObject.SetActive(false);
			return;
		}
		Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		banner.sprite = image;
		banner.gameObject.SetActive(true);
	}

	public void ShowVideo()
	{
		//Debug.Log("Show Video");
		string videoPath = ACEPlay.CrossPromotion.CrossPromotionController.instance.GetVideoURL();
		if (string.IsNullOrEmpty(videoPath))
		{
			Debug.Log("No Video");
			videoPlayer.transform.parent.gameObject.SetActive(false);
		}
		else
		{
			Debug.Log("Video");
			//meshVideo.sortingLayerID = 2;
			videoPlayer.transform.parent.gameObject.SetActive(true);
			videoPlayer.url = ACEPlay.CrossPromotion.CrossPromotionController.instance.GetVideoURL();
			videoPlayer.Play();
		}
	}

	public void ShowVideoEndGame()
	{
		//Debug.Log("Show Video");
		string videoPath = ACEPlay.CrossPromotion.CrossPromotionController.instance.GetVideoURL();
		if (string.IsNullOrEmpty(videoPath))
		{
			Debug.Log("No Video");
			videoPlayerEndGame.transform.parent.gameObject.SetActive(false);
		}
		else
		{
			Debug.Log("Video");
			videoPlayerEndGame.transform.parent.gameObject.SetActive(true);
			videoPlayerEndGame.url = ACEPlay.CrossPromotion.CrossPromotionController.instance.GetVideoURL();
			videoPlayerEndGame.Play();
		}
	}

#if UNITY_ANDOID || UNITY_IOS
	public void clickCrossPromotionGame()
    {
        ACEPlay.CrossPromotion.CrossPromotionController.instance.SetIndexVideoOnList();
        string url = "";
#if UNITY_ANDROID
        url = ACEPlay.CrossPromotion.CrossPromotionController.instance.AndroidAppPackage;
#elif UNITY_IOS
		 url = ACEPlay.CrossPromotion.CrossPromotionController.instance.IOSAppID;
#endif
        if (!url.Equals(""))
        {
            ACEPlay.Bridge.BridgeController.instance.TrackingDataGame("cross_promotion");
            Application.OpenURL(url);
        }
    }
#endif
	public void _PressedCloseVideoMain()
	{
		videoPlayer.transform.parent.gameObject.SetActive(false);
	}
	public void _PressedCloseVideoEndGame()
	{
		videoPlayerEndGame.transform.parent.gameObject.SetActive(false);
	}
}
