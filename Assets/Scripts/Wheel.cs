using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool powered;
    public float maxAngle;
    public float offset;
    public float driftStiffness;
    public bool driftBrake;
    [Range(0.0f, 1.0f)] public float powerMultiplier;

    float turnAngle;
    WheelCollider wheelCollider;
    Transform visual;
    float normalStiffness;

    private void Start()
    {
        wheelCollider = GetComponentInChildren<WheelCollider>();
        visual = GetComponentInChildren<MeshFilter>().transform;
        normalStiffness = wheelCollider.sidewaysFriction.stiffness;
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
            wheelCollider.motorTorque = Mathf.Max(torque * powerMultiplier, 1.0f);
            wheelCollider.brakeTorque = 0.0f;
            WheelFrictionCurve curve = wheelCollider.sidewaysFriction;
            curve.stiffness = normalStiffness;
            wheelCollider.sidewaysFriction = curve;

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

    public void Drift()
    {
        WheelFrictionCurve curve = wheelCollider.sidewaysFriction;
        curve.stiffness = driftStiffness;
        wheelCollider.sidewaysFriction = curve;

        if (driftBrake)
        {
            wheelCollider.brakeTorque = wheelCollider.motorTorque;
        }
    }
}
