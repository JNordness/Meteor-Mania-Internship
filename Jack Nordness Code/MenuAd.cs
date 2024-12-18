using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Advertisements; 
using System;

public class MenuAd : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public string gameID = "";          //  Game ID
    public string rewardedID = "";     

    [SerializeField] private bool _testMode = true;

    [SerializeField] private List<PowerUpEffect> upgrades; // List of power-ups to select from
    private PowerUpEffect pendingReward;                  // store selected power-up

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
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

   public void LoadAd()
    {
        Advertisement.Load(rewardedID, this);
    }

    public void ShowAd()
    {
        Advertisement.Show(rewardedID, this);
    }


   
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Ad Loaded: {adUnitId}");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == rewardedID && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Ad successfully completed!");

            // Select a random reward 
            pendingReward = SelectRandomUpgrade();

            // Switch to the game scene
            SceneManager.sceneLoaded += OnSceneLoaded; 
            SceneManager.LoadScene("GameScene");       
        }
        else
        {
            Debug.Log("Ad skipped or failed. No reward granted.");
        }
    }

    private PowerUpEffect SelectRandomUpgrade()
    {
        if (upgrades == null || upgrades.Count == 0)
        {
            Debug.LogWarning("No upgrades available to grant as a reward.");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, upgrades.Count);
        Debug.Log($"Selected upgrade: {upgrades[randomIndex].name}");
        return upgrades[randomIndex];
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene" && pendingReward != null) // Check if it's the game scene
        {
            // Find the Ship and apply reward
            Ship ship = FindObjectOfType<Ship>();
            if (ship != null)
            {
                pendingReward.Apply(ship.gameObject);
                Debug.Log($"Granted power-up: {pendingReward.name}");
            }
            else
            {
                Debug.LogWarning("Ship not found in the game scene. Cannot apply reward.");
            }

            // Clear the pending reward 
            pendingReward = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
