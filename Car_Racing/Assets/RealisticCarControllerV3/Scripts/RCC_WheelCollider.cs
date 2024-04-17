//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------


using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Based on Unity's WheelCollider. Modifies few curves, settings in order to get stable and realistic physics depends on selected behavior in RCC Settings.
/// </summary>
[RequireComponent(typeof(WheelCollider))]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/RCC Wheel Collider")]
public class RCC_WheelCollider : RCC_Core {

    // Getting an Instance of Main Shared RCC Settings.
    #region RCC Settings Instance

    private RCC_Settings RCCSettingsInstance;
    private RCC_Settings RCCSettings {
        get {
            if (RCCSettingsInstance == null) {
                RCCSettingsInstance = RCC_Settings.Instance;
                return RCCSettingsInstance;
            }
            return RCCSettingsInstance;
        }
    }

    #endregion

    // Getting an Instance of Ground Materials.
    #region RCC_GroundMaterials Instance

    private RCC_GroundMaterials RCCGroundMaterialsInstance;
    private RCC_GroundMaterials RCCGroundMaterials {
        get {
            if (RCCGroundMaterialsInstance == null) {
                RCCGroundMaterialsInstance = RCC_GroundMaterials.Instance;
            }
            return RCCGroundMaterialsInstance;
        }
    }

    #endregion

    // Getting an Instance of Skidmarks Manager.
    #region RCC_SkidmarksManager Instance
    private RCC_SkidmarksManager RCC_SkidmarksManagerInstance;
    private RCC_SkidmarksManager RCCSkidmarksManager {
        get {
            if (RCC_SkidmarksManagerInstance == null) {
                RCC_SkidmarksManagerInstance = RCC_SkidmarksManager.Instance;
            }
            return RCC_SkidmarksManagerInstance;
        }
    }

    #endregion

    private RCC_GroundMaterials physicsMaterials { get { return RCCGroundMaterials; } }     // Getting instance of Configurable Ground Materials.
    private RCC_GroundMaterials.GroundMaterialFrictions[] physicsFrictions { get { return RCCGroundMaterials.frictions; } }

    // WheelCollider.
    private WheelCollider _wheelCollider;
    public WheelCollider wheelCollider {
        get {
            if (_wheelCollider == null)
                _wheelCollider = GetComponent<WheelCollider>();
            return _wheelCollider;
        }
    }

    // Car controller.
    private RCC_CarControllerV3 _carController;
    public RCC_CarControllerV3 carController {
        get {
            if (_carController == null)
                _carController = GetComponentInParent<RCC_CarControllerV3>();
            return _carController;
        }
    }

    // Rigidbody of the vehicle.
    private Rigidbody _rigid;
    public Rigidbody rigid {
        get {
            if (_rigid == null)
                _rigid = carController.gameObject.GetComponent<Rigidbody>();
            return _rigid;
        }
    }

    private List<RCC_WheelCollider> allWheelColliders = new List<RCC_WheelCollider>();      // All wheelcolliders attached to this vehicle.
    public Transform wheelModel;        // Wheel model for animating and aligning.

    public WheelHit wheelHit;               //	Wheel Hit data.
    public bool isGrounded = false;     //	Is wheel grounded?

    // Locating correct position and rotation for the wheel.
    [HideInInspector] public Vector3 wheelPosition = Vector3.zero;
    [HideInInspector] public Quaternion wheelRotation = Quaternion.identity;

    [Space()]
    public bool canPower = false;       //	Can this wheel power?
    [Range(-1f, 1f)] public float powerMultiplier = 1f;
    public bool canSteer = false;       //	Can this wheel steer?
    [Range(-1f, 1f)] public float steeringMultiplier = 1f;
    public bool canBrake = false;       //	Can this wheel brake?
    [Range(0f, 1f)] public float brakingMultiplier = 1f;
    public bool canHandbrake = false;       //	Can this wheel handbrake?
    [Range(0f, 1f)] public float handbrakeMultiplier = 1f;

