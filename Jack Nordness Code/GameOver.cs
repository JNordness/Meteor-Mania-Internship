using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public Button tryAgainButton;

    private void Start()
    {
        gameObject.SetActive(false); // Ensure the GameOver screen is inactive at the begining 
        tryAgainButton.onClick.AddListener(RestartGame);
    }

    public void Setup(int score)
    {
        gameObject.SetActive(true);
        pointsText.text = score.ToString() + " POINTS";
        Time.timeScale = 0f; // Pause the game
    }

    private void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
