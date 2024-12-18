using UnityEngine;

public class Alien1points : MonoBehaviour
{
    [SerializeField] int points = 500; // Default points awarded for destroying 

  
    public void AwardPoints()
    {
        Scoreboard.Instance.IncrementScore(points);
    }
}