    [Space()]
    public float width = .275f; //	Width.
    public float offset = .05f;     // Offset by X axis.

    private float wheelRPM2Speed = 0f;     // Wheel RPM to Speed.

    [Range(-5f, 5f)] public float camber = 0f;      // Camber angle.
    [Range(-5f, 5f)] public float caster = 0f;      // Caster angle.
    [Range(-5f, 5f)] public float toe = 0f;              // Toe angle.

    //	Skidmarks
    private int lastSkidmark = -1;

    //	Slips
    [HideInInspector] public float wheelSlipAmountForward = 0f;       // Forward slip.
    [HideInInspector] public float wheelSlipAmountSideways = 0f;  // Sideways slip.
    [HideInInspector] public float totalSlip = 0f;                              // Total amount of forward and sideways slips.

    //	WheelFriction Curves and Stiffness.
    private WheelFrictionCurve forwardFrictionCurve;        //	Forward friction curve.
    private WheelFrictionCurve sidewaysFrictionCurve;   //	Sideways friction curve.

    //	Audio
    private AudioSource audioSource;        // Audiosource for tire skid SFX.
    private AudioClip audioClip;                    // Audioclip for tire skid SFX.
    private float audioVolume = 1f;         //	Maximum volume for tire skid SFX.

    private int groundIndex = 0;        // Current ground physic material index.

    // List for all particle systems.
    [HideInInspector] public List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
    private ParticleSystem.EmissionModule emission;

    //	Tractions used for smooth drifting.
    [HideInInspector] public float tractionHelpedSidewaysStiffness = 1f;
    private float minForwardStiffness = .75f;
    private float maxForwardStiffness = 1f;
    private float minSidewaysStiffness = .75f;
    private float maxSidewaysStiffness = 1f;

