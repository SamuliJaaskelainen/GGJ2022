using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bike : MonoBehaviour
{
    public float maxTorque;
    Wheel[] wheels;
    float throttle;
    float turn;

    void Update()
    {
        throttle = Input.GetAxis("Throttle");
        turn = Input.GetAxis("Turn");
        wheels = GetComponentsInChildren<Wheel>();
    }

    void FixedUpdate()
    {
        foreach (Wheel w in wheels)
        {
            w.Steer(turn);
            w.Accelerate(throttle * maxTorque);
            w.UpdatePosition();
        }
    }
}