using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure there is only one AuthenticationManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep alive 
        }
        else
        {
            Destroy(gameObject);  // Destroy any dups if needed 
        }
    }

    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();

            // Sign in anonymously
            await SignInAnonymously();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services or authenticate: " + e.Message);
        }
    }

    private async Task SignInAnonymously()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Player signed in anonymously with ID: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to sign in anonymously: " + e.Message);
        }
    }
}
