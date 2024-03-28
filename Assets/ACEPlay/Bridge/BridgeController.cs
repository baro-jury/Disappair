using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ACEPlay.Bridge
{
    public class BridgeController : MonoBehaviour
    {
        public static BridgeController instance;
        public System.Action ACTION_SHOW_NATIVE;
        public System.Action ACTION_HIDE_NATIVE;
        public System.Action<int> ACTION_LOAD_NATIVE;
        public bool IsVipComplete
        {
            get
            {
                return PlayerPrefs.GetInt("IsVip", 0) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("IsVip", value ? 1 : 0);
            }
        }
        public bool CanShowAdsAOA //true: show ads, false: not show ads
        {
            get
            {
                return PlayerPrefs.GetInt("CanShowAdsAOA", 0) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("CanShowAdsAOA", value ? 1 : 0);
            }
        }
        public bool CanShowAds //true: show ads, false: not show ads
        {
            get
            {
                return PlayerPrefs.GetInt("CanShowAds", 1) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("CanShowAds", value ? 1 : 0);
            }
        }
        public bool CanShowAdsWithVip //true: show ads, false: not show ads
        {
            get
            {
                return PlayerPrefs.GetInt("CanShowAdsWithVip", 1) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("CanShowAdsWithVip", value ? 1 : 0);
            }
        }

        public bool IsShowVipAtStart
        {
            get
            {
                return PlayerPrefs.GetInt("IsShowVipAtStart", 0) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("IsShowVipAtStart", value ? 1 : 0);
            }
        }

        public bool IsBuyProduct
        {
            get
            {
                return PlayerPrefs.GetInt("IsBuyProduct", 0) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("IsBuyProduct", value ? 1 : 0);
            }
        }
        public bool ischeckFirebase;
        public bool maxAds = false;
        public bool isLoadGameSuccess = false;
        public List<string> NonConsumableList = new List<string>();
        public List<string> VipPackageList = new List<string>();

        public int levelCountShowAds = 2;
        public int levelCountShowAdsCurrent = 0;
        private void Awake()
        {
            //PlayerPrefs.DeleteAll();
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else Destroy(this.gameObject);
        }

        private void Start()
        {
           
        }
        public void FirstLogin()
        {
            if (PlayerPrefs.HasKey("firstLogin"))
            {
                PlayerPrefs.SetInt("firstLogin", 1);
            }
            isLoadGameSuccess = true;
        }
      
        public void AddNonConsumableLists(string productSku)
        {
            if (!NonConsumableList.Contains(productSku))
            {
                NonConsumableList.Add(productSku);
            }
        }
        public void AddVipPackageLists(string productSku)
        {
            if (!VipPackageList.Contains(productSku))
            {
                VipPackageList.Add(productSku);
            }
        }
        public void ShowBeginPack()
        {
            //if (IsShowVipAtStart )
            //{

            //}
            //else
            //    return;
        }
        public bool CheckShowInter()
        {
            return true;
            //return ServiceController.instance.Max.IsInterstitialAvailable() && CheckTimeShowInter();
        }
        public bool CheckTimeShowInter()
        {
            return true;
            // return (ServiceController.instance.Max.isCheckTimeShowInter() || (levelCountShowAdsCurrent >= levelCountShowAds)) && DontDestroyOnload.instance.level >= ServiceController.instance.levelAllowADS;
        }
        public void ShowBannerAd()
        {
            if (CanShowAds)
            {
                Debug.Log("=====Banner Show success!=====");
            }
            else
            {
                HideBannerAD();
            }
        }
        public void HideBannerAD()
        {
            Debug.Log("=====Banner Hide success!=====");

        }
      
        public bool ShowIntersitialAd(UnityEvent onClosed)
        {
            if (CanShowAds)
            {
                Debug.Log("=====Intersitial Show success!=====");
                if (onClosed != null) onClosed.Invoke();
                return true;
            }
            else
            {
                if (onClosed != null) onClosed.Invoke();
                return false;
            }

        }

        public bool ShowRewardedAd(UnityEvent onRewarded, UnityEvent onClosed)
        {
            Debug.Log("=====Rewarded Show success!=====");
            if (onRewarded != null) onRewarded.Invoke();
            return true;
      
        }
        public void PurchaseProduct(string productSku, UnityStringEvent onDonePurchaseEvent)
        {
            Debug.Log("=====Purchase success!=====" + productSku);
            if (onDonePurchaseEvent != null) onDonePurchaseEvent.Invoke(productSku);
        }

        public void RestorePurchase()
        {
            Debug.Log("=====Restore Purchase success!=====");
            
        }

        public void RateGame(UnityEvent onRateRewarded)
        {
            Debug.Log("=====Rate Success=====");
            if (onRateRewarded != null) onRateRewarded.Invoke();
        }

        public void ShareGame(UnityEvent onShareRewarded)
        {
            Debug.Log("=====Share Success=====");
            if (onShareRewarded != null) onShareRewarded.Invoke();
           
        }
        public void Moregames()
        {
            Debug.Log("=====Show Moregames success!=====");

        }
        public void CheckTutComplete(bool value = false)
        {
            Debug.Log("=====Tuturial success!=====");
        }
        public void TrackingDataAppflyer(string eventName, Dictionary<string, string> parameterTracking)
        {

        }
        public void TrackingDataGame(string index)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (ischeckFirebase)
            {
                Debug.Log("=====TrackingDataGame success! =====" + index);
            }
#endif

        }
      

        public void TrackingDataGame(string eventName, Parameter[] parameterTracking)
        {
           // Debug.Log(string.Format("=====TrackingDataGame success!===== eventName: {0}, parameters: {1}",eventName, JsonConvert.SerializeObject(parameterTracking)));
#if UNITY_ANDROID || UNITY_IOS
            if (ischeckFirebase)
            {
               
            }
#endif
        }

        public void UserPropertyData(string properties, string value)
        {
            Debug.Log(string.Format("=====Set User Property: {0} Data: {1}===== ", properties, value));
        }
        public void UserPropertyData(string properties, int value)
        {
            Debug.Log(string.Format("=====Set User Property: {0} Data: {1}===== ", properties, value));
        }
        public void PublishHighScore(int score)
        {
            Debug.Log("=====Publish Score:" + score + "Success=====");
        }
        //hien thi ban xep hang
        public void ShowLeaderBoard()
        {
            Debug.Log("=====Show Leaderboard success!=====");
        }

        public void ShowFacebook(UnityEvent onOpenFBRewarded)
        {
            Debug.Log("=====Show Facebook Success=====");
            if (onOpenFBRewarded != null) onOpenFBRewarded.Invoke();

        }

        public void ShowInstagram(UnityEvent onInstaRewarded)
        {
            Debug.Log("=====Show Instagram Success=====");
            if (onInstaRewarded != null) onInstaRewarded.Invoke();

        }

        public void ShowTwitter(UnityEvent onTwitterRewarded)
        {
            Debug.Log("=====Show Twitter Success=====");
            if (onTwitterRewarded != null) onTwitterRewarded.Invoke();
        }
        public void ShowWebsite()
        {
            Debug.Log("=====Show Website success!=====");
        }
    }
}
public class Parameter
{
    public string key = "";
    public string string_value = "";
    public int int_value = -1;
    public float float_value = -1;

    public Parameter(string key, string value)
    {
        this.key = key;
        this.string_value = value;
    }
    public Parameter(string key, int value)
    {
        this.key = key;
        this.int_value = value;
    }
    public Parameter(string key, float value)
    {
        this.key = key;
        this.float_value = value;
    }
}

[System.Serializable]
public class UnityStringEvent : UnityEvent<string>
{
}


