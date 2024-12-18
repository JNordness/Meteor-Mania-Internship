using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class UnityServicesInitializer : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Unity Services initialized and player authenticated.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
        }
    }
}
