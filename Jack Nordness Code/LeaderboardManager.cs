using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System.Threading.Tasks;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContent; // The Content for Scroll View
    [SerializeField] private GameObject leaderboardEntryPrefab; // Prefab player
        private string leaderboardID = "InternshipLeaderboard"; 

    // Fetch and display leaderboard data
    public async void DisplayLeaderboard()
    {
        // Clear previous leaderboard entries
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        try
        {
            // Fetch scores from Unity 
            LeaderboardScoresPage scoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);

            int rank = 1; // place counter

            // Populate the leaderboard 
            foreach (LeaderboardEntry entry in scoresPage.Results)
            {
                GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardContent);

                // Add rank before the player's name
                leaderboardEntry.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = $"{rank}. {entry.PlayerName}";

                // Set the player's score
                leaderboardEntry.transform.Find("PlayerScore").GetComponent<TextMeshProUGUI>().text = entry.Score.ToString();

                rank++; // Increment rank for the next player
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to fetch leaderboard data: " + e.Message);
        }
    }
}
