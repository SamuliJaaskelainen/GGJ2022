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
                Debug.Log("Game over");
            }
        }
    }

    void Update()
    {
        if (currentCheckpoint > 0)
        {
            time += Time.deltaTime;
        }
    }
}
