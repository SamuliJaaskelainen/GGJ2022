using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool powered;
    public float maxAngle;
    public float offset;

    float turnAngle;
    WheelCollider wheelCollider;
    Transform visual;

    private void Start()
    {
        wheelCollider = GetComponentInChildren<WheelCollider>();
        visual = GetComponentInChildren<MeshFilter>().transform;
    }

    public void Steer(float steerInput)
    {
        turnAngle = steerInput * maxAngle + offset;
        wheelCollider.steerAngle = turnAngle;
    }

    public void Accelerate(float torque)
    {
        if (powered)
        {
            wheelCollider.motorTorque = torque;
        }
    }

    public void UpdatePosition()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        wheelCollider.GetWorldPose(out pos, out rot);
        visual.transform.position = pos;
        visual.transform.rotation = rot;
    }
}
