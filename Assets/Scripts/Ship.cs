using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class Ship : MonoBehaviour {
    [Header("Multiplier for down force")]
    public float downForce = 1f;

    [Header("Thrust force while accelerating")]
    public float accelerationForce = 2000f;

    [Header("Thrust force while reversing")]
    public float reverseForce = 200f;

    [Header("Drag brake position on X axis")]
    public float brakeLeverage = 0.1f;

    [Header("Drag brake force (left / right brake)")]
    public float brakeDrag = 0.3f;

    [Header("Brake force (handbrake)")]
    public float brakeForce = 8000f;

    [Header("Multiplier for turning while mid-air")]
    public float airTurnModifier = 0.2f;

    [Header("Hover force strength relative to gravity")]
    public float upForce = 3.0f;

    [Header("Air resistance on Z axis")]
    public float forwardDrag = 0.015f;

    [Header("Air resistance on X axis")]
    public float sideDrag = 0.2f;

    [Header("Air resistance modifier while not close to ground")]
    public float airDragModifier = 0.2f;

    [Header("Turn power multiplier")]
    public float turnPower = 3.0f;

    [Header("Rate of steering adjustment")]
    public float bankRate = 120.0f;

    [Header("Max steering value")]
    public float maxBankAngle = 30.0f;

    [Header("Hover height")]
    public float targetHeight = 1.0f;

    [Header("Scale of editor debug rays")]
    public float debugRayScale = 0.01f;

    public float yawDelay = 0.2f;

    public float yawSpeed = 180.0f;

    public float yawHalfSpeed = 50.0f;

    public Transform cameraTarget;
    public Transform body;
    float throttle;
    float turn;
    private float bank;
    bool drfit;
    private bool accelerate;
    private bool brake;
    private bool reverse;
    private float leftBrake;
    private float rightBrake;
    private float yawRate;
    Rigidbody rb;
    private int excludeShipLayerMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        excludeShipLayerMask = ~(1 << gameObject.layer);
    }

    void Update()
    {
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        throttle = Input.GetAxis("Throttle");
        turn = gamePadState.ThumbSticks.Left.X;
        drfit = Input.GetButton("Drift");
        accelerate = gamePadState.Buttons.A == ButtonState.Pressed;
        brake = gamePadState.Buttons.X == ButtonState.Pressed;
        reverse = gamePadState.Buttons.B == ButtonState.Pressed;


        leftBrake = gamePadState.Triggers.Left;
        rightBrake = gamePadState.Triggers.Right;

        Debug.LogFormat("{0} {1}", leftBrake, rightBrake);

        if (drfit) throttle = 0.0f;

        // transform.RotateAround(transform.position, Vector3.forward, Time.deltaTime * -turn * maxBankAngle);

        cameraTarget.localPosition = new Vector3(turn * (drfit ? 2.5f : 2.0f), cameraTarget.localPosition.y, cameraTarget.localPosition.z);
        // cameraTarget.forward = GetHeading();

    }

    Vector3 GetHeading() {
        Vector3 dir = transform.forward + rb.velocity.normalized;
        dir.y = 0;
        if (dir.sqrMagnitude < Mathf.Epsilon) {
            return transform.forward;
        }
        return dir.normalized;
    }

    void FixedUpdate() {
        Vector3 heading = transform.forward;

        bank = Mathf.MoveTowardsAngle(bank, maxBankAngle * turn, Time.fixedDeltaTime * bankRate);
        // body.LookAt(body.transform.position + heading, Quaternion.AngleAxis(-bank, heading) * Vector3.up);
        Debug.DrawRay(rb.worldCenterOfMass, heading, Color.white);
        // AddRelativeForce(2000 * throttle * heading, Color.green);
        bool appliedUpForce;
        Vector3 groundNormal;
        bool groundContact = ApplyUpForce(out appliedUpForce, out groundNormal);
        if (!appliedUpForce) {
            float downVel = Mathf.Clamp(rb.velocity.sqrMagnitude * downForce, 0.0f, 100000.0f);
            AddForce(-downVel * groundNormal, Color.black);
        }
        Debug.DrawRay(rb.worldCenterOfMass, rb.velocity, Color.yellow);
        float turnRate = turnPower * (groundContact ? 1.0f : airTurnModifier);
        AddForceAtPosition(turnRate * rb.velocity.magnitude * bank * (Quaternion.AngleAxis(90.0f, Vector3.up) * heading), rb.worldCenterOfMass + heading, Color.cyan);

        if (groundContact) {
            if (accelerate) {
                AddRelativeForce(accelerationForce * Vector3.forward, Color.green);
            }
            if (reverse) {
                AddRelativeForce(-reverseForce * Vector3.forward, Color.green);
            }
            if (brake && Vector3.Dot(transform.forward, rb.velocity) > 0.0f) {
                AddRelativeForce(-brakeForce * Vector3.forward, Color.green);
            }
        }

        ApplyDrag(groundContact);
        ApplyBrake(leftBrake, -brakeLeverage);
        ApplyBrake(rightBrake, brakeLeverage);
        ApplyYaw(groundContact);
        // float downVel = Mathf.Clamp(rb.velocity.sqrMagnitude * maxTorque * 0.005f, 0.0f, 100000.0f);
        // rb.AddForceAtPosition(Vector3.down * downVel, downForce.position, ForceMode.Force);
        //Debug.Log(downVel);
    }

    private void ApplyYaw(bool groundContact) {
        if (!groundContact) {
            return;
        }
        Debug.LogFormat("Speed {0}", rb.velocity.magnitude);

        yawRate = Mathf.MoveTowards(yawRate, turn, Time.fixedDeltaTime / yawDelay);
        float yaw;
        if (yawRate < 0.0f) {
            yaw = Mathf.SmoothStep(0.0f, -1.0f, -yawRate);
        } else {
            yaw = Mathf.SmoothStep(0.0f, 1.0f, yawRate);
        }
        float speedLoss = 1.0f + rb.velocity.magnitude * rb.velocity.magnitude / Mathf.Pow(yawHalfSpeed, 2.0f);
        rb.MoveRotation(Quaternion.AngleAxis(yaw * Time.fixedDeltaTime * yawSpeed / speedLoss, transform.up) * rb.rotation);
    }

    private void ApplyDrag(bool groundContact) {
        float mod = groundContact ? 1.0f : airDragModifier;
        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        float rightSpeed = Vector3.Dot(transform.right, rb.velocity);
        AddForce(-mod * forwardDrag * forwardSpeed * Mathf.Abs(forwardSpeed) * transform.forward, Color.magenta);
        AddForce(-mod * sideDrag * rightSpeed * Mathf.Abs(rightSpeed) * transform.right, Color.magenta);
    }

    private void ApplyBrake(float axis, float leverage) {
        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        AddForceAtPosition(-axis * brakeDrag * forwardSpeed * Mathf.Abs(forwardSpeed) * transform.forward, rb.worldCenterOfMass + leverage * transform.right, Color.magenta);
    }

    private bool ApplyUpForce(out bool appliedForce, out Vector3 groundNormal) {
        appliedForce = false;
        groundNormal = Vector3.up;
        RaycastHit hitInfo;
        if (!Physics.Raycast(transform.position, -transform.up, out hitInfo, targetHeight + 1.0f, excludeShipLayerMask)) {
            return false;
        }
        groundNormal = hitInfo.normal;
        float distToGo = targetHeight - hitInfo.distance;
        if (distToGo < 0.0f) {
            // AddForce(-(1.0f + distToGo) * Physics.gravity, Color.red);
            return true;
        }

        float velocity = Vector3.Dot(transform.up, rb.velocity);
        float time = distToGo / velocity;
        float gravityComponent = time * Vector3.Dot(transform.up, Physics.gravity);

        bool overshoot = time > 0.0f && velocity + gravityComponent > 0.0f;
        // Debug.Log(velocity + gravityComponent );
        if (!overshoot) {
            AddForce(upForce * Physics.gravity.magnitude * rb.mass * transform.up, Color.red);
            appliedForce = true;
        } else {
        }
        return true;
    }

    private void AddRelativeForce(Vector3 force, Color color) {
        AddForce(transform.TransformDirection(force), color);
    }
    private void AddForce(Vector3 force, Color color) {
        AddForceAtPosition(force, rb.worldCenterOfMass, color);
    }
    private void AddForceAtPosition(Vector3 force, Vector3 position, Color color) {
        Debug.DrawRay(position, debugRayScale * force, color);
        rb.AddForceAtPosition(force, position, ForceMode.Force);
    }

}