using UnityEngine;

public class Misslepoints : MonoBehaviour
{
    [SerializeField] int points = 750; // Default points awarded for destroying this 

    
    public void AwardPoints()
    {
        Scoreboard.Instance.IncrementScore(points);
    }
}