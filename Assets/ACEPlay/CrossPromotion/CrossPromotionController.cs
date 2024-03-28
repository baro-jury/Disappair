using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ACEPlay.CrossPromotion
{
    public class CrossPromotionController : MonoBehaviour
    {
        public static CrossPromotionController instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else Destroy(this.gameObject);
        }

        public int day { get { return DateTime.Now.DayOfYear; } }

        int lastCheck
        {
            get
            {
                if (PlayerPrefs.HasKey("last_check_cross"))
                    return int.Parse(PlayerPrefs.GetString("last_check_cross"));
                return day;
            }
            set { PlayerPrefs.SetString("last_check_cross", value.ToString()); }
        }

        int indexVideoOnList
        {
            get { return PlayerPrefs.GetInt("indexVideoOnList", 0); }
            set { PlayerPrefs.SetInt("indexVideoOnList", value); }
        }

        string iconURL = "";
        string bannerURL = "";
        string videoURL = "";
        [SerializeField] List<string> videoURLList = new List<string>();
        [SerializeField] List<string> AndroidAppPackageList = new List<string>();
        [SerializeField] List<string> IOSAppIdList = new List<string>();
        public string AndroidAppPackage = "";
        public string IOSAppID = "";
        public Texture2D appIcon;
        public Texture2D appBanner;
        public bool EnableCrossPromotion;

        public int EnableVideoOnStart
        {
            get { return PlayerPrefs.GetInt("EnableVideoOnStart", 0); }
            set { PlayerPrefs.SetInt("EnableVideoOnStart", value); }
        }

        public int EnableVideoOnEndgame
        {
            get { return PlayerPrefs.GetInt("EnableVideoOnEndgame", 0); }
            set { PlayerPrefs.SetInt("EnableVideoOnEndgame", value); }
        }

        public int EnableIconOnMenu
        {
            get { return PlayerPrefs.GetInt("EnableIconOnMenu", 0); }
            set { PlayerPrefs.SetInt("EnableIconOnMenu", value); }
        }

        public int EnableIconOnEndgame
        {
            get { return PlayerPrefs.GetInt("EnableIconOnMenu", 0); }
            set { PlayerPrefs.SetInt("EnableIconOnMenu", value); }
        }

        public int EnableBannerOnSetting
        {
            get { return PlayerPrefs.GetInt("EnableBannerOnSetting", 0); }
            set { PlayerPrefs.SetInt("EnableBannerOnSetting", value); }
        }

        public int EnableBannerOnEndGame
        {
            get { return PlayerPrefs.GetInt("EnableBannerOnEndGame", 0); }
            set { PlayerPrefs.SetInt("EnableBannerOnEndGame", value); }
        }

        public string GetVideoURL()
        {
            if (videoURLList.Count != 0)
            {
                if (day != lastCheck)
                {
                    lastCheck = day;
                    SetIndexVideoOnList();
                }

                this.videoURL = videoURLList[indexVideoOnList];
                if (AndroidAppPackageList.Count != 0) AndroidAppPackage = AndroidAppPackageList[indexVideoOnList];
                if (IOSAppIdList.Count != 0) IOSAppID = IOSAppIdList[indexVideoOnList];
            }

            return videoURL;
        }

        public void SetIndexVideoOnList()
        {
            if (indexVideoOnList >= videoURLList.Count - 1)
                indexVideoOnList = 0;
            else
                indexVideoOnList++;
        }

        [HideInInspector] public int isReadyVideo = 0;

        public void SetUrl(string icon, string banner, string videoList, string androidList, string iosList,
            int startVideo, int endVideo, int menuIcon, int endIcon, int settingBanner, int endgameBanner, int enable)
        {
            EnableVideoOnStart = startVideo;
            EnableVideoOnEndgame = endVideo;
            EnableIconOnMenu = menuIcon;
            EnableIconOnEndgame = endIcon;
            EnableBannerOnSetting = settingBanner;
            EnableBannerOnEndGame = endgameBanner;
            EnableCrossPromotion = enable == 1;
            this.iconURL = icon;
            this.bannerURL = banner;
            if (!string.IsNullOrEmpty(videoList))
            {
                foreach (string value in videoList.Split('-'))
                {
                    videoURLList.Add(value);
                }
            }
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(androidList))
            {
                foreach (string value in androidList.Split('-'))
                {
                    AndroidAppPackageList.Add(value);
                }
            }
#elif UNITY_IOS
            if (!string.IsNullOrEmpty(iosList))
            {
                foreach (string value in iosList.Split('~'))
                {
                    IOSAppIdList.Add(value);
                }
            }
#endif

            GetImage(iconURL, (result) => { appIcon = result; });
            GetImage(bannerURL, (result) => { appBanner = result; });
        }

        public bool CheckAppInstallation(string bundleId)
        {
#if UNITY_ANDROID
            bool installed = false;
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject curActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = curActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = null;
            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
                if (launchIntent == null) installed = false;
                else installed = true;
            }
            catch (System.Exception e)
            {
                installed = false;
            }
            return installed;
#elif UNITY_IOS
            return false;
#endif
            return false;
        }

        #region Loader

        public void GetText(string url, Action<string> callback)
        {
            string text = "";
            StartCoroutine(loadFileFromURL(url,
                (result) =>
                {
                    text = result.text;
                    if (callback != null) callback.Invoke(text);
                }));
        }

        public void GetImage(string url, Action<Texture2D> callback)
        {
            Texture2D texture = new Texture2D(1, 1);
            StartCoroutine(loadFileFromURL(url,
                (result) =>
                {
                    result.LoadImageIntoTexture(texture);
                    if (callback != null) callback.Invoke(texture);
                }));
        }

        public void GetVideo(string url, Action<string> callback)
        {
            string[] splitted = url.Split('/');
            string fileName = splitted[splitted.Length - 1];
            string path = Application.persistentDataPath + "/" + fileName;
            if (File.Exists(path))
            {
                if (callback != null) callback.Invoke(path);
            }
            else
            {
                StartCoroutine(loadFileFromURL(url,
                    (result) =>
                    {
                        File.WriteAllBytes(path, result.bytes);
                        if (callback != null) callback.Invoke(path);
                    }));
            }
        }

        IEnumerator loadFileFromURL(string url, Action<WWW> callback)
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (WWW www = new WWW(url))
                {
                    yield return www;
                    if (www.isDone && www != null)
                    {
                        callback.Invoke(www);
                    }
                }
            }
        }

        #endregion
    }
}