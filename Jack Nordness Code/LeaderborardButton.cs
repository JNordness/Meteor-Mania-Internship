using UnityEngine;

public class LeaderboardButton : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardImage; 
    [SerializeField] private LeaderboardManager leaderboardManager; 

    private bool isLeaderboardVisible = false; // leaderboard is inactive until button is pressed

    public void ToggleLeaderboard()
    {
        Debug.Log("ToggleLeaderboard button pressed.");

        isLeaderboardVisible = !isLeaderboardVisible; 

        if (leaderboardImage == null)
        {
            Debug.LogError("LeaderboardButton: 'leaderboardImage' is not assigned in the Inspector.");
            return;
        }

        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardButton: 'leaderboardManager' is not assigned in the Inspector.");
            return;
        }

        if (isLeaderboardVisible)
        {
            Debug.Log("Attempting to show leaderboard.");

            leaderboardImage.SetActive(true); // Show leaderboard
            Debug.Log($"LeaderboardImage active state after SetActive(true): {leaderboardImage.activeSelf}");

            leaderboardManager.DisplayLeaderboard(); 
            Debug.Log("LeaderboardManager: DisplayLeaderboard called.");
        }
        else
        {
            Debug.Log("Attempting to hide leaderboard.");

            leaderboardImage.SetActive(false); // Hide leaderboard
            Debug.Log($"LeaderboardImage active state after SetActive(false): {leaderboardImage.activeSelf}");
        }

        Debug.Log($"Leaderboard visibility is now: {isLeaderboardVisible}");
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable called for LeaderboardButton.");

        if (leaderboardImage != null && !isLeaderboardVisible)
        {
            leaderboardImage.SetActive(false); // Only disable if the leaderboard is not supposed to be visible
            Debug.Log("LeaderboardImage set to inactive on enable.");
        }
    }
}
