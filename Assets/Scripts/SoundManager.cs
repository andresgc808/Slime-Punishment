using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public Sound[] sounds;
    private static Dictionary<string, float> soundTimerDictionary;

    public static SoundManager instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Another instance of SoundManager already exists. Destroying this instance.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("SoundManager instance set.");
        }

        soundTimerDictionary = new Dictionary<string, float>();

        // Load all audio clips from the SFX folder
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("SFX");
        if (audioClips.Length == 0)
        {
            Debug.LogError("No audio clips found in SFX folder!");
        }
        else
        {
            Debug.Log($"{audioClips.Length} audio clips loaded from SFX folder.");
        }

        sounds = new Sound[audioClips.Length];

        for (int i = 0; i < audioClips.Length; i++)
        {
            AudioClip clip = audioClips[i];
            sounds[i] = new Sound
            {
                name = clip.name,
                clip = clip,
                volume = 1f,
                pitch = 1f,
                isLoop = false,
                hasCooldown = false
            };
            Debug.Log($"Sound '{clip.name}' initialized.");
        }

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.isLoop;
            Debug.Log($"AudioSource for sound '{sound.name}' initialized.");

            if (sound.hasCooldown)
            {
                soundTimerDictionary[sound.name] = 0f;
            }
        }
    }

    private void Start()
    {
        // for possible theme music if we end up making it
        // Play('Theme');
    }

    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);

        if (sound == null)
        {
            Debug.LogError("Sound " + name + " Not Found!");
            return;
        }

        if (!CanPlaySound(sound)) return;

        sound.source.Play();
        Debug.Log($"Playing sound '{name}'.");
    }

    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);

        if (sound == null)
        {
            Debug.LogError("Sound " + name + " Not Found!");
            return;
        }

        sound.source.Stop();
        Debug.Log($"Stopped sound '{name}'.");
    }

    private static bool CanPlaySound(Sound sound)
    {
        if (soundTimerDictionary.ContainsKey(sound.name))
        {
            float lastTimePlayed = soundTimerDictionary[sound.name];

            if (lastTimePlayed + sound.clip.length < Time.time)
            {
                soundTimerDictionary[sound.name] = Time.time;
                return true;
            }

            return false;
        }

        return true;
    }
}
