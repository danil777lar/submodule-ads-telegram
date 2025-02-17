using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Larje.Core;
using Larje.Core.Services;
using UnityEngine;
using UnityEngine.Serialization;

[BindService(typeof(IAdsService))]
public class TelegramAdsService : Service, IAdsService
{
    private const string INTER_FIRST = "interstitial_first";
    private const string INTER_DEFAULT = "interstitial_default";
    private const string REWARDED = "rewarded";

    [SerializeField] private bool useAds;
    [Space]
    [SerializeField] private string interFirstId;
    [SerializeField] private string interDefaultId;
    [SerializeField] private string rewardedId;
    [Space] 
    [SerializeField] private float interDelay = 30;
    [SerializeField] private bool debugMode;
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void InitBlockJs(string key, string id, int debugMode);
    [DllImport("__Internal")] private static extern void ShowBlockJs(string key, int id, string goName, string callbackName);
#endif

    private float _interDelay = -1f;
    private List<Action<string>> _adFeedbacks = new List<Action<string>>();
    
    public bool Initialized { get; private set; }
    public bool InterstitialAdAvailable { get; private set; }
    public bool RewardedAdAvailable { get; private set; }
    public bool BannerShowing { get; private set; }
    public float BannerHeight { get; private set; }
    
    public event Action EventBannerShown;
    public event Action EventBannerHidden;
    
    public override void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!string.IsNullOrEmpty(interFirstId)) 
        {
            InitBlockJs(INTER_FIRST, interFirstId, debugMode ? 1 : 0);
            InterstitialAdAvailable = true;
        }

        if (!string.IsNullOrEmpty(interDefaultId)) 
        {
            InitBlockJs(INTER_DEFAULT, interDefaultId, debugMode ? 1 : 0);
            InterstitialAdAvailable = true;
        }

        if (!string.IsNullOrEmpty(rewardedId)) 
        {
            InitBlockJs(REWARDED, rewardedId, debugMode ? 1 : 0);
            RewardedAdAvailable = true;
        }
#endif
        
        Initialized = true;
    }

    public void ShowInterstitial(int interIndex = 0)
    {
        if (!useAds)
        {
            return;
        }

        if (_interDelay <= 0f)
        {
            _interDelay = interDelay;

            string adKey = interIndex == 0 ? INTER_DEFAULT : INTER_FIRST;
            _adFeedbacks.Add((result) => { });

#if UNITY_WEBGL && !UNITY_EDITOR
        ShowBlockJs(adKey, _adFeedbacks.Count - 1, gameObject.name, "CatchBlockFeedback");
#endif
        }
    }

    public void ShowRewarded(Action onAdShowStart, Action onAdShowClick, Action onAdShowComplete, Action onAdShowFailed)
    {
        if (!useAds)
        {
            return;
        }
        
        string adKey = REWARDED;
        _adFeedbacks.Add((result) =>
        {
            if (result == "complete")
            {
                onAdShowComplete?.Invoke();
            }
            else
            {
                onAdShowFailed?.Invoke();
            }
        });
        
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowBlockJs(adKey, _adFeedbacks.Count - 1, gameObject.name, "CatchBlockFeedback");
#endif  
    }

    private void Update()
    {
        if (_interDelay > 0f)
        {
            _interDelay -= Time.deltaTime;
        }
    }

    private void CatchBlockFeedback(string data)
    {
        string[] split = data.Split('/');
        int index = int.Parse(split[0]);
        string result = split[1];
        
        _adFeedbacks[index].Invoke(result);
    }
}
