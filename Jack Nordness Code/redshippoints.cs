using UnityEngine;

public class redshippoints : MonoBehaviour
{
    [SerializeField] int points = 150; // Default points awarded for destroying this 

    
    public void AwardPoints()
    {
        Scoreboard.Instance.IncrementScore(points);
    }
}