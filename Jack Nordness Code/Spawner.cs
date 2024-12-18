using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] asteroidPrefabs;
    [SerializeField] float roundDuration = 10f;
    [SerializeField] int initialAsteroidCount = 30;
    [SerializeField] int asteroidsPerRoundIncrement = 5;
    [SerializeField] float spawnInterval = 1.0f;
    [SerializeField] float respawnDelay = 5.0f;
    [SerializeField] float totalInitialSpawnDuration = 5.0f;
    [SerializeField] Transform playerTransform;
    [SerializeField] float minDistanceFromPlayer = 5.0f;
    [SerializeField] float spawnBuffer = 5.0f; 
    private Vector2 playAreaMin;
    private Vector2 playAreaMax;
    private int currentRound = 0;
    private int targetAsteroidCount;
    private float elapsedTime = 0f;
    private List<GameObject> asteroids = new List<GameObject>();
    public GameObject asteroid2Prefab;
    public GameObject asteroid1Prefab;
    private bool isSpawning = false;

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += HandleAsteroidDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= HandleAsteroidDestroyed;
    }

    private void Start()
    {
        CalculateScreenBounds();
        targetAsteroidCount = initialAsteroidCount;
        StartCoroutine(SpawnInitialAsteroids());
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= roundDuration)
        {
            elapsedTime = 0f;
            StartNewRound();
        }
    }

    private void CalculateScreenBounds()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            
            playAreaMin = new Vector2(bottomLeft.x - spawnBuffer, bottomLeft.y - spawnBuffer);
            playAreaMax = new Vector2(topRight.x + spawnBuffer, topRight.y + spawnBuffer);
        }
        else
        {
            Debug.LogError("Main camera not found.");
        }
    }

    private void StartNewRound()
    {
        currentRound++;
        targetAsteroidCount += asteroidsPerRoundIncrement;
        StartCoroutine(AdjustAsteroidCount());
    }

    private IEnumerator SpawnInitialAsteroids()
    {
        float spawnInterval = totalInitialSpawnDuration / initialAsteroidCount;

        for (int i = 0; i < initialAsteroidCount; i++)
        {
            SpawnAsteroid();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnAsteroidsRoutine()
    {
        while (true)
        {
            MaintainAsteroidCount();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void MaintainAsteroidCount()
    {
        while (asteroids.Count < targetAsteroidCount)
        {
            SpawnAsteroid();
        }
    }

    private IEnumerator AdjustAsteroidCount()
    {
        while (asteroids.Count < targetAsteroidCount)
        {
            SpawnAsteroid();
            yield return new WaitForSeconds(spawnInterval / targetAsteroidCount);
        }
    }

    private void SpawnAsteroid()
    {
        if (asteroidPrefabs == null)
        {
            Debug.LogError("Asteroid prefab is not assigned!");
            return;
        }

        Vector2 spawnPosition = GetValidSpawnPosition();

        int randomIndex = UnityEngine.Random.Range(0, asteroidPrefabs.Length);
        GameObject asteroidPrefab = asteroidPrefabs[randomIndex];

        GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);

        Enemy asteroidEnemy = newAsteroid.GetComponent<Enemy>();
        if (asteroidEnemy != null)
        {
            asteroidEnemy.maxHealth = UnityEngine.Random.Range(1f, 5.0f);
            asteroidEnemy.moveSpeed = UnityEngine.Random.Range(1.0f, 9.0f);
            asteroidEnemy.directionChangeInterval = UnityEngine.Random.Range(1.0f, 6.0f);
            asteroidEnemy.randomDirectionInfluence = UnityEngine.Random.Range(0.05f, 1.0f);
            asteroidEnemy.directionSmoothTime = UnityEngine.Random.Range(0.5f, 2.0f);
        }

        asteroids.Add(newAsteroid);
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        int safetyCounter = 0;

        do
        {
            spawnPosition = new Vector2(
                UnityEngine.Random.Range(playAreaMin.x, playAreaMax.x),
                UnityEngine.Random.Range(playAreaMin.y, playAreaMax.y)
            );
            safetyCounter++;
        } while (Vector2.Distance(spawnPosition, playerTransform.position) < minDistanceFromPlayer && safetyCounter < 100);

        return spawnPosition;
    }

    private void HandleAsteroidDestroyed(Enemy asteroid)
    {
        if (asteroid != null && !isSpawning)
        {
            asteroids.Remove(asteroid.gameObject);

            if (asteroid.gameObject.name == "asteroid4(Clone)")
            {
                isSpawning = true;
                Vector2 destroyedAsteroidPosition = asteroid.transform.position;
                Debug.Log("Destroyed asteroid position: " + destroyedAsteroidPosition);

                if (UnityEngine.Random.value < 0.5f)
                {
                    SpawnSpecificAsteroid(asteroid1Prefab, destroyedAsteroidPosition);
                    SpawnSpecificAsteroid(asteroid1Prefab, destroyedAsteroidPosition);
                }
                else
                {
                    SpawnSpecificAsteroid(asteroid2Prefab, destroyedAsteroidPosition);
                }
            }

            StartCoroutine(RespawnAsteroidWithDelay());
        }
    }

    private void SpawnSpecificAsteroid(GameObject asteroidPrefab, Vector2 position)
    {
        if (asteroidPrefab == null)
        {
            Debug.LogError("Asteroid prefab is null!");
            return;
        }

        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity);
        asteroids.Add(newAsteroid);
    }

    private IEnumerator RespawnAsteroidWithDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        isSpawning = false;
        if (asteroids.Count < targetAsteroidCount)
        {
            SpawnAsteroid();
        }
    }
}
