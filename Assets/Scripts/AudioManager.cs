using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] soundEffects;
    public AudioSource[] musicLayers;

    public GameObject audioPrefab;
    
    public Song song_1;
    public Song song_2;
    public Song activeSong;

    List<GameObject> audioPoolObject = new List<GameObject>();

    public static AudioManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < 100; ++i)
        {
            GameObject audioObject = Instantiate(audioPrefab, transform) as GameObject;
            audioPoolObject.Add(audioObject);
        }

    }

    public void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        for (int i = 0; i < soundEffects.Length; ++i)
        {
            if (soundEffects[i].name == audioClip.name)
            {
                PlaySound(i, position, volume, pitch);
                break;
            }
        }
    }

    public void PlaySound(AudioClip[] audioClips, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
        if (audioClip != null)
        {
            PlaySound(audioClip, position, volume, pitch);
        }
    }

    public void PlaySound(string clipName, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        for (int i = 0; i < soundEffects.Length; ++i)
        {
            if (soundEffects[i].name == clipName)
            {
                PlaySound(i, position, volume, pitch);
                break;
            }
        }
    }

    public void PlaySound(int clipIndex, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        if (clipIndex >= soundEffects.Length)
        {
            Debug.LogError("No sound at index: " + clipIndex);
            return;
        }

        GameObject audioObject = audioPoolObject[0] as GameObject;
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioPoolObject.RemoveAt(0);
            audioSource.transform.position = position;
            audioSource.clip = soundEffects[clipIndex];
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
            StartCoroutine("ReturnToPool", audioObject);
        }
        else
        {
            Debug.LogError("NULL audio source at index: " + clipIndex);
        }
    }

    IEnumerator ReturnToPool(GameObject audioObject)
    {
        yield return new WaitForSeconds(audioObject.GetComponent<AudioSource>().clip.length);
        audioPoolObject.Add(audioObject);
    }

    public void LoadSong(int index)
    {
        // Load a song

        if (index == 1)
        {
            activeSong = song_1;
        }
        else if (index == 2)
        {
            activeSong = song_2;
        }
        else
        {
            activeSong = song_1;
        }

        activeSong.base_melody_loop.Play();
        activeSong.base_perc_loop.Play();
        activeSong.energy_one.Play();
        activeSong.energy_two.Play();
        activeSong.energy_three.Play();
        activeSong.warp_main.Play();
        activeSong.warp_melody.Play();

    }
        
    public void SongNoWarp()
    {
        // Play layers of song for energy level 1
        //
        activeSong.base_melody_loop.mute = false;
        activeSong.base_perc_loop.mute = false;
        activeSong.energy_one.mute = false;

        activeSong.energy_two.mute = true;
        activeSong.energy_three.mute = true;
        activeSong.warp_main.mute = true;
        activeSong.warp_melody.mute = true;
    }

    public void EnergyIncrease(int level)
    {
        // Add stem for increased energy level on boost
        if (level == 0)
        {
            activeSong.energy_one.mute = false;
            activeSong.energy_two.mute = true;
            activeSong.energy_three.mute = true;
        }
        else if (level == 1)
        {
            activeSong.energy_one.mute = true;
            activeSong.energy_two.mute = false;
            activeSong.energy_three.mute = true;
        }
        else
        {
            activeSong.energy_one.mute = true;
            activeSong.energy_two.mute = true;
            activeSong.energy_three.mute = false;
        }
    }

    public void SongWarp ()
    {
        // Play layers of song for warping

        activeSong.base_melody_loop.mute = false;
        activeSong.base_perc_loop.mute = false;
        activeSong.energy_one.mute = true;

        activeSong.energy_two.mute = true;
        activeSong.energy_three.mute = true;
        activeSong.warp_main.mute = false;
        activeSong.warp_melody.mute = false;
    }

    public void SetMusicLayer(int index, bool on)
    {
        musicLayers[index].mute = !on;
    }
}
