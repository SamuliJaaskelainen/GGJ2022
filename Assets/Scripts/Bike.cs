using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Bike : MonoBehaviour
{
    public AudioSource driveAudio;
    public AudioSource boostAudio;
    public AudioSource driftAudio;
    public AudioClip[] driveClips;
    public float[] maxTorque;
    public float boostTorque;
    public float boostTime;
    public Transform downForce;
    public Transform cameraTarget;
    public CinemachineVirtualCamera bikeCamera;
    public GameObject dimension1;
    public GameObject dimension2;
    public WireframeRenderer wireRenderer;
    Wheel[] wheels;
    float throttle;
    float turn;
    bool drift;
    Rigidbody rb;
    int warps;
    float boostTimer;
    bool isBoosting;

    void Awake()
    {
        wheels = GetComponentsInChildren<Wheel>();
        rb = GetComponent<Rigidbody>();
        dimension1.SetActive(true);
        dimension2.SetActive(false);

        AudioManager.Instance.SetMusicLayer(2, false);
        AudioManager.Instance.SetMusicLayer(3, false);
        AudioManager.Instance.SetMusicLayer(4, false);
        AudioManager.Instance.SetMusicLayer(5, false);
    }

    void Update()
    {
        throttle = Input.GetAxis("Throttle");
        turn = Input.GetAxis("Turn");
        drift = Input.GetButton("Drift");
        isBoosting = Time.time < boostTimer;

        if (drift) throttle = 0.0f;
        cameraTarget.localPosition = new Vector3(turn * (drift ? 2.5f : 2.0f), cameraTarget.localPosition.y, cameraTarget.localPosition.z);

        float fovTarget = isBoosting ? 80.0f : (dimension2.activeSelf ? 70.0f : 60f);
        if (bikeCamera.m_Lens.FieldOfView > fovTarget)
        {
            bikeCamera.m_Lens.FieldOfView = Mathf.Lerp(bikeCamera.m_Lens.FieldOfView, fovTarget, Time.deltaTime * 3.0f);
        }
        else
        {
            bikeCamera.m_Lens.FieldOfView = Mathf.Lerp(bikeCamera.m_Lens.FieldOfView, fovTarget, Time.deltaTime * 20.0f);
        }

        wireRenderer.randomOffset -= 0.12f * Time.deltaTime;
        wireRenderer.randomOffset = Mathf.Clamp01(wireRenderer.randomOffset);

        if (isBoosting)
        {
            if (!boostAudio.isPlaying)
            {
                boostAudio.Play();
                driftAudio.Pause();
                driveAudio.Pause();
            }
        }
        else if (drift)
        {
            if (!driftAudio.isPlaying)
            {
                boostAudio.Pause();
                driftAudio.Play();
                driveAudio.Pause();
            }
        }
        else
        {
            if (!driveAudio.isPlaying)
            {
                boostAudio.Pause();
                driftAudio.Pause();
                driveAudio.Play();
            }
        }
    }

    void FixedUpdate()
    {
        foreach (Wheel w in wheels)
        {
            w.Steer(turn);
            if (isBoosting)
            {
                w.Accelerate(boostTorque);
            }
            else if (dimension2.activeSelf)
            {
                w.Accelerate(throttle * maxTorque[warps]);
            }
            else
            {
                w.Accelerate(throttle * maxTorque[warps]);
            }
            w.UpdatePosition();
            if (drift)
            {
                w.Drift();
            }
        }

        float downVel = Mathf.Clamp(rb.velocity.sqrMagnitude * maxTorque[0] * 0.005f, 0.0f, 100000.0f);
        rb.AddForceAtPosition(Vector3.down * downVel, downForce.position, ForceMode.Force);

        if (dimension2.activeSelf && rb.velocity.sqrMagnitude < 1000.0f)
        {
            ShiftDimension();
            AudioManager.Instance.SetMusicLayer(2, false);
            AudioManager.Instance.SetMusicLayer(3, false);
            AudioManager.Instance.SetMusicLayer(4, false);
            AudioManager.Instance.SetMusicLayer(5, false);
            warps = 0;
        }

        if (rb.velocity.sqrMagnitude < 800.0f)
        {
            driveAudio.clip = driveClips[0];
        }
        else if (rb.velocity.sqrMagnitude < 4000.0f)
        {
            driveAudio.clip = driveClips[1];
        }
        else
        {
            driveAudio.clip = driveClips[2];
        }
    }

    public void Warp()
    {
        AudioManager.Instance.PlaySound("WarpEnter", transform.position);
        boostTimer = Time.time + boostTime;
        warps++;
        warps = Mathf.Clamp(warps, 0, 3);
        wireRenderer.randomOffset = 0.025f * warps;
        if (warps >= 3)
        {
            ShiftDimension();
        }

        if (warps == 1)
        {
            AudioManager.Instance.SetMusicLayer(2, true);
        }
        else if (warps == 2)
        {
            AudioManager.Instance.SetMusicLayer(2, false);
            AudioManager.Instance.SetMusicLayer(3, true);
        }
        else
        {
            AudioManager.Instance.SetMusicLayer(3, false);
            AudioManager.Instance.SetMusicLayer(4, true);
            AudioManager.Instance.SetMusicLayer(5, true);
        }
    }

    void ShiftDimension()
    {
        dimension1.SetActive(!dimension1.activeSelf);
        dimension2.SetActive(!dimension2.activeSelf);
        wireRenderer.randomOffset = 0.1f;

        if (dimension1.activeSelf)
        {
            AudioManager.Instance.PlaySound("DimensionShiftTo1", transform.position);
        }
        else
        {
            AudioManager.Instance.PlaySound("DimensionShiftTo2", transform.position);
        }
    }
}