    // Getting bump force.
    [HideInInspector] public float bumpForce;
    private float oldForce;
    private float RotationValue;
    void Start() {

        // Getting all WheelColliders attached to this vehicle (Except this).
        allWheelColliders = carController.GetComponentsInChildren<RCC_WheelCollider>().ToList();

        CheckBehavior();        //	Checks selected behavior in RCC Settings.

        // Increasing WheelCollider mass for avoiding unstable behavior.
        if (RCCSettings.useFixedWheelColliders)
            wheelCollider.mass = rigid.mass / 15f;

        // Creating audiosource for skid SFX.
        audioSource = NewAudioSource(RCCSettings.audioMixer, carController.gameObject, "Skid Sound AudioSource", 5f, 50f, 0f, audioClip, true, true, false);
        audioSource.transform.position = transform.position;

        // Creating all ground particles, and adding them to list.
        if (!RCCSettings.dontUseAnyParticleEffects) {

            for (int i = 0; i < RCCGroundMaterials.frictions.Length; i++) {

                GameObject ps = (GameObject)Instantiate(RCCGroundMaterials.frictions[i].groundParticles, transform.position, transform.rotation) as GameObject;
                emission = ps.GetComponent<ParticleSystem>().emission;
                emission.enabled = false;
                ps.transform.SetParent(transform, false);
                ps.transform.localPosition = Vector3.zero;
                ps.transform.localRotation = Quaternion.identity;
                allWheelParticles.Add(ps.GetComponent<ParticleSystem>());

            }

        }

        //	Creating pivot position of the wheel at correct position and rotation.
        GameObject newPivot = new GameObject("Pivot_" + wheelModel.transform.name);
        newPivot.transform.position = RCC_GetBounds.GetBoundsCenter(wheelModel.transform);
        newPivot.transform.rotation = transform.rotation;
        newPivot.transform.SetParent(wheelModel.transform.parent, true);

        //	Settings offsets.
        if (newPivot.transform.localPosition.x > 0)
            wheelModel.transform.position += transform.right * offset;
        else
            wheelModel.transform.position -= transform.right * offset;

        //	Assigning temporary created wheel to actual wheel.
        wheelModel.SetParent(newPivot.transform, true);
        wheelModel = newPivot.transform;

        // Override wheels automatically if enabled.
        if (!carController.overrideWheelAll) {

            // Overriding canPower, canSteer, canBrake, canHandbrake.
            if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider) {

                canSteer = true;
                canBrake = true;
                brakingMultiplier = 1f;

            }

            if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider) {

                canHandbrake = true;
                canBrake = true;
                brakingMultiplier = .5f;

            }

        }

    }

    void OnEnable() {

        // Listening an event when main behavior changed.
        RCC_SceneManager.OnBehaviorChanged += CheckBehavior;
        wheelModel.gameObject.SetActive(true);

    }

    private void CheckBehavior() {

        // Getting friction curves.
        forwardFrictionCurve = wheelCollider.forwardFriction;
        sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

        //	Getting behavior if selected.
        RCC_Settings.BehaviorType behavior = RCCSettings.selectedBehaviorType;

        //	If there is a selected behavior, override friction curves.
        if (behavior != null) {

            forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, behavior.forwardExtremumSlip, behavior.forwardExtremumValue, behavior.forwardAsymptoteSlip, behavior.forwardAsymptoteValue);
            sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, behavior.sidewaysExtremumSlip, behavior.sidewaysExtremumValue, behavior.sidewaysAsymptoteSlip, behavior.sidewaysAsymptoteValue);

        }

        // Assigning new frictons.
        wheelCollider.forwardFriction = forwardFrictionCurve;
        wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

    }

    void Update() {

        // Return if RCC is disabled.
        if (!carController.enabled)
            return;

        // Setting position and rotation of the wheel model.
        WheelAlign();

    }

    void FixedUpdate() {

        isGrounded = wheelCollider.GetGroundHit(out wheelHit);
        groundIndex = GetGroundMaterialIndex();

        // Forward, sideways, and total slips.
        if (isGrounded && wheelHit.point != Vector3.zero) {

            wheelSlipAmountForward = wheelHit.forwardSlip;
            wheelSlipAmountSideways = wheelHit.sidewaysSlip;

        } else {

            wheelSlipAmountForward = 0f;
            wheelSlipAmountSideways = 0f;

        }

        totalSlip = Mathf.Lerp(totalSlip, ((Mathf.Abs(wheelSlipAmountSideways) + Mathf.Abs(wheelSlipAmountForward)) / 2f), Time.fixedDeltaTime * 10f);

        float circumFerence = 2.0f * 3.14f * wheelCollider.radius; // Finding circumFerence 2 Pi R.
        wheelRPM2Speed = (circumFerence * wheelCollider.rpm) * 60; // Finding KMH.
        wheelRPM2Speed = Mathf.Clamp(wheelRPM2Speed / 1000f, 0f, Mathf.Infinity);

        // Setting power state of the wheels depending on drivetrain mode. Only overrides them if overrideWheels is enabled for the vehicle.
        if (!carController.overrideWheelAll) {

            switch (carController.wheelTypeChoise) {

                case RCC_CarControllerV3.WheelType.AWD:
                    canPower = true;

                    break;

                case RCC_CarControllerV3.WheelType.BIASED:
                    canPower = true;

                    break;

                case RCC_CarControllerV3.WheelType.FWD:

                    if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
                        canPower = true;
                    else
                        canPower = false;

                    break;

                case RCC_CarControllerV3.WheelType.RWD:

                    if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
                        canPower = true;
                    else
                        canPower = false;

                    break;

            }

        }

        Frictions();
        SkidMarks();
        Audio();
        Particles();

        // Return if RCC is disabled.
        if (!carController.enabled)
            return;

        #region ESP.

        // ESP System. All wheels have individual brakes. In case of loosing control of the vehicle, corresponding wheel will brake for gaining the control again.
        if (carController.ESP && carController.brakeInput < .5f) {

            if (carController.handbrakeInput < .5f) {

                if (carController.underSteering) {

                    if (this == carController.FrontLeftWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.rearSlip, 0f, Mathf.Infinity));

                    if (this == carController.FrontRightWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.rearSlip, 0f, Mathf.Infinity));

                }

                if (carController.overSteering) {

                    if (this == carController.RearLeftWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.frontSlip, 0f, Mathf.Infinity));

                    if (this == carController.RearRightWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.frontSlip, 0f, Mathf.Infinity));

                }

            }

        }

        #endregion

    }

    // Aligning wheel model position and rotation.
    private void WheelAlign() {

        // Return if no wheel model selected.
        if (!wheelModel) {

            Debug.LogError(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
            enabled = false;
            return;

        }

        wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);

        //Increase the rotation value
        RotationValue += wheelCollider.rpm * (360f / 60f) * Time.deltaTime;

        //	Assigning position and rotation to the wheel model.
        wheelModel.transform.position = wheelPosition;
        wheelModel.transform.rotation = transform.rotation * Quaternion.Euler(RotationValue, wheelCollider.steerAngle, 0f);

        //	Adjusting offset by X axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.position += transform.right * offset;
        else
            wheelModel.transform.position -= transform.right * offset;

        // Adjusting camber angle by Z axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, -camber);
        else
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, camber);

        // Adjusting caster angle by X axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, -caster);
        else
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, caster);

    }

    /// <summary>
    /// Skidmarks.
    /// </summary>
    private void SkidMarks() {

        isGrounded = wheelCollider.GetGroundHit(out wheelHit);

        //// Forward, sideways, and total slips.
        //if (isGrounded && wheelHit.point != Vector3.zero) {

        //    wheelSlipAmountForward = wheelHit.forwardSlip;
        //    wheelSlipAmountSideways = wheelHit.sidewaysSlip;

        //} else {

        //    wheelSlipAmountForward = 0f;
        //    wheelSlipAmountSideways = 0f;

        //}

        //totalSlip = Mathf.Lerp(totalSlip, ((Mathf.Abs(wheelSlipAmountSideways) + Mathf.Abs(wheelSlipAmountForward)) / 2f), Time.fixedDeltaTime * 10f);

        // If scene has skidmarks manager...
        if (!RCCSettings.dontUseSkidmarks) {

            // If slips are bigger than target value...
            if (totalSlip > physicsFrictions[groundIndex].slip) {

                Vector3 skidPoint = wheelHit.point + 1f * (rigid.velocity) * Time.deltaTime;

                if (rigid.velocity.magnitude > 1f && isGrounded && wheelHit.normal != Vector3.zero && wheelHit.point != Vector3.zero && skidPoint != Vector3.zero && Mathf.Abs(skidPoint.x) > 1f && Mathf.Abs(skidPoint.z) > 1f)
                    lastSkidmark = RCC_SkidmarksManager.Instance.AddSkidMark(skidPoint, wheelHit.normal, totalSlip - physicsFrictions[groundIndex].slip, width, lastSkidmark, groundIndex);
                else
                    lastSkidmark = -1;

            } else {

                lastSkidmark = -1;

            }

        }

    }

    /// <summary>
    /// Sets forward and sideways frictions.
    /// </summary>
    private void Frictions() {

        // Handbrake input clamped 0f - 1f.
        float hbInput = carController.handbrakeInput;

        if (canHandbrake && hbInput > .75f)
            hbInput = .75f;
        else
            hbInput = 1f;

        // Setting wheel stiffness to ground physic material stiffness.
        forwardFrictionCurve.stiffness = physicsFrictions[groundIndex].forwardStiffness;
        sidewaysFrictionCurve.stiffness = (physicsFrictions[groundIndex].sidewaysStiffness * hbInput * tractionHelpedSidewaysStiffness);

        // If drift mode is selected, apply specific frictions.
        if (RCCSettings.selectedBehaviorType != null && RCCSettings.selectedBehaviorType.applyExternalWheelFrictions)
            Drift();

        // Setting new friction curves to wheels.
        wheelCollider.forwardFriction = forwardFrictionCurve;
        wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

        // Also damp too.
        wheelCollider.wheelDampingRate = physicsFrictions[groundIndex].damp;

        // Set audioclip to ground physic material sound.
        audioClip = physicsFrictions[groundIndex].groundSound;
        audioVolume = physicsFrictions[groundIndex].volume;

    }

    /// <summary>
    /// Particles.
    /// </summary>
    private void Particles() {

        if (RCCSettings.dontUseAnyParticleEffects)
            return;

        // If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
        for (int i = 0; i < allWheelParticles.Count; i++) {

            if (totalSlip > physicsFrictions[groundIndex].slip) {

                if (i != groundIndex) {

                    ParticleSystem.EmissionModule em;

                    em = allWheelParticles[i].emission;
                    em.enabled = false;

                } else {

                    ParticleSystem.EmissionModule em;

                    em = allWheelParticles[i].emission;
                    em.enabled = true;

                }

            } else {

                ParticleSystem.EmissionModule em;

                em = allWheelParticles[i].emission;
                em.enabled = false;

            }

            if (isGrounded && wheelHit.point != Vector3.zero)
                allWheelParticles[i].transform.position = wheelHit.point + (.05f * transform.up);

        }

    }

    /// <summary>
    /// Drift.
    /// </summary>
    private void Drift() {

        Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);

        float sqrVel = (relativeVelocity.x * relativeVelocity.x) / 250f;
        sqrVel += (Mathf.Abs(wheelHit.forwardSlip * wheelHit.forwardSlip) * .5f);

        // Forward
        if (wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider) {
            forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .5f, maxForwardStiffness);
            forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minForwardStiffness);
        } else {
            forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, 1f, maxForwardStiffness);
            forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), 1.5f, minForwardStiffness);
        }

        // Sideways
        if (wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider) {
            sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .5f, maxSidewaysStiffness);
            sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
        } else {
            sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .5f, maxSidewaysStiffness);
            sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
        }

    }

    /// <summary>
    /// Audio.
    /// </summary>
    private void Audio() {

        // If total slip is high enough...
        if (totalSlip > physicsFrictions[groundIndex].slip) {

            // Assigning corresponding audio clip.
            if (audioSource.clip != audioClip)
                audioSource.clip = audioClip;

            // Playing it.
            if (!audioSource.isPlaying)
                audioSource.Play();

            // If vehicle is moving, set volume and pitch. Otherwise set them to 0.
            if (rigid.velocity.magnitude > 1f) {

                audioSource.volume = Mathf.Lerp(0f, audioVolume, totalSlip - 0);
                audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);

            } else {

                audioSource.volume = 0f;

            }

        } else {

            audioSource.volume = 0f;

            // If volume is minimal and audio is still playing, stop.
            if (audioSource.volume <= .05f && audioSource.isPlaying)
                audioSource.Stop();

        }

        // Calculating bump force.
        bumpForce = wheelHit.force - oldForce;

        //	If bump force is high enough, play bump SFX.
        if ((bumpForce) >= 5000f) {

            // Creating and playing audiosource for bump SFX.
            AudioSource bumpSound = NewAudioSource(RCCSettings.audioMixer, carController.gameObject, "Bump Sound AudioSource", 5f, 50f, (bumpForce - 5000f) / 3000f, RCCSettingsInstance.bumpClip, false, true, true);
            bumpSound.pitch = Random.Range(.9f, 1.1f);

        }

        oldForce = wheelHit.force;

    }

    /// <summary>
    /// Returns true if one of the wheel is slipping.
    /// </summary>
    /// <returns><c>true</c>, if skidding was ised, <c>false</c> otherwise.</returns>
    private bool IsSkidding() {

        for (int i = 0; i < allWheelColliders.Count; i++) {

            if (allWheelColliders[i].totalSlip > physicsFrictions[groundIndex].slip)
                return true;

        }

        return false;

    }

    /// <summary>
    /// Applies the motor torque.
    /// </summary>
    /// <param name="torque">Torque.</param>
    public void ApplyMotorTorque(float torque) {

        //	If TCS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
        if (carController.TCS) {

            if (Mathf.Abs(wheelCollider.rpm) >= 1) {

                if (Mathf.Abs(wheelSlipAmountForward) > physicsFrictions[groundIndex].slip) {

                    carController.TCSAct = true;

                    torque -= Mathf.Clamp(torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, -Mathf.Infinity, Mathf.Infinity);

                    if (wheelCollider.rpm > 1) {

                        torque -= Mathf.Clamp(torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, 0f, Mathf.Infinity);
                        torque = Mathf.Clamp(torque, 0f, Mathf.Infinity);

                    } else {

                        torque += Mathf.Clamp(-torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, 0f, Mathf.Infinity);
                        torque = Mathf.Clamp(torque, -Mathf.Infinity, 0f);

                    }

                } else {

                    carController.TCSAct = false;

                }

            } else {

                carController.TCSAct = false;

            }

        }

        if (OverTorque())
            torque = 0;

        if (Mathf.Abs(torque) > 1f)
            wheelCollider.motorTorque = torque;
        else
            wheelCollider.motorTorque = 0f;

    }

    /// <summary>
    /// Applies the steering.
    /// </summary>
    /// <param name="steerInput">Steer input.</param>
    /// <param name="angle">Angle.</param>
    public void ApplySteering(float steerInput, float angle) {

        //	Ackerman steering formula.
        if (steerInput > 0f) {

            if (transform.localPosition.x < 0)
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 + (1.5f / 2))) * steerInput);
            else
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 - (1.5f / 2))) * steerInput);

        } else if (steerInput < 0f) {

            if (transform.localPosition.x < 0)
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 - (1.5f / 2))) * steerInput);
            else
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 + (1.5f / 2))) * steerInput);

        } else {

            wheelCollider.steerAngle = 0f;

        }

        if (transform.localPosition.x < 0)
            wheelCollider.steerAngle += toe;
        else
            wheelCollider.steerAngle -= toe;

    }

    /// <summary>
    /// Applies the brake torque.
    /// </summary>
    /// <param name="torque">Torque.</param>
    public void ApplyBrakeTorque(float torque) {

        //	If ABS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
        if (carController.ABS && carController.handbrakeInput <= .1f) {

            if ((Mathf.Abs(wheelHit.forwardSlip) * Mathf.Clamp01(torque)) >= carController.ABSThreshold) {

                carController.ABSAct = true;
                torque = 0;

            } else {

                carController.ABSAct = false;

            }

        }

        if (Mathf.Abs(torque) > 1f)
            wheelCollider.brakeTorque = torque;
        else
            wheelCollider.brakeTorque = 0f;

    }

    /// <summary>
    /// Checks if overtorque applying.
    /// </summary>
    /// <returns><c>true</c>, if torque was overed, <c>false</c> otherwise.</returns>
    private bool OverTorque() {

        if (carController.speed > carController.maxspeed || (carController.speed > carController.gears[carController.currentGear].maxSpeed && carController.engineRPM > (carController.maxEngineRPM)) || !carController.engineRunning)
            return true;

        return false;

    }

    /// <summary>
    /// Converts to splat map coordinate.
    /// </summary>
    /// <returns>The to splat map coordinate.</returns>
    /// <param name="playerPos">Player position.</param>
    private Vector3 ConvertToSplatMapCoordinate(Terrain terrain, Vector3 playerPos) {

        Vector3 vecRet = new Vector3();
        Vector3 terPosition = terrain.transform.position;
        vecRet.x = ((playerPos.x - terPosition.x) / terrain.terrainData.size.x) * terrain.terrainData.alphamapWidth;
        vecRet.z = ((playerPos.z - terPosition.z) / terrain.terrainData.size.z) * terrain.terrainData.alphamapHeight;
        return vecRet;

    }

    /// <summary>
    /// Gets the index of the ground material.
    /// </summary>
    /// <returns>The ground material index.</returns>
    private int GetGroundMaterialIndex() {

        // Contacted any physic material in Configurable Ground Materials yet?
        bool contacted = false;

        if (wheelHit.point == Vector3.zero)
            return 0;

        if (wheelHit.collider == null)
            return 0;

        int ret = 0;

        for (int i = 0; i < physicsFrictions.Length; i++) {

            if (wheelHit.collider.sharedMaterial == physicsFrictions[i].groundMaterial) {

                contacted = true;
                ret = i;

            }

        }

        // If ground pyhsic material is not one of the ground material in Configurable Ground Materials, check if we are on terrain collider...
        if (!contacted) {

            if (!RCC_SceneManager.Instance.terrainsInitialized)
                return 0;

            for (int i = 0; i < RCCGroundMaterials.terrainFrictions.Length; i++) {

                if (wheelHit.collider.sharedMaterial == RCCGroundMaterials.terrainFrictions[i].groundMaterial) {

                    RCC_SceneManager.Terrains currentTerrain = null;

                    for (int l = 0; l < RCC_SceneManager.Instance.terrains.Length; l++) {

                        if (RCC_SceneManager.Instance.terrains[l].terrainCollider == RCCGroundMaterials.terrainFrictions[i].groundMaterial) {

                            currentTerrain = RCC_SceneManager.Instance.terrains[l];
                            break;

                        }

                    }

                    if (currentTerrain != null) {

                        Vector3 playerPos = transform.position;
                        Vector3 TerrainCord = ConvertToSplatMapCoordinate(currentTerrain.terrain, playerPos);
                        float comp = 0f;

                        for (int k = 0; k < currentTerrain.mNumTextures; k++) {

                            if (comp < currentTerrain.mSplatmapData[(int)TerrainCord.z, (int)TerrainCord.x, k])
                                ret = k;

                        }

                        ret = RCCGroundMaterialsInstance.terrainFrictions[i].splatmapIndexes[ret].index;

                    }

                }

            }

        }

        return ret;

    }

    /// <summary>
    /// Sets a new friction to WheelCollider.
    /// </summary>
    /// <returns>The friction curves.</returns>
    /// <param name="curve">Curve.</param>
    /// <param name="extremumSlip">Extremum slip.</param>
    /// <param name="extremumValue">Extremum value.</param>
    /// <param name="asymptoteSlip">Asymptote slip.</param>
    /// <param name="asymptoteValue">Asymptote value.</param>
    public WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue) {

        WheelFrictionCurve newCurve = curve;

        newCurve.extremumSlip = extremumSlip;
        newCurve.extremumValue = extremumValue;
        newCurve.asymptoteSlip = asymptoteSlip;
        newCurve.asymptoteValue = asymptoteValue;

        return newCurve;

    }

    void OnDisable() {

        RCC_SceneManager.OnBehaviorChanged -= CheckBehavior;

        if(wheelModel)
            wheelModel.gameObject.SetActive(false);

        groundIndex = 0;
        audioVolume = 0f;
        wheelSlipAmountForward = 0f;
        wheelSlipAmountSideways = 0f;

        if (audioSource) {

            audioSource.volume = 0f;
            audioSource.Stop();

        }

    }

    /// <summary>
    /// Raises the draw gizmos event.
    /// </summary>
    void OnDrawGizmos() {

#if UNITY_EDITOR
        if (Application.isPlaying) {

            WheelHit hit;
            wheelCollider.GetGroundHit(out hit);

            // Drawing gizmos for wheel forces and slips.
            float extension = (-wheelCollider.transform.InverseTransformPoint(hit.point).y - (wheelCollider.radius * transform.lossyScale.y)) / wheelCollider.suspensionDistance;
            Debug.DrawLine(hit.point, hit.point + transform.up * (hit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
            Debug.DrawLine(hit.point, hit.point - transform.forward * hit.forwardSlip * 2f, Color.green);
            Debug.DrawLine(hit.point, hit.point - transform.right * hit.sidewaysSlip * 2f, Color.red);

        }
#endif

    }

}