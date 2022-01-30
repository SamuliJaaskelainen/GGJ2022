using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using XInputDotNetPure;
using Cinemachine;

public class Ship : MonoBehaviour
{
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

    [Header("Air resistance on Z axis")] // 0.06
    public float[] forwardDrags;
    float forwardDrag;

    [Header("Air resistance on Z axis")]
    public float boostDrag;

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

    public float visualRotationRate = 15.0f;

    public float normalCameraOffset = 15.0f;

    public float driftCameraOffset = 15.0f;


    public AudioSource driveAudio;
    public AudioSource boostAudio;
    public AudioSource driftAudio;
    public AudioClip[] driveClips;
    public float boostTime;

    public CinemachineVirtualCamera bikeCamera;
    public GameObject dimension1;
    public GameObject dimension2;
    public WireframeRenderer wireRenderer;
    public float dimensionMinVelocity = 3000.0f;

    int warps;
    float boostTimer;
    bool isBoosting;
    bool isDrifting;
    public Transform cameraTarget;
    public Transform body;
    public Transform idleBody;
    public Transform accBody;
    float turn;
    private float bank;
    private bool accelerate;
    private bool brake;
    private bool reverse;
    private float leftBrake;
    private float rightBrake;
    private float yawRate;
    Rigidbody rb;
    private int excludeShipLayerMask;
    private Vector3 groundNormal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        excludeShipLayerMask = ~(1 << gameObject.layer);

