using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Ship : MonoBehaviour
{
    [SerializeField] public float lives = 2;
    [SerializeField] float immunityDuration = 2.0f; 
    [SerializeField] float flashInterval = 0.1f; 
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private List<Image> heartImages; 
        [SerializeField] private Sprite emptyHeart;
    private bool isImmune = false; 
    private SpriteRenderer spriteRenderer; 

    public GameObject gameOverScreen; 
        public TextMeshProUGUI pointsText; 
        public Button tryAgainButton; 
    private int score; // score variable
    public delegate void DamageTakenDelegate();
    public event DamageTakenDelegate OnDamageTaken;
    private UnityRewarded unityRewarded;



    public GameObject explosionPrefab;  


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
        else
        {
            Debug.LogError("GameOverScreen not assigned error");
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("TryAgainButton is not assigned");
        }
    }
         public void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < lives)
            {
                heartImages[i].sprite = fullHeart; // Show heart
            }
            else
            {
                heartImages[i].sprite = emptyHeart; // Show empty heart
            }

            heartImages[i].enabled = true; // Ensure the heart is visible
        }
    }


    public void AddHealth(int amount)
    {
        lives += amount;

        // Ensure lives do not exceed the maximum number of hearts
        if (lives > heartImages.Count)
        {
            lives = heartImages.Count;
        }

        UpdateHearts(); // Refresh the hearts UI
    }



    public void TakeDamage()
    {
        if (!isImmune)
        {
            lives--;
             UpdateHearts(); // Update hearts UI
            if (lives <= 0)
             
            {
                AudioManager.Instance.StopMusic(); // Stop background music 
                AudioManager.Instance.PlaySFX("explosion");
          
                Debug.Log("Game Over");
                if (gameOverScreen != null)
                {
                    gameOverScreen.SetActive(true);
                    AudioManager.Instance.PlaySFX("Gameover");
                    Debug.Log("GameOverScreen Activated");
                    if (pointsText != null)
                    {
                        //get score from scoreboard
                        score = Scoreboard.Instance.score;
                        pointsText.text = "Score: " + score.ToString();
                        Debug.Log("PointsText updated: " + pointsText.text);
                    }
                    else
                    {
                        Debug.LogError("PointsText is not assigned");
                    }
                }
                else
                {
                    Debug.LogError("GameOverScreen is not assigned!");
                }
                Time.timeScale = 0f; // Pause game
            }
            else
            {
                StartCoroutine(ImmunityFrames());
                OnDamageTaken?.Invoke();
            }
        }
    }
    private void OnDestroy()
    {
       
        if (unityRewarded != null)
        {
            unityRewarded.OnAdCompleted -= GrantRewardLives;
        }
    }

   
    public void GrantRewardLives()
    {
        lives += 2;
        Debug.Log($"Player granted 2 extra lives. Current lives: {lives}");

        gameObject.SetActive(true);  // Reactivate the Ship GameObject
        Time.timeScale = 1f;  // Resume the game


        StartCoroutine(ImmunityFrames());  // Start immunity frames

    }
    private IEnumerator ImmunityFrames()
    {
        isImmune = true;
        float elapsedTime = 0f;

        while (elapsedTime < immunityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; 
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        spriteRenderer.enabled = true; // Ensure the sprite is visible at the end
        isImmune = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Ship");
        int asteroidLayer = LayerMask.NameToLayer("asteroid");
        int bulletLayer = LayerMask.NameToLayer("PlayerBullet");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        int missleLayer = LayerMask.NameToLayer("Kaze");
        int pulsebulletLayer = LayerMask.NameToLayer("pulsebullet");
        int AlienLayer = LayerMask.NameToLayer("Alien");
        int CometLayer = LayerMask.NameToLayer("Comet");
        if (collision.gameObject.layer == asteroidLayer)
        {
            TakeDamage();
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
        else if (collision.gameObject.layer == enemyBulletLayer)
        {
            TakeDamage();
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
        else if (collision.gameObject.layer == pulsebulletLayer)
        {
            TakeDamage();
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
        else if (collision.gameObject.layer == AlienLayer)
        {
            TakeDamage();
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
        else if (collision.gameObject.layer == missleLayer)
        {
            TakeDamage();
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);  // Play explosion animation
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
        else if (collision.gameObject.layer == CometLayer)
        {
            TakeDamage();
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);  // Play explosion animation
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("ship hits", 1f);
        }
    }
    private void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the scene
    }
    public void AddScore(int points)
    {
        score += points;
       
    }
}
