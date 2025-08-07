using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ashsvp
{
    public class SimcadeVehicleController : MonoBehaviour
    {
        [Header("Suspension")] [Space(10)] public float springForce = 30000f;

        public float springDamper = 200f;

        [Header("Car Stats")] [Space(10)] public float MaxSpeed = 200f;

        public float Acceleration;
        public AnimationCurve AccelerationCurve;
        public float MaxTurnAngle = 30f;
        public AnimationCurve turnCurve;
        public float brakeAcceleration = 50f;
        public float RollingResistance = 2f;
        public float driftFactor = 0.2f;
        public float FrictionCoefficient = 1f;
        public AnimationCurve sideFrictionCurve;
        public AnimationCurve forwardFrictionCurve;
        public Transform CenterOfMass_air;
        public bool AutoCounterSteer;
        public float DownForce = 5;


        [Header("Visuals")] [Space(10)] public Transform VehicleBody;

        [Range(0, 10)] public float forwardBodyTilt = 3f;

        [Range(0, 10)] public float sidewaysBodyTilt = 3f;

        public GameObject WheelSkid;
        public GameObject SkidMarkController;
        public float wheelRadius;
        public float skidmarkWidth;
        public Transform[] HardPoints = new Transform[4];
        public Transform[] Wheels;

        [HideInInspector] public Vector3 carVelocity;

        [Header("Events")] [Space(10)] public Vehicle_Events VehicleEvents;

        [HideInInspector] public float steerInput, accelerationInput;

        [HideInInspector] public bool brakeInput;

        [HideInInspector] public float rearTrack, wheelBase, ackermennLeftAngle, ackermennRightAngle;

        [HideInInspector] public Vector3 localVehicleVelocity;

        [HideInInspector] public bool vehicleIsGrounded;

        [HideInInspector] public bool CanDrive, CanAccelerate;


        //Skidmarks
        [HideInInspector] public float[] forwardSlip = new float[4], slipCoeff = new float[4], skidTotal = new float[4];

        private IVehicleController _controls;
        private Vector3 CentreOfMass_ground;

        private GearSystem GearSystem;
        private Vector3 lastVelocity;
        private float MaxSpringDistance;
        private int NumberOfGroundedWheels;

        private readonly float[] offset_Prev = new float[4];

        private Rigidbody rb;
        private bool tempGroundedProperty;

        [Header("Other Things")] private readonly RaycastHit[] wheelHits = new RaycastHit[4];

        private readonly WheelSkid[] wheelSkids = new WheelSkid[4];

        #region Vehicle Events Stuff

        public bool GroundedProperty
        {
            get => tempGroundedProperty;

            set
            {
                if (value != tempGroundedProperty)
                {
                    tempGroundedProperty = value;
                    if (tempGroundedProperty)
                    {
                        Debug.Log("Grounded");
                        VehicleEvents.OnGrounded.Invoke();
                    }
                    else
                    {
                        Debug.Log("Take off");
                        VehicleEvents.OnTakeOff.Invoke();
                    }
                }
            }
        }

        #endregion


        private void Awake()
        {
            var SkidMarkController_Self = Instantiate(SkidMarkController);
            SkidMarkController_Self.GetComponent<Skidmarks>().SkidmarkWidth = skidmarkWidth;

            CanDrive = true;
            CanAccelerate = true;

            rb = GetComponent<Rigidbody>();
            lastVelocity = Vector3.zero;


            for (var i = 0; i < Wheels.Length; i++)
            {
                HardPoints[i].localPosition = new Vector3(Wheels[i].localPosition.x, 0, Wheels[i].localPosition.z);

                wheelSkids[i] = Instantiate(WheelSkid, Wheels[i]).GetComponent<WheelSkid>();
                setWheelSkidvalues_Start(i, SkidMarkController_Self.GetComponent<Skidmarks>(), wheelRadius);
            }

            MaxSpringDistance = Mathf.Abs(Wheels[0].localPosition.y - HardPoints[0].localPosition.y) + 0.1f +
                                wheelRadius;

            wheelBase = Vector3.Distance(Wheels[0].position, Wheels[2].position);
            rearTrack = Vector3.Distance(Wheels[0].position, Wheels[1].position);

            GearSystem = GetComponent<GearSystem>();
        }

        private void Start()
        {
            CentreOfMass_ground = (HardPoints[0].localPosition + HardPoints[1].localPosition +
                                   HardPoints[2].localPosition + HardPoints[3].localPosition) / 4;

            rb.centerOfMass = CentreOfMass_ground;
        }

        private void Update()
        {
            switch (CanDrive)
            {
                case true when CanAccelerate:
                {
                    accelerationInput = _controls.AccelerationInput;
                    steerInput = _controls.SteerInput;
                    brakeInput = _controls.BrakeInput;
                    break;
                }
                case true when !CanAccelerate:
                {
                    accelerationInput = 0;
                    steerInput = _controls.SteerInput;
                    brakeInput = _controls.BrakeInput;
                    break;
                }
                default:
                    accelerationInput = 0;
                    steerInput = 0;
                    brakeInput = true;
                    break;
            }
        }

        private void FixedUpdate()
        {
            localVehicleVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            AckermannSteering(steerInput);

            float suspensionForce = 0;
            for (var i = 0; i < Wheels.Length; i++)
            {
                var wheelIsGrounded = false;

                AddSuspensionForce_2(HardPoints[i].position, Wheels[i], MaxSpringDistance, out wheelHits[i],
                    out wheelIsGrounded, out suspensionForce, i);

                GroundedCheckPerWheel(wheelIsGrounded);

                tireVisual(wheelIsGrounded, Wheels[i], HardPoints[i], wheelHits[i].distance, i);
                setWheelSkidvalues_Update(i, skidTotal[i], wheelHits[i].point, wheelHits[i].normal);
            }

            vehicleIsGrounded = NumberOfGroundedWheels > 1;

            if (vehicleIsGrounded)
            {
                AddAcceleration(accelerationInput);
                AddRollingResistance();
                brakeLogic();
                bodyAnimation();

                //AutoBalence
                if (rb.centerOfMass != CentreOfMass_ground) rb.centerOfMass = CentreOfMass_ground;

                // angular drag
                rb.angularDamping = 1;

                //downforce
                rb.AddForce(-transform.up * DownForce * rb.mass);
            }
            else
            {
                if (rb.centerOfMass != CenterOfMass_air.localPosition) rb.centerOfMass = CenterOfMass_air.localPosition;

                // angular drag
                rb.angularDamping = 0.1f;
            }

            //friction
            for (var i = 0; i < Wheels.Length; i++)
                if (i < 2)
                {
                    AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1,
                        suspensionForce, i);
                }
                else
                {
                    if (brakeInput)
                        AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded,
                            driftFactor, suspensionForce, i);
                    else
                        AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1,
                            suspensionForce, i);
                }


            NumberOfGroundedWheels = 0; //reset grounded int


            //grounded property for event
            if (GroundedProperty != vehicleIsGrounded) GroundedProperty = vehicleIsGrounded;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (var i = 0; i < Wheels.Length; i++)
            {
                Gizmos.DrawLine(HardPoints[i].position + transform.up * wheelRadius, Wheels[i].position);
                Gizmos.DrawWireSphere(Wheels[i].position, wheelRadius);
                Gizmos.DrawSphere(HardPoints[i].position + transform.up * wheelRadius, 0.05f);
            }
        }

        public void SetController(IVehicleController controller)
        {
            _controls = controller;
        }

        private void AddAcceleration(float accelerationInput)
        {
            var deltaSpeed = Acceleration * accelerationInput * Time.fixedDeltaTime;
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed) *
                         AccelerationCurve.Evaluate(Mathf.Abs(localVehicleVelocity.z / MaxSpeed));

            if ((accelerationInput > 0 && localVehicleVelocity.z < 0) ||
                (accelerationInput < 0 && localVehicleVelocity.z > 0))
                deltaSpeed = (1 + Mathf.Abs(localVehicleVelocity.z / MaxSpeed)) * Acceleration * accelerationInput *
                             Time.fixedDeltaTime;
            if (!brakeInput && localVehicleVelocity.z < MaxSpeed) rb.linearVelocity += transform.forward * deltaSpeed;
        }

        private void AddRollingResistance()
        {
            var localSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            var deltaSpeed = RollingResistance * Time.fixedDeltaTime * Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if (accelerationInput == 0)
            {
                if (localSpeed > 0)
                    rb.linearVelocity -= transform.forward * deltaSpeed;
                else
                    rb.linearVelocity += transform.forward * deltaSpeed;
            }
        }

        private void brakeLogic()
        {
            var localSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            var deltaSpeed = brakeAcceleration * (brakeInput ? 1 : 0) * Time.fixedDeltaTime *
                             Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if (localSpeed > 0)
                rb.linearVelocity -= transform.forward * deltaSpeed;
            else
                rb.linearVelocity += transform.forward * deltaSpeed;
        }

        private void AddSuspensionForce(Vector3 hardPoint, Transform wheel, float MaxSpringDistance,
            out RaycastHit wheelHit, out bool WheelIsGrounded, out float SuspensionForce)
        {
            var direction = -transform.up;

            if (Physics.SphereCast(hardPoint, wheelRadius, direction, out wheelHit, MaxSpringDistance))
                WheelIsGrounded = true;
            else
                WheelIsGrounded = false;

            // suspension spring force
            if (WheelIsGrounded)
            {
                var springDir = transform.up;
                var wheelWorldVel = rb.GetPointVelocity(hardPoint);
                var offset = MaxSpringDistance - wheelHit.distance;

                var vel = Vector3.Dot(springDir, wheelWorldVel);
                var force = offset * springForce - vel * springDamper;
                SuspensionForce = force;
                if (offset > 0) rb.AddForceAtPosition(springDir * force, hardPoint);
            }
            else
            {
                SuspensionForce = 0;
            }
        }

        private void AddSuspensionForce_2(Vector3 hardPoint, Transform wheel, float MaxSpringDistance,
            out RaycastHit wheelHit, out bool WheelIsGrounded, out float SuspensionForce, int WheelNum)
        {
            var direction = -transform.up;

            if (Physics.SphereCast(hardPoint + transform.up * wheelRadius, wheelRadius, direction, out wheelHit,
                    MaxSpringDistance))
                WheelIsGrounded = true;
            else
                WheelIsGrounded = false;

            // suspension spring force
            if (WheelIsGrounded)
            {
                var springDir = wheelHit.normal;
                var offset = (MaxSpringDistance + 0.1f - wheelHit.distance) / (MaxSpringDistance - wheelRadius - 0.1f);

                var vel = -((offset - offset_Prev[WheelNum]) / Time.fixedDeltaTime);

                var wheelWorldVel = rb.GetPointVelocity(wheelHit.point);
                var WheelVel = Vector3.Dot(springDir, wheelWorldVel);

                offset_Prev[WheelNum] = offset;
                if (offset < 0.3f)
                    vel = 0;
                else if (vel < 0 && offset > 0.6f && WheelVel < 10) vel *= 10;

                var TotalSpringForce = offset * offset * springForce;
                var totalDampingForce = Mathf.Clamp(-(vel * springDamper),
                    -0.25f * rb.mass * Mathf.Abs(WheelVel) / Time.fixedDeltaTime,
                    0.25f * rb.mass * Mathf.Abs(WheelVel) / Time.fixedDeltaTime);
                if (MaxSpringDistance + 0.1f - wheelHit.distance < 0.1f) totalDampingForce = 0;
                var force = TotalSpringForce + totalDampingForce;
                SuspensionForce = force;

                rb.AddForceAtPosition(springDir * force, hardPoint);
            }
            else
            {
                SuspensionForce = 0;
            }
        }

        public void AddLateralFriction(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool wheelIsGrounded,
            float factor)
        {
            if (wheelIsGrounded)
            {
                var SurfaceNormal = wheelHit.normal;

                var contactVel = wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).x * wheel.right;
                //contactVel = localVehicleVelocity.x * wheel.right;
                //Debug.DrawRay(hardPoint, contactVel.normalized, Color.gray);
                var contactDesiredAccel = -Vector3.ProjectOnPlane(contactVel, SurfaceNormal) / Time.fixedDeltaTime;

                //Vector3 frictionForce = Vector3.ClampMagnitude(rb.mass/4 * contactDesiredAccel, springForce * FrictionCoefficient);
                var frictionForce = rb.mass / 4 * contactDesiredAccel * FrictionCoefficient;

                //Debug.DrawRay(hardPoint, frictionForce.normalized, Color.red);

                rb.AddForceAtPosition(frictionForce * factor, hardPoint);
            }
        }

        public void AddLateralFriction_2(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool wheelIsGrounded,
            float factor, float suspensionForce, int wheelNum)
        {
            if (wheelIsGrounded)
            {
                var SurfaceNormal = wheelHit.normal;

                var sideVelocity = wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).x * wheel.right;
                var forwardVelocity = wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).z * wheel.forward;

                slipCoeff[wheelNum] = sideVelocity.magnitude / (sideVelocity.magnitude +
                                                                Mathf.Clamp(forwardVelocity.magnitude, 0.1f,
                                                                    forwardVelocity.magnitude));

                var contactDesiredAccel = -Vector3.ProjectOnPlane(sideVelocity, SurfaceNormal) / Time.fixedDeltaTime;

                var frictionForce = Vector3.ClampMagnitude(
                    rb.mass * contactDesiredAccel * sideFrictionCurve.Evaluate(slipCoeff[wheelNum]),
                    suspensionForce * FrictionCoefficient);
                frictionForce = suspensionForce * FrictionCoefficient * -sideVelocity.normalized *
                                sideFrictionCurve.Evaluate(slipCoeff[wheelNum]);

                var clampedFrictionForce =
                    Mathf.Min(rb.mass / 4 * contactDesiredAccel.magnitude, -Physics.gravity.y * rb.mass);

                frictionForce =
                    Vector3.ClampMagnitude(
                        frictionForce * forwardFrictionCurve.Evaluate(forwardVelocity.magnitude / MaxSpeed),
                        clampedFrictionForce);
                rb.AddForceAtPosition(frictionForce * factor, hardPoint);
            }
        }


        private void AckermannSteering(float steerInput)
        {
            var turnRadius = wheelBase / Mathf.Tan(MaxTurnAngle / Mathf.Rad2Deg);
            if (steerInput > 0) //is turning right
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
            }
            else if (steerInput < 0) //is turning left
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
            }
            else
            {
                ackermennLeftAngle = 0;
                ackermennRightAngle = 0;
            }

            // auto counter steering
            if (localVehicleVelocity.z > 0 && AutoCounterSteer && Mathf.Abs(localVehicleVelocity.x) > 1f)
            {
                ackermennLeftAngle += Vector3.SignedAngle(transform.forward, rb.linearVelocity + transform.forward,
                    transform.up);
                ackermennLeftAngle = Mathf.Clamp(ackermennLeftAngle, -70, 70);
                ackermennRightAngle += Vector3.SignedAngle(transform.forward, rb.linearVelocity + transform.forward,
                    transform.up);
                ackermennRightAngle = Mathf.Clamp(ackermennRightAngle, -70, 70);
            }

            Wheels[0].localRotation = Quaternion.Euler(0,
                ackermennLeftAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
            Wheels[1].localRotation = Quaternion.Euler(0,
                ackermennRightAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
        }

        private void tireVisual(bool WheelIsGrounded, Transform wheel, Transform hardPoint, float hitDistance,
            int tireNum)
        {
            if (WheelIsGrounded)
            {
                if (offset_Prev[tireNum] > 0.3f)
                    wheel.localPosition = hardPoint.localPosition + Vector3.up * wheelRadius - Vector3.up * hitDistance;
                else
                    wheel.localPosition =
                        Vector3.Lerp(
                            new Vector3(hardPoint.localPosition.x, wheel.localPosition.y, hardPoint.localPosition.z),
                            hardPoint.localPosition + Vector3.up * wheelRadius - Vector3.up * hitDistance, 0.1f);
            }
            else
            {
                wheel.localPosition =
                    Vector3.Lerp(
                        new Vector3(hardPoint.localPosition.x, wheel.localPosition.y, hardPoint.localPosition.z),
                        hardPoint.localPosition + Vector3.up * wheelRadius - Vector3.up * MaxSpringDistance, 0.05f);
            }

            var wheelVelocity = rb.GetPointVelocity(hardPoint.position);
            var minRotation = Vector3.Dot(wheelVelocity, wheel.forward) / wheelRadius * Time.fixedDeltaTime *
                              Mathf.Rad2Deg;
            var maxRotation = Mathf.Sign(Vector3.Dot(wheelVelocity, wheel.forward)) * MaxSpeed / wheelRadius *
                              Time.fixedDeltaTime * Mathf.Rad2Deg;
            float wheelRotation = 0;

            if (brakeInput)
            {
                wheelRotation = 0;
            }
            else if (Mathf.Abs(accelerationInput) > 0.1f)
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, maxRotation / 2);
                wheelRotation = maxRotation;
            }
            else
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, minRotation);
                wheelRotation = minRotation;
            }

            wheel.GetChild(0).localPosition = Vector3.zero;
            var rot = wheel.GetChild(0).localRotation;
            rot.y = 0;
            rot.z = 0;
            wheel.GetChild(0).localRotation = rot;

            //wheel slip calculation
            forwardSlip[tireNum] = Mathf.Abs(Mathf.Clamp((wheelRotation - minRotation) / maxRotation, -1, 1));
            if (WheelIsGrounded)
                skidTotal[tireNum] = Mathf.MoveTowards(skidTotal[tireNum],
                    (forwardSlip[tireNum] + slipCoeff[tireNum]) / 2, 0.05f);
            else
                skidTotal[tireNum] = 0;
        }

        private void setWheelSkidvalues_Start(int wheelNum, Skidmarks skidmarks, float radius)
        {
            wheelSkids[wheelNum].skidmarks = skidmarks;
            wheelSkids[wheelNum].radius = wheelRadius;
        }

        private void setWheelSkidvalues_Update(int wheelNum, float skidTotal, Vector3 skidPoint, Vector3 normal)
        {
            wheelSkids[wheelNum].skidTotal = skidTotal;
            wheelSkids[wheelNum].skidPoint = skidPoint;
            wheelSkids[wheelNum].normal = normal;
        }


        private void bodyAnimation()
        {
            var accel = Vector3.ProjectOnPlane((rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime, transform.up);
            accel = transform.InverseTransformDirection(accel);
            lastVelocity = rb.linearVelocity;

            VehicleBody.localRotation = Quaternion.Lerp(VehicleBody.localRotation,
                Quaternion.Euler(Mathf.Clamp(-accel.z / 10, -forwardBodyTilt, forwardBodyTilt), 0,
                    Mathf.Clamp(accel.x / 5, -sidewaysBodyTilt, sidewaysBodyTilt)), 0.1f);
        }

        private void GroundedCheckPerWheel(bool wheelIsGrounded)
        {
            if (wheelIsGrounded) NumberOfGroundedWheels += 1;
        }

        [Serializable]
        public class Vehicle_Events
        {
            public UnityEvent OnTakeOff;
            public UnityEvent OnGrounded;
            public UnityEvent OnGearChange;
        }
    }

    public interface IVehicleController
    {
        float AccelerationInput { get; }
        float SteerInput { get; }
        bool BrakeInput { get; }
    }
}