using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadePlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    
    public Rigidbody2D rb;
    private Vector2 moveDirection;
      private Ship shipScript;

    public GameObject bulletPrefab;
    public Transform firePoint;

    [Range(1, 10)]
    [SerializeField] public float fireRate = 1f;
    private float nextFireTime = 0f;

    

    public float acceleration = 10f;
    public float boostSpeedMultiplier = 2f;
    public float boostDuration = 1.5f;
    public float boostCooldown = 3f;
    private bool isBoosting = false;
    private float boostEndTime = 0f;
    public float boostCooldownEndTime = 0f;

    public SpriteRenderer spriteRenderer;
    public Transform additionalFirePoint1;
    public Transform additionalFirePoint2;
    public Transform mainFirePoint; 
    public bool additionalFirePointsActivated = false;
         public Sprite redShipSprite; // Variable to hold the new sprite

    // Colliders
    public Collider2D defaultCollider;
    public Collider2D redShipSpriteCollider;
    private Sprite originalSprite; // Store the original sprite
    private Collider2D originalCollider;

    public ParticleSystem particleSystem; 

    
    // Shotgun-related properties
    public float spreadAngle = 60f;
    public int bulletCount = 5;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 2f;
    public bool spreadFireActivated = false; // Variable to control spread fire activation
    public bool plasmaFireActivated = false; // Variable to control plasma activation
    public GameObject shotgunBullet;
    public GameObject plasmaBullet;
    public Sprite shotgunSprite;
    public Sprite plasmaSprite;
    public Collider2D shotgunSpriteCollider;
    public Collider2D plasmaSpriteCollider;
   
    public float speedbuffspeed = 0.0f;
    // Plasma charging-related properties
    public float maxChargeTime = 2f; // Maximum time for a full charge
    public float currentChargeTime = 0f; // Current charge time
    private bool isCharging = false;
    public Vector3 maxBulletScale = new Vector3(3f, 3f, 3f); // Scale of the fully charged bullet
     
    public float plasmaBulletSpeed = 20f; // Speed of the plasma bullet
       private Coroutine chargeCoroutine;
           private GameObject plasmaBulletInstance;

   
       
       
    public Transform FirePointPlasma;
    void Awake()
    {
        originalSprite = spriteRenderer.sprite;
        originalCollider = defaultCollider;
          shipScript = GetComponent<Ship>();
        if (shipScript != null)
        {
            shipScript.OnDamageTaken += HandleDamageTaken;
        }
        else
        {
            Debug.LogError("Ship script not found on the same GameObject.");
        }
    }

    void Update()
    {
        ProcessInputs();
        RotateTowardsMouse();
        HandleFiring();
        ClampPositionToBounds();
        CheckBoostInput();
         
    }

    void FixedUpdate()
    {
        Move();
    }

    void ProcessInputs()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        moveDirection = (mousePos - transform.position).normalized;
    }
