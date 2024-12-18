using UnityEngine;
using System.Collections;

public class Alien1 : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float moveSpeed;
    public float rotationSpeed;
    public float engagementRange = 20f;
    public float fireRate = 1f;
    public bool isCharging;
    public float chargeTime;
    public Vector3 maxBulletScale;
    public float health;
    public GameObject explosionPrefab;

    public Alien1points pointsScript;

    private Transform player;
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private float camWidth;
    private float camHeight;
    private float randomMovementTimer;
    private Vector2 randomDirection;
  
    
    //upgrade drop
  public GameObject upgradePrefab;
    public float upgradeDropChance = 0.2f; // 20% drop chance

    private void Awake()
    {
        pointsScript = GetComponent<Alien1points>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        camWidth = mainCamera.orthographicSize * mainCamera.aspect;
        camHeight = mainCamera.orthographicSize;
        FindPlayer();
        GenerateRandomMovement();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player)
        {
            RotateTowardsPlayer();
            HandleMovement();
        }
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Ship");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void HandleMovement()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Dodge bullets if there are any nearby
        Vector2 dodgeDirection = GetDodgeDirection();
        if (dodgeDirection != Vector2.zero)
        {
            rb.velocity = dodgeDirection.normalized * moveSpeed;
        }
        // If within engagement range, engage the player
        else if (distanceToPlayer < engagementRange)
        {
            EngagePlayer(directionToPlayer, distanceToPlayer);
        }
        // If out of engagement range, move towards the player
        else
        {
            MoveTowardsPlayer(directionToPlayer);
        }

      
        StayWithinCameraBounds();

        // Add a bit of randomness to the movement already engaging the player
        ApplyRandomMovement();
    }

    private void EngagePlayer(Vector2 directionToPlayer, float distanceToPlayer)
    {
        if (distanceToPlayer > 1)
        {
            // Move away from the player if too close
            rb.velocity = -directionToPlayer.normalized * moveSpeed;
        }
        else
        {
            // Stop movement if very close
            rb.velocity = Vector2.zero;
        }

        // Handle firing at the player
        if (!isCharging && Time.time > nextFireTime)
        {
            StartCoroutine(ChargeAndFire());
        }
    }

    private void MoveTowardsPlayer(Vector2 directionToPlayer)
    {
        rb.velocity = directionToPlayer.normalized * moveSpeed;
    }

    private void StayWithinCameraBounds()
    {
        Vector3 position = transform.position;

        // Clamp position 
        if (position.x > camWidth || position.x < -camWidth || position.y > camHeight || position.y < -camHeight)
        {
            
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            rb.velocity = directionToPlayer * moveSpeed;

           
            transform.position = new Vector3(Mathf.Clamp(position.x, -camWidth, camWidth), Mathf.Clamp(position.y, -camHeight, camHeight), position.z);
        }
    }

    private void ApplyRandomMovement()
    {
        randomMovementTimer -= Time.deltaTime;

        if (randomMovementTimer <= 1.0f)
        {
            GenerateRandomMovement();
        }

        // Apply random movement 
        rb.velocity += randomDirection * moveSpeed * 0.1f;
    }

    private void GenerateRandomMovement()
    {
        randomMovementTimer = Random.Range(1f, 3f); // Change direction 
        randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private Vector2 GetDodgeDirection()
    {
        int playerBulletLayer = LayerMask.GetMask("PlayerBullet");
        Collider2D[] nearbyBullets = Physics2D.OverlapCircleAll(transform.position, 5f, playerBulletLayer);
        Vector2 dodgeDirection = Vector2.zero;

        foreach (var bullet in nearbyBullets)
        {
            Vector2 bulletDirection = bullet.GetComponent<Rigidbody2D>().velocity.normalized;

            // If the bullet is heading towards the alien, dodge it
            float angleToBullet = Vector2.Angle(transform.up, bulletDirection);

            if (angleToBullet < 90f) // Consider dodging if the bullet is heading toward the alien
            {
                dodgeDirection += Vector2.Perpendicular(bulletDirection) * (Random.value > 0.5f ? 1 : -1);
            }
        }

        return dodgeDirection;
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float step = rotationSpeed * Time.deltaTime;
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, step);
        rb.rotation = angle;
    }

    private IEnumerator ChargeAndFire()
    {
        isCharging = true;
        AudioManager.Instance.PlaySFX("Alien1 Charge", .22f);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity, firePoint);
        bullet.transform.localScale = Vector3.one;

        float chargeProgress = 0;
        while (chargeProgress < chargeTime)
        {
            if (bullet == null)
            {
                AudioManager.Instance.StopSFX("Alien1 Charge");
                isCharging = false;
                yield break;
            }

            bullet.transform.localScale = Vector3.Lerp(Vector3.one, maxBulletScale, chargeProgress / chargeTime);
            chargeProgress += Time.deltaTime;
            yield return null;
        }

        if (bullet != null)
        {
            AudioManager.Instance.StopSFX("Alien1 Charge");
            AudioManager.Instance.PlaySFX("Alien1 Shoot", .30f);

            bullet.transform.parent = null;
            bullet.GetComponent<Rigidbody2D>().velocity = firePoint.up * 10;
        }

        isCharging = false;
        nextFireTime = Time.time + 1f / fireRate;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (pointsScript != null)
            {
                pointsScript.AwardPoints();
            }
            AudioManager.Instance.PlaySFX("explosion", 1.5f);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Debug.Log("Alien destroyed");
            
            // 20% chance to drop an upgrade
            if (Random.value <= upgradeDropChance && upgradePrefab != null)
            {
                Instantiate(upgradePrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int playerBulletLayer = LayerMask.NameToLayer("PlayerBullet");
        if (collision.gameObject.layer == playerBulletLayer)
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }
}
