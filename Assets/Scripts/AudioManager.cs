using UnityEngine;
using System;

// Credit to Brackeys YouTube tutorial on Audio managers

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(.1f, 3f)] public float pitch = 1f;
    public bool loop;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        //Play("Theme");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        GameObject tempGO = new GameObject("TempAudio_" + name);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();

        tempSource.clip = s.clip;
        tempSource.volume = s.volume;
        tempSource.pitch = s.pitch;
        tempSource.loop = false; // No looping for temp clips
        tempSource.Play();

        Destroy(tempGO, s.clip.length / s.pitch); // Account for pitch affecting clip duration
    }


    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
    }
}