void Move()
{
    // Calculate  speed
    float effectiveSpeed = moveSpeed + speedbuffspeed;
    
    //  boosting
    float targetSpeed = isBoosting ? effectiveSpeed * boostSpeedMultiplier : effectiveSpeed;

   

    Vector2 targetVelocity = moveDirection * targetSpeed;
    rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
}
    void RotateTowardsMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }

   void HandleFiring()
{
    if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
    {
        if (plasmaFireActivated)
        {
            if (!isCharging)
            {
                chargeCoroutine = StartCoroutine(ChargeAndFire());
            }
        }
        else
        {
            nextFireTime = Time.time + 1f / fireRate;
            Fire();
        }
    }

  if (Input.GetMouseButtonUp(0) && isCharging && plasmaFireActivated)
        {
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }
            FireCurrentBullet(plasmaBulletInstance);
            isCharging = false;
             
            nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
        }
}


       
    
    
 void Fire()
{
    // Regular fire when additionalFirePoints are not activated
    if (!additionalFirePointsActivated && firePoint != null && bulletPrefab != null && !spreadFireActivated && !plasmaFireActivated)
    {
        AudioManager.Instance.PlaySFX("playershoot", .25f);
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    // Fire from additional fire points if activated
    if (additionalFirePointsActivated)
    {
        if (additionalFirePoint1 != null)
        {
            Instantiate(bulletPrefab, additionalFirePoint1.position, additionalFirePoint1.rotation);
             AudioManager.Instance.PlaySFX("playershoot", .25f);
        }

        if (additionalFirePoint2 != null)
        {
            Instantiate(bulletPrefab, additionalFirePoint2.position, additionalFirePoint2.rotation);
        }
    }

    // Spread fire behavior
    if (spreadFireActivated && firePoint != null)
    {
        DeactivateAdditionalFirePoints();
        FireWithSpread(firePoint);
    }
}
   

  private void ClampPositionToBounds()
{
    // Calculate the screen bounds in world coordinates
    Vector3 screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.z));
    Vector3 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.transform.position.z));

    // Clamp the position using these bounds
    Vector3 clampedPosition = transform.position;
    clampedPosition.x = Mathf.Clamp(clampedPosition.x, screenBottomLeft.x, screenTopRight.x);
    clampedPosition.y = Mathf.Clamp(clampedPosition.y, screenBottomLeft.y, screenTopRight.y);

    transform.position = clampedPosition;
}

    void CheckBoostInput()
    {
        if (Input.GetMouseButton(1) && Time.time >= boostCooldownEndTime)
        {
            StartBoost();
        }
    }

    void StartBoost()
    {
        isBoosting = true;
        boostEndTime = Time.time + boostDuration;
        Invoke("EndBoost", boostDuration);
    }

    void EndBoost()
    {
        isBoosting = false;
        boostCooldownEndTime = Time.time + boostCooldown;
            // Check if plasma or shotgun is activated and adjust the speed 
    }

    public void ActivateAdditionalFirePoints()
    {
        additionalFirePointsActivated = true;
        spreadFireActivated = false;
        Debug.Log("Additional fire points activated");
        DeactivatePlasma();
        BaseSpeed();
    }

    public void ChangeSprite(Sprite redShipSprite)
    {
        if (spriteRenderer != null)
        {
            Debug.Log($"Changing sprite from {spriteRenderer.sprite.name} to {redShipSprite.name}");
            spriteRenderer.sprite = redShipSprite;
            Debug.Log("Sprite changed successfully");
            spriteRenderer.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            // Enable the new collider and disable the old one
            if (defaultCollider != null) defaultCollider.enabled = false;
            if (redShipSpriteCollider != null) redShipSpriteCollider.enabled = true;
            if (shotgunSpriteCollider != null) shotgunSpriteCollider.enabled = false;
            if (plasmaSpriteCollider != null) plasmaSpriteCollider.enabled = false;

            if (particleSystem != null)
            {
                AdjustParticleSystemPosition(particleSystem);
                Debug.Log("Particle system position adjusted to fit new sprite");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on player.");
        }
    }

    public void DeactivateAdditionalFirePoints()
    {
        additionalFirePointsActivated = false;
        Debug.Log("Additional fire points deactivated");
    }

    public void RevertToOriginalSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
            spriteRenderer.gameObject.transform.localScale = new Vector3(3f, 3f, 3f);

            if (defaultCollider != null) defaultCollider.enabled = true;
            if (redShipSpriteCollider != null) redShipSpriteCollider.enabled = false;
            if (shotgunSpriteCollider != null) shotgunSpriteCollider.enabled = false;
            if (plasmaSpriteCollider != null) plasmaSpriteCollider.enabled = false;
            if (particleSystem != null)
            {
                AdjustParticleSystemPositionBack(particleSystem);
                Debug.Log("Particle system position adjusted to fit original sprite");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on player.");
        }
    }

    private void AdjustParticleSystemPosition(ParticleSystem ps)
    {
        //Red ship particle adjustment
        // Set the particle system's local position to y = 0.9 relative 
        ps.transform.localPosition = new Vector3(0, 0.9f, ps.transform.localPosition.z);
        Debug.Log($"Particle system local position set to: {ps.transform.localPosition}");
    }

    private void AdjustParticleSystemPositionBack(ParticleSystem ps)
    {   //base particle function
        // Set the particle system's local position to y = 0.23
        ps.transform.localPosition = new Vector3(0.0f , 0.23f, ps.transform.localPosition.z);
        Debug.Log($"Particle system local position set to: {ps.transform.localPosition}");
    }

    private void HandleDamageTaken()
    {
        // Call RemoveUpgrade or equivalent functionality here
        RemoveUpgrade();
    }

   
    // Shotgun Powerups Below
    public void ActivateSpreadFire()
    {
        DeactivatePlasma();
        spreadFireActivated = true;
        Debug.Log("Spread fire activated");
        ShotgunSpeed();
    }

    void FireWithSpread(Transform firePoint)
    {
        float angleStep = spreadAngle / (bulletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float bulletDirX = Mathf.Cos((currentAngle + transform.eulerAngles.z) * Mathf.Deg2Rad);
            float bulletDirY = Mathf.Sin((currentAngle + transform.eulerAngles.z) * Mathf.Deg2Rad);

            Vector2 bulletDirection = new Vector2(bulletDirX, bulletDirY).normalized;

            // Instantiate the bullet
            GameObject bullet = Instantiate(shotgunBullet, firePoint.position, Quaternion.identity);

            // Set the bullet's rotation to match the direction it's moving
            float bulletAngle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, bulletAngle - 180));

            // Set the velocity of the bullet
            bullet.GetComponent<Rigidbody2D>().velocity = bulletDirection * bulletSpeed;

            // Destroy the bullet after its lifetime
            Destroy(bullet, bulletLifetime);
        }
    }

    public void ChangeSpriteShotgun(Sprite shotgunSprite)
    {
        if (spriteRenderer != null)
        {
            Debug.Log($"Changing sprite from {spriteRenderer.sprite.name} to {shotgunSprite.name}");
            spriteRenderer.sprite = shotgunSprite;
            Debug.Log("Sprite changed successfully");
            spriteRenderer.gameObject.transform.localScale = new Vector3(4f, 4f, 4f);//change these to make it scale to normal size

            // Enable the shotgun sprite collider and disable the others
            if (defaultCollider != null) defaultCollider.enabled = false;
            if (redShipSpriteCollider != null) redShipSpriteCollider.enabled = false;
            if (shotgunSpriteCollider != null) shotgunSpriteCollider.enabled = true;
            if (plasmaSpriteCollider != null) plasmaSpriteCollider.enabled = false;
            if (particleSystem != null)
            {
                //particle system scale would be 4,.05,.05
                  particleSystem.transform.localPosition = new Vector3(0, 0.2f, particleSystem.transform.localPosition.z);
                Debug.Log("Particle system position adjusted to fit new sprite");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on player.");
        }
    }

    public void ChangeSpritePlasma(Sprite plasmaSprite)
    {
        if (spriteRenderer != null)
        {
            Debug.Log($"Changing sprite from {spriteRenderer.sprite.name} to {plasmaSprite.name}");
            spriteRenderer.sprite = plasmaSprite;
            Debug.Log("Sprite changed successfully");
            spriteRenderer.gameObject.transform.localScale = new Vector3(.8f, .8f, .8f);//change these to make it scale to normal size

            // Enable the shotgun sprite collider and disable the others
            if (defaultCollider != null) defaultCollider.enabled = false;
            if (redShipSpriteCollider != null) redShipSpriteCollider.enabled = false;
            if (shotgunSpriteCollider != null) shotgunSpriteCollider.enabled = false;
            if (plasmaSpriteCollider != null) plasmaSpriteCollider.enabled = true;

            if (particleSystem != null)
            {
               particleSystem.transform.localPosition = new Vector3(-.14f, 1.4f, particleSystem.transform.localPosition.z);
                Debug.Log("Particle system position adjusted to fit new sprite");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on player.");
        }
    }

    public void ActivateShotgunFirePoints()
    {
        spreadFireActivated = true;
        Debug.Log("Shotgun fire points activated");
       DeactivatePlasma();
       ShotgunSpeed();
    }

    public void ActivatePlasmaFire()
    {
        RemoveUpgrade();
        plasmaFireActivated = true;
        PlasmaSpeed();
        Debug.Log("Plasma fire activated");

    }

   


  public IEnumerator ChargeAndFire()
    {
        isCharging = true;
        currentChargeTime = 0f;

        plasmaBulletInstance = Instantiate(plasmaBullet, FirePointPlasma.position, FirePointPlasma.rotation);
        plasmaBulletInstance.transform.parent = FirePointPlasma;
        while (isCharging && currentChargeTime < maxChargeTime)
        {
            if (plasmaBulletInstance == null)
    {
        AudioManager.Instance.StopSFX("Alien1 Charge");
        isCharging = false; // Reset isCharging if bullet is destroyed
        yield break; 
    
    }
            currentChargeTime += Time.deltaTime;
            float chargePercentage = Mathf.Clamp01(currentChargeTime / maxChargeTime);
            plasmaBulletInstance.transform.localScale = Vector3.Lerp(Vector3.one, maxBulletScale, chargePercentage);
            yield return null;
        }

        isCharging = true; // Maintain charging state until the mouse button is released
    
     
    }
     private void FireCurrentBullet(GameObject bulletInstance)
    {
        if (bulletInstance != null)
        {
            bulletInstance.transform.parent = null; // Detach the bullet from the fire point
            Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = FirePointPlasma.up * plasmaBulletSpeed;
            }
            plasmaBulletInstance = null; 
        }
    }
    public void OnPlasmaBulletHit()
    {
        if (isCharging)
        {
            // Stop the current charge and start a new one
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null; // Clear the reference 
            }

            // Fire the current bullet 
            if (plasmaBulletInstance != null)
            {
                FireCurrentBullet(plasmaBulletInstance);
                plasmaBulletInstance = null; // Clear reference 
            }

            // Reset charging state
            isCharging = false;
            currentChargeTime = 0f;

            
            chargeCoroutine = StartCoroutine(ChargeAndFire());
        }
    }
   public void StopCharging()
    {
        if (isCharging)
        {
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }

            FireCurrentBullet(plasmaBulletInstance);
            isCharging = false;
          
            nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
        }
    }

    public void DeactivatePlasma()
    {
        plasmaFireActivated = false;
        StopCharging();
        
    }

 
    private void RemoveUpgrade()
    {
        // Implement the logic to remove the upgrade
        DeactivateAdditionalFirePoints();
        RevertToOriginalSprite();
        DeactivatePlasma();
        BaseSpeed();
        spreadFireActivated = false;
        plasmaFireActivated = false;
    }

   
    public void ShotgunSpeed()
    {
        moveSpeed = 8f + speedbuffspeed;
        
       
    }
    public void BaseSpeed()
    {
        moveSpeed = 7f ;
    }
    public void PlasmaSpeed()
    {
       moveSpeed = 6.3f + speedbuffspeed;
    }
    public void RedshipSpeed()
    {
        moveSpeed = 7f +speedbuffspeed;
    }
}
