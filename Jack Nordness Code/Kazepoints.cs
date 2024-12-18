using UnityEngine;

public class Kazepoints : MonoBehaviour
{
    [SerializeField] int points = 50; // Default points awarded for destroying this

    
    public void AwardPoints()
    {
        Scoreboard.Instance.IncrementScore(points);
    }
}