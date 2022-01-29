using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bike : MonoBehaviour
{
    public float maxTorque;
    public Transform downForce;
    Wheel[] wheels;
    float throttle;
    float turn;
    Rigidbody rb;

    void Awake()
    {
        wheels = GetComponentsInChildren<Wheel>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        throttle = Input.GetAxis("Throttle");
        turn = Input.GetAxis("Turn");

    }

    void FixedUpdate()
    {
        foreach (Wheel w in wheels)
        {
            w.Steer(turn);
            w.Accelerate(throttle * maxTorque);
            w.UpdatePosition();
        }

        float downVel = Mathf.Clamp(rb.velocity.sqrMagnitude * maxTorque * 0.005f, 0.0f, 100000.0f);
        rb.AddForceAtPosition(Vector3.down * downVel, downForce.position, ForceMode.Force);
        Debug.Log(downVel);
    }
}