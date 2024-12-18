using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; 
        PlayMusicForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene(); // Play correct music for each scene 
    }

    public void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "MenuScene":
                PlayMusic("MainMenu");
                break;
            case "GameScene":
                PlayMusic("background music");
                break;
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    // Add the StopMusic method here
    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(string name, float volume = 1.0f)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip, volume);
        }
    }

    public void StopSFX(string name)
{
    Sound s = Array.Find(sfxSounds, sound => sound.name == name);
    sfxSource.Stop(); // Stops the current sound playing on this AudioSource
}

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
{
    sfxSource.mute = !sfxSource.mute;
}
public void MusicVolume(float volume)
{
musicSource.volume = volume;
}

public void SFXVolume(float volume)
{
    sfxSource.volume = volume;
}
}
