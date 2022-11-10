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
    public Song song_3;
    public Song song_4;
    public Song song_5;
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

        activeSong.base_loop.Play();
        activeSong.level_one.Play();
        activeSong.level_two.Play();
        activeSong.level_three.Play();
        activeSong.warp_1.Play();
        activeSong.warp_2.Play();

    }

    public void EnergyChange(int level)
    {
        {
            if (level == 0)
            {
                activeSong.base_loop.mute = false;
                activeSong.level_one.mute = true;
                activeSong.level_two.mute = true;
                activeSong.level_three.mute = true;
                activeSong.warp_1.mute = true;
                activeSong.warp_2.mute = true;
            }
            else if (level == 1)
            {
                activeSong.base_loop.mute = true;
                activeSong.level_one.mute = false;
                activeSong.level_two.mute = true;
                activeSong.level_three.mute = true;
                activeSong.warp_1.mute = true;
                activeSong.warp_2.mute = true;
            }
            else if (level == 2)
            {
                activeSong.base_loop.mute = true;
                activeSong.level_one.mute = true;
                activeSong.level_two.mute = false;
                activeSong.level_three.mute = true;
                activeSong.warp_1.mute = true;
                activeSong.warp_2.mute = true;
            }
            else if (level == 3)
            {
                activeSong.base_loop.mute = true;
                activeSong.level_one.mute = true;
                activeSong.level_two.mute = true;
                activeSong.level_three.mute = false;
                activeSong.warp_1.mute = true;
                activeSong.warp_2.mute = true;
            }
            else if (level == 4)
            {
                activeSong.base_loop.mute = true;
                activeSong.level_one.mute = true;
                activeSong.level_two.mute = true;
                activeSong.level_three.mute = true;
                activeSong.warp_1.mute = false;
                activeSong.warp_2.mute = true;
            }
            else
            {
                activeSong.base_loop.mute = true;
                activeSong.level_one.mute = true;
                activeSong.level_two.mute = true;
                activeSong.level_three.mute = true;
                activeSong.warp_1.mute = true;
                activeSong.warp_2.mute = false;
            }
        }
    }
    

    public void SetMusicLayer(int index, bool on)
    {
        musicLayers[index].mute = !on;
    }
}
