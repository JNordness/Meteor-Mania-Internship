using UnityEngine;

public class spin : MonoBehaviour
{
    public float rotationSpeed = 10f; 

    void Update()
    {
        
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
