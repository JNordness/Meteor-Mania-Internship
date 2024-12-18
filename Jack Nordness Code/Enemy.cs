using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnEnemyKilled;
    public float health, maxHealth = 1.0f;
    public float moveSpeed = 3.0f;
    public float directionChangeInterval = 2.0f;
    public float randomDirectionInfluence = 0.5f;
    public float directionSmoothTime = 0.5f;
    [SerializeField] float boundaryBuffer = .1f; 
    [SerializeField] float impulseForce = 100f; // Impulse force for asteroid

    public bool isDestroyed = false;
    public GameObject explosionPrefab;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 currentDirection;
    private Vector2 screenBoundsX;
    private Vector2 screenBoundsY;
     public GameObject dropPrefab1;
     public GameObject dropPrefab2;
     public GameObject dropPrefab3;
     public float dropChance = 0.01f; // 1% chance to drop


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        health = maxHealth;
        moveDirection = Vector2.zero;
        currentDirection = Vector2.zero;
        CalculateScreenBounds(); // Calculate screen bounds on start
        StartCoroutine(ChangeDirectionRoutine());
    }

    private void Update()
    {
        currentDirection = Vector2.Lerp(currentDirection, moveDirection, directionSmoothTime * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        rb.velocity = currentDirection * moveSpeed;
        ClampPosition();
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            ChangeDirection();
            yield return new WaitForSeconds(directionChangeInterval);
        }
    }

    private void ChangeDirection()
    {
        Vector2 randomDirection = new Vector2(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        Vector2 randomInfluence = new Vector2(
            UnityEngine.Random.Range(-randomDirectionInfluence, randomDirectionInfluence),
            UnityEngine.Random.Range(-randomDirectionInfluence, randomDirectionInfluence)
        );

        moveDirection = (randomDirection + randomInfluence).normalized;
    }

    private void CalculateScreenBounds()
    {
        // Get screen bounds in world space and add a buffer
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        screenBoundsX = new Vector2(bottomLeft.x - boundaryBuffer, topRight.x + boundaryBuffer);
        screenBoundsY = new Vector2(bottomLeft.y - boundaryBuffer, topRight.y + boundaryBuffer);
    }

private void ClampPosition()
{
    Vector2 clampedPosition = new Vector2(
        Mathf.Clamp(transform.position.x, screenBoundsX.x, screenBoundsX.y),
        Mathf.Clamp(transform.position.y, screenBoundsY.x, screenBoundsY.y)
    );

    // Check if the asteroid is at the screen boundary
    bool isAtBoundaryX = transform.position.x <= screenBoundsX.x || transform.position.x >= screenBoundsX.y;
    bool isAtBoundaryY = transform.position.y <= screenBoundsY.x || transform.position.y >= screenBoundsY.y;

    if (isAtBoundaryX || isAtBoundaryY)
    {
        // Calculate center of the screen
        Vector2 screenCenter = Vector2.zero; // assuming (0,0) is the center in world space
        Vector2 directionToCenter = (screenCenter - (Vector2)transform.position).normalized;

        
        moveDirection = directionToCenter;
        
        // Apply a stronger force to push the asteroid back towards the center
        float centerForceMultiplier = 2.0f; // Increase this for a stronger push effect
        rb.AddForce(directionToCenter * impulseForce * centerForceMultiplier, ForceMode2D.Impulse);
    }

    // Apply the clamped position 
    transform.position = clampedPosition;
}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Ship");
        int asteroidLayer = LayerMask.NameToLayer("asteroid");
        int bulletLayer = LayerMask.NameToLayer("PlayerBullet");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");

        if (collision.gameObject.layer == playerLayer)
        {
            TakeDamage(1);
            AudioManager.Instance.PlaySFX("Astroid hit", .75f);
        }
        else if (collision.gameObject.layer == asteroidLayer)
        {
            Vector2 impulseDirection = (collision.transform.position - transform.position).normalized;
            rb.AddForce(impulseDirection * impulseForce, ForceMode2D.Impulse);
        }
        else if (collision.gameObject.layer == bulletLayer)
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
            AudioManager.Instance.PlaySFX("Astroid hit", .75f);
        }
        else if (collision.gameObject.layer == enemyBulletLayer)
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
            AudioManager.Instance.PlaySFX("Astroid explode", .75f);
        }
    }

    private void Die()
    {
        OnEnemyKilled?.Invoke(this);
        AsteroidPoints asteroidPoints = GetComponent<AsteroidPoints>();
        if (asteroidPoints != null)
        {
            asteroidPoints.AwardPoints();
        }

        Destroy(gameObject);
        DropItem();
    }
     private void DropItem()
    {
        float randomValue = UnityEngine.Random.value;
        if (randomValue <= dropChance)
        {
            GameObject selectedPrefab = null;
            int randomIndex = UnityEngine.Random.Range(0, 3);
            switch (randomIndex)
            {
                case 0:
                    selectedPrefab = dropPrefab1;
                    break;
                case 1:
                    selectedPrefab = dropPrefab2;
                    break;
                case 2:
                    selectedPrefab = dropPrefab3;
                    break;
            }

            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
