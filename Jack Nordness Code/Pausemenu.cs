using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausemenu : MonoBehaviour
{
    public GameObject PausePanel;
    public GameObject SettingsPanel;

    private bool isSettingsPanelOpen = false; // Track settings panel 
    
    void Update()
    {
       
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        SettingsPanel.SetActive(false); // Deactivate the settings 
        isSettingsPanelOpen = false; // Reset panel state
        Time.timeScale = 1;
    }

    public void ToggleSettings()
    {
        isSettingsPanelOpen = !isSettingsPanelOpen; 

        if (isSettingsPanelOpen)
        {
            SettingsPanel.SetActive(true); // Show 
        }
        else
        {
            SettingsPanel.SetActive(false); // Hide 
        }
    }

    public void StopGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }
}