        dimension1.SetActive(true);
        dimension2.SetActive(false);
    }

    void Start()
    {
        AudioManager.Instance.SetMusicLayer(2, false);
        AudioManager.Instance.SetMusicLayer(3, false);
        AudioManager.Instance.SetMusicLayer(4, false);
        AudioManager.Instance.SetMusicLayer(5, false);
    }

    void Update()
    {
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        turn = gamePadState.ThumbSticks.Left.X;
        accelerate = gamePadState.Buttons.A == ButtonState.Pressed;
        brake = gamePadState.Buttons.X == ButtonState.Pressed;
        reverse = gamePadState.Buttons.B == ButtonState.Pressed;

        leftBrake = gamePadState.Triggers.Left;
        rightBrake = gamePadState.Triggers.Right;

        isBoosting = Time.time < boostTimer;
        isDrifting = (leftBrake > 0.3f || rightBrake > 0.3f);

        //Debug.LogFormat("{0} {1}", leftBrake, rightBrake);
        // transform.RotateAround(transform.position, Vector3.forward, Time.deltaTime * -turn * maxBankAngle);

        cameraTarget.localPosition = new Vector3(turn * (isDrifting ? driftCameraOffset : normalCameraOffset), cameraTarget.localPosition.y, cameraTarget.localPosition.z);

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
        else if (isDrifting)
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

        ApplyVisuals();
        // cameraTarget.forward = GetHeading();
    }

    Vector3 GetHeading()
    {
        Vector3 dir = transform.forward + rb.velocity.normalized;
        dir.y = 0;
        if (dir.sqrMagnitude < Mathf.Epsilon)
        {
            return transform.forward;
        }
        return dir.normalized;
    }

    void FixedUpdate()
    {
        Vector3 heading = transform.forward;

        if (isBoosting)
        {
            forwardDrag = boostDrag;
            accelerate = true;
        }
        else
        {
            forwardDrag = forwardDrags[warps];
        }


        bank = Mathf.MoveTowardsAngle(bank, maxBankAngle * turn, Time.fixedDeltaTime * bankRate);
        // body.LookAt(body.transform.position + heading, Quaternion.AngleAxis(-bank, heading) * Vector3.up);
        Debug.DrawRay(rb.worldCenterOfMass, heading, Color.white);
        // AddRelativeForce(2000 * throttle * heading, Color.green);
        bool appliedUpForce;
        bool groundContact = ApplyUpForce(out appliedUpForce, out groundNormal);
        if (!appliedUpForce)
        {
            float downVel = Mathf.Clamp(rb.velocity.sqrMagnitude * downForce, 0.0f, 100000.0f);
            AddForce(-downVel * groundNormal, Color.black);
        }
        Debug.DrawRay(rb.worldCenterOfMass, rb.velocity, Color.yellow);
        float turnRate = turnPower * (groundContact ? 1.0f : airTurnModifier);
        AddForceAtPosition(turnRate * rb.velocity.magnitude * bank * (Quaternion.AngleAxis(90.0f, Vector3.up) * heading), rb.worldCenterOfMass + heading, Color.cyan);

        if (groundContact)
        {
            if (accelerate)
            {
                AddRelativeForce(accelerationForce * Vector3.forward, Color.green);
            }
            if (reverse)
            {
                AddRelativeForce(-reverseForce * Vector3.forward, Color.green);
            }
            if (brake && Vector3.Dot(transform.forward, rb.velocity) > 0.0f)
            {
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

        if (dimension2.activeSelf && rb.velocity.sqrMagnitude < dimensionMinVelocity)
        {
            ShiftDimension();
            AudioManager.Instance.SetMusicLayer(2, false);
            AudioManager.Instance.SetMusicLayer(3, false);
            AudioManager.Instance.SetMusicLayer(4, false);
            AudioManager.Instance.SetMusicLayer(5, false);
            warps = 0;
        }

        if (rb.velocity.sqrMagnitude < 3000.0f)
        {
            driveAudio.clip = driveClips[0];
        }
        else if (rb.velocity.sqrMagnitude < 8000.0f)
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

    private void ApplyYaw(bool groundContact)
    {
        if (!groundContact)
        {
            return;
        }
        Debug.LogFormat("Speed {0}", rb.velocity.magnitude);

        yawRate = Mathf.MoveTowards(yawRate, turn, Time.fixedDeltaTime / yawDelay);
        float yaw;
        if (yawRate < 0.0f)
        {
            yaw = Mathf.SmoothStep(0.0f, -1.0f, -yawRate);
        }
        else
        {
            yaw = Mathf.SmoothStep(0.0f, 1.0f, yawRate);
        }
        float speedLoss = 1.0f + rb.velocity.magnitude * rb.velocity.magnitude / Mathf.Pow(yawHalfSpeed, 2.0f);
        rb.MoveRotation(Quaternion.AngleAxis(yaw * Time.fixedDeltaTime * yawSpeed / speedLoss, transform.up) * rb.rotation);
    }

    private void ApplyDrag(bool groundContact)
    {
        float mod = groundContact ? 1.0f : airDragModifier;
        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        float rightSpeed = Vector3.Dot(transform.right, rb.velocity);
        AddForce(-mod * forwardDrag * forwardSpeed * Mathf.Abs(forwardSpeed) * transform.forward, Color.magenta);
        AddForce(-mod * sideDrag * rightSpeed * Mathf.Abs(rightSpeed) * transform.right, Color.magenta);
    }

    private void ApplyBrake(float axis, float leverage)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        AddForceAtPosition(-axis * brakeDrag * forwardSpeed * Mathf.Abs(forwardSpeed) * transform.forward, rb.worldCenterOfMass + leverage * transform.right, Color.magenta);
    }

    private bool ApplyUpForce(out bool appliedForce, out Vector3 groundNormal)
    {
        appliedForce = false;
        groundNormal = Vector3.up;
        RaycastHit hitInfo;
        if (!Physics.Raycast(transform.position, -transform.up, out hitInfo, targetHeight + 1.0f, excludeShipLayerMask))
        {
            return false;
        }
        groundNormal = hitInfo.normal;
        float distToGo = targetHeight - hitInfo.distance;
        if (distToGo < 0.0f)
        {
            // AddForce(-(1.0f + distToGo) * Physics.gravity, Color.red);
            return true;
        }

        float velocity = Vector3.Dot(transform.up, rb.velocity);
        float time = distToGo / velocity;
        float gravityComponent = time * Vector3.Dot(transform.up, Physics.gravity);

        bool overshoot = time > 0.0f && velocity + gravityComponent > 0.0f;
        // Debug.Log(velocity + gravityComponent );
        if (!overshoot)
        {
            AddForce(upForce * Physics.gravity.magnitude * rb.mass * transform.up, Color.red);
            appliedForce = true;
        }
        else
        {
        }
        return true;
    }

    private void AddRelativeForce(Vector3 force, Color color)
    {
        AddForce(transform.TransformDirection(force), color);
    }
    private void AddForce(Vector3 force, Color color)
    {
        AddForceAtPosition(force, rb.worldCenterOfMass, color);
    }
    private void AddForceAtPosition(Vector3 force, Vector3 position, Color color)
    {
        Debug.DrawRay(position, debugRayScale * force, color);
        rb.AddForceAtPosition(force, position, ForceMode.Force);
    }

    private void ApplyVisuals()
    {
        // rotate around local X to match local Y with normal's projection to local YZ plane
        // rotate around local Z to match normal
        // local roll to match normal



        Quaternion toGo = Quaternion.FromToRotation(body.up, groundNormal);
        Quaternion targetLocalRotation = body.localRotation * toGo;
        // body.localRotation = Quaternion.RotateTowards(body.localRotation, targetLocalRotation, 60.0f * Time.deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(groundNormal, transform.forward);
        body.rotation = Quaternion.RotateTowards(body.rotation, targetRotation, visualRotationRate * Time.deltaTime);

        // transform.rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);

        // Quaternion.RotateTowards()
        // Vector3 rotationAxis = Vector3.Cross(groundNormal, body.forward);
        // float angleToGo = Vector3.SignedAngle(body.forward, groundNormal, rotationAxis) - 90.0f;
        // body.forward, gr

        // Quaternion.RotateTowards()
        // body.LookAt();
        float speedLoss = 1.0f + rb.velocity.magnitude / yawHalfSpeed;
        float yawSway = 1.4f * Mathf.Cos(0.4f * Time.time * Mathf.PI) / speedLoss;
        float pitchSway = 2.0f * Mathf.Cos(1.0f + 1.0f * Time.time * Mathf.PI) / speedLoss;
        float rollSway = 1.3f * Mathf.Cos(2.3f + 1.6f * Time.time * Mathf.PI) / speedLoss;
        float xSway = 1.4f * Mathf.Cos(0.8f * Time.time * Mathf.PI) / speedLoss;
        float ySway = 2.0f * Mathf.Cos(0.97f + 1.0f * Time.time * Mathf.PI) / speedLoss;
        float zSway = 1.3f * Mathf.Cos(1.04f + 1.6f * Time.time * Mathf.PI) / speedLoss;
        idleBody.localPosition = 0.01f * new Vector3(xSway, ySway, zSway);
        idleBody.localRotation = Quaternion.Euler(pitchSway, yawSway, rollSway);

        float tilt = Mathf.Clamp(Vector3.Dot(transform.forward, rb.velocity) * 0.01f, -1.0f, 1.0f);
        float roll = Mathf.Clamp(rb.angularVelocity.y / 120.0f + rb.velocity.x * 0.1f, -1.0f, 1.0f);

        accBody.localRotation = Quaternion.Euler(-25.0f * tilt, 0, 25.0f * roll);
    }

}