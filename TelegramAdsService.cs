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
    
    [SerializeField] private string interFirstId;
    [SerializeField] private string interDefaultId;
    [SerializeField] private string rewardedId;
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void InitBlockJs(string key, string id);
    [DllImport("__Internal")] private static extern void ShowBlockJs(string key, int id, string goName, string callbackName);
#endif

    private List<Action<string>> _adFeedbacks = new List<Action<string>>();
    
    public bool Initialized { get; }
    public bool InterstitialAdAvailable { get; }
    public bool RewardedAdAvailable { get; }
    public bool BannerShowing { get; }
    public float BannerHeight { get; }
    
    public event Action EventBannerShown;
    public event Action EventBannerHidden;
    
    public override void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitBlockJs(INTER_FIRST, interFirstId);
        InitBlockJs(INTER_DEFAULT, interDefaultId);
        InitBlockJs(REWARDED, rewardedId);
#endif  
    }
    
    public void ShowInterstitial(bool first = false)
    {
        string adKey = first ? INTER_FIRST : INTER_DEFAULT;
        _adFeedbacks.Add((result) =>
        {
            TelegramBridge.Instance.ShowTelegramAlert(result);
        });
        
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowBlockJs(adKey, _adFeedbacks.Count - 1, gameObject.name, "CatchBlockFeedback");
#endif  
    }

    public void ShowRewarded(Action onAdShowStart, Action onAdShowClick, Action onAdShowComplete, Action onAdShowFailed)
    {
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

    private void CatchBlockFeedback(string data)
    {
        string[] split = data.Split('/');
        int index = int.Parse(split[0]);
        string result = split[1];
        
        _adFeedbacks[index].Invoke(result);
    }
}
