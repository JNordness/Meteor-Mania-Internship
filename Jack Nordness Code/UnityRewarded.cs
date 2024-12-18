using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Advertisements;

using System;

public class UnityRewarded : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener

{

   [SerializeField] private GameObject gameOverScreen; 

    public string gameID = "";
    public string rewardedID = "";

    [SerializeField] bool _testMode = true;
    public event Action OnAdCompleted;

    void Start()
    {
        InitializeAds();
        LoadAd();
    }

    public void InitializeAds()
    {
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(gameID, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void LoadAd()
    {
        Advertisement.Load(rewardedID, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Ad Loaded: {adUnitId}");
    }

    public void ShowAd()
    {
        Advertisement.Show(rewardedID, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == rewardedID && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Ad successfully completed.");

            Ship ship = FindObjectOfType<Ship>();  // Find the Ship 

            if (ship != null)
            {
                Debug.Log("Granting extra lives.");
                ship.GrantRewardLives();  // Grant lives and start immunity
                ship.UpdateHearts();
            }
            else
            {
                Debug.LogWarning("No active Ship instance found. Cannot grant lives.");
            }

  
            if (gameOverScreen != null)
            {
                gameOverScreen.SetActive(false);  
                Time.timeScale = 1f;            
                AudioManager.Instance.PlayMusic("background music");  
            }
            else
            {
                Debug.LogWarning("Game Over screen reference is missing.");
            }
        }
        else
        {
            Debug.Log("Ad skipped or failed. No reward granted.");
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}