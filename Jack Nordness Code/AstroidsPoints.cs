using UnityEngine;

public class AsteroidPoints : MonoBehaviour
{
    [SerializeField] int points = 10; // Default points awarded for destroying this 

    public void AwardPoints()
    {
        Scoreboard.Instance.IncrementScore(points);
    }
}