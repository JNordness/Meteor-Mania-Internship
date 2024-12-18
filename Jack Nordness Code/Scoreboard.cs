using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance { get; private set; }
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    public int score = 0;
    private int highscore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load high score 
        highscore = PlayerPrefs.GetInt("highscore", 0);
        UpdateScoreText();
        UpdateHighscoreText();
        StartCoroutine(IncrementScoreEverySecond());
    }

    IEnumerator IncrementScoreEverySecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            IncrementScore(1);
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    void UpdateHighscoreText()
    {
        highscoreText.text = "Highscore: " + highscore.ToString();
    }

    public void IncrementScore(int amount)
    {
        score += amount;

        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt("highscore", highscore);
            PlayerPrefs.Save();
            UpdateHighscoreText();

            // Submit the high score to the leaderboard
            SubmitScoreToLeaderboard();
        }

        UpdateScoreText();
    }

    
    private async void SubmitScoreToLeaderboard()
    {
        string leaderboardID = "InternshipLeaderboard"; // Replace with your actual leaderboard ID

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, highscore);
            Debug.Log("High score submitted to leaderboard successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to submit high score: " + e.Message);
        }
    }
}
