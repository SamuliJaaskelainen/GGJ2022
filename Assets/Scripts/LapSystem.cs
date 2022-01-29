using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LapSystem : MonoBehaviour
{
    public SimpleHelvetica timeText;
    public GameObject timeCamera;
    public AutomaticWireframeObjects timeWire;
    public Bike bike;
    public Checkpoint[] checkpoints;
    public int lapCount = 2;
    public int currentLap = 1;
    public int currentCheckpoint = 0;
    public double time;

    public void HitCheckpoint(int id)
    {
        if (currentCheckpoint == id)
            return;

        if (currentCheckpoint == 0 && id == 1)
        {
            currentCheckpoint = id;
            AudioManager.Instance.PlaySound("Start", transform.position);
            Debug.Log("Race begins!");
        }
        else if (currentCheckpoint == (id - 1))
        {
            currentCheckpoint = id;
            Debug.Log("Checkpoint: " + id);
        }
        else if (currentCheckpoint == checkpoints.Length && id == 1)
        {
            currentCheckpoint = id;
            ++currentLap;
            Debug.Log("LAP: " + currentLap);
            Debug.Log("Time: " + time);

            if (currentLap > lapCount)
            {
                currentCheckpoint = -1;
                TimeSpan timeSpan = TimeSpan.FromSeconds(time);
                timeText.Text = timeSpan.ToString("mm':'ss':'ms");
                timeText.GenerateText();
                timeWire.enabled = true;
                timeCamera.SetActive(true);
                bike.enabled = false;
                AudioManager.Instance.PlaySound("Complete", transform.position);
                Debug.Log("Game over");
            }
            else if (currentLap == 2)
            {
                AudioManager.Instance.PlaySound("Lap2", transform.position);
            }
            else if (currentLap == 3)
            {
                AudioManager.Instance.PlaySound("LapFinal", transform.position);
            }
        }
    }

    void Update()
    {
        if (currentCheckpoint > 0)
        {
            time += Time.deltaTime;
        }

        if (Input.GetButton("Restart"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }
    }
}
