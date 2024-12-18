using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;   // Speed of the bullet
    public float lifeTime = 5f; // Lifetime of the bullet
    private int damage = 1;     // Default damage 

    private Rigidbody2D rb;    
    private ArcadePlayerMovement playerMovement; // Reference to the player 
    public GameObject hitExplosion;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.up * speed; // Ensure the bullet goes forward
        Destroy(gameObject, lifeTime); // Destroy the bullet after lifetime to not slowdown game.

      
        GameObject player = GameObject.FindWithTag("Ship");  
        if (player != null)
        {
            playerMovement = player.GetComponent<ArcadePlayerMovement>();
        }
        else
        {
            Debug.LogError("Player object not found! Make sure the player has the correct tag.");
        }
    }

    // Method to detect when plasmaFireActivated is true and calculate damage
    public void DetectPlasmaFireStatus()
    {
        if (playerMovement != null && playerMovement.plasmaFireActivated)
        {
            //determine how long player is holding down shoot
            float chargeTime = playerMovement.currentChargeTime;

            //use chargetime to calc damage
            if (chargeTime >= 0f && chargeTime <= 0.74f)
            {
                damage = 1;  
            }
            else if (chargeTime >= 0.75f && chargeTime <= 1.49f)
            {
                damage = 2;  
            }
            else if (chargeTime >= 1.5f)
            {
                damage = 3; 
            }

            Debug.Log("Plasma fire is activated, enhancing bullet behavior! Damage: " + damage);
        }
        else
        {
            Debug.Log("Plasma fire is inactive.");
            damage = 1; //normal bullet damage if plasma pu is not on
        }
    }

     private void OnCollisionEnter2D(Collision2D collision)
    {
        int asteroidLayer = LayerMask.NameToLayer("asteroid");
        int kazeLayer = LayerMask.NameToLayer("Kaze");
        int enemyShipLayer = LayerMask.NameToLayer("EnemyShip");
        int AlienLayer = LayerMask.NameToLayer("Alien");
        int Alien2Layer = LayerMask.NameToLayer("Alien2");
        int StompLayer = LayerMask.NameToLayer("Stomp");
       

        // Check if the collision is with any of the enemy layers
        if (collision.gameObject.layer == asteroidLayer || 
            collision.gameObject.layer == kazeLayer || 
            collision.gameObject.layer == enemyShipLayer ||
            collision.gameObject.layer == AlienLayer ||
            collision.gameObject.layer == Alien2Layer ||
            collision.gameObject.layer == StompLayer)
        {
            // Detect plasma fire status and calculate damage 
            DetectPlasmaFireStatus();

            
            Enemy asteroid = collision.gameObject.GetComponent<Enemy>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(damage);
                Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;  
            }

            TeleportingEnemy kaze = collision.gameObject.GetComponent<TeleportingEnemy>();
            if (kaze != null)
            {
                kaze.TakeDamage(damage);
                 Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;  
            }

            EnemyShip enemyShip = collision.gameObject.GetComponent<EnemyShip>();
            if (enemyShip != null)
            {
                enemyShip.TakeDamage(damage);
                 Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;  
            }

            Alien1 alien = collision.gameObject.GetComponent<Alien1>();
            if (alien != null)
            {
                alien.TakeDamage(damage);
                 Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;
            }

            EnemyShotgun alien2 = collision.gameObject.GetComponent<EnemyShotgun>();
            if (alien2 != null)  // Fix reference to Alien2 component
            {
                alien2.TakeDamage(damage);
                 Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;
            }
            AreaStomp Stomp = collision.gameObject.GetComponent<AreaStomp>();
            if (Stomp != null)
            {
                Stomp.TakeDamage(damage);
                 Instantiate(hitExplosion, transform.position, Quaternion.identity);
                return;
            }
        }
    }
}