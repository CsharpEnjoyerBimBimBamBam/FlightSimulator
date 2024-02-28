using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ControlSurfaces : MonoBehaviour, IPausable
{
    [SerializeField] private Transform LeftAileron;
    [SerializeField] private Transform RightAileron;
    [SerializeField] private Transform LeftElevator;
    [SerializeField] private Transform RightElevator;
    [SerializeField] private Transform LeftRudder;
    [SerializeField] private Transform RightRudder;
    [SerializeField] private ParticleSystem LeftEngineAfterburner;
    [SerializeField] private ParticleSystem RightEngineAfterburner;
    public float PitchRotationCoefficient = 1.5f;
    public float RollRotationCoefficient = 1;
    public float YawRotationCoefficient = 1;
    public float PitchTrimmerCoefficient = 300;
    public float CommandThrottle = 0;
    private Rigidbody PlaneRigidbody;
    private RigidbodyPhysics PlanePhysics;
    private float TrueThrottle = 0;
    private float Trimmer = 0;
    private float MaxThrust = 250000;
    private float AileronsNeutralAngle = 0;
    private float ElevatorsNeutralAngle = 0;
    private float AileronsRotationSpeed = 300;
    private float ElevatorsRotationSpeed = 200;
    private float RuddersRotationSpeed = 200;
    private float CurrentLeftAileronAngle = 0;
    private float CurrentRightAileronAngle = 0;
    private float CurrentElevatorsAngle = 0;
    private float CurrentRuddersAngle = 0;
    private float ElevatorsMaxTrimmerAngle = 10;
    private float _MaxRuddersAngle = 30;
    private float _MinRuddersAngle = -30;
    private float _MaxAileronAngle = 30;
    private float _MinAileronAngle = -20;
    private float _MaxElevatorsAngle = 30;
    private float _MinElevatorsAngle = -30;

    void Start()
    {
        PlaneRigidbody = transform.GetComponent<Rigidbody>();
        PlanePhysics = GetComponent<RigidbodyPhysics>();
        IPausable.OnGamePaused.AddListener(Pause);
        IPausable.OnGameUnpaused.AddListener(Unpause);
    }

    void Update()
    {
        if (IPausable.IsGamePaused)
            return;
        if (Input.GetKey(KeyCode.LeftShift))
            CommandThrottle += 0.3f * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftControl))
            CommandThrottle -= 0.3f * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightControl))
            Trimmer -= 0.3f * Time.deltaTime;           
        else if (Input.GetKey(KeyCode.RightShift))
            Trimmer += 0.3f * Time.deltaTime;

        CommandThrottle = Mathf.Clamp(CommandThrottle, 0, 1);
        Trimmer = Mathf.Clamp(Trimmer, -1, 1);
        float _Roll = Input.GetAxis("Roll");
        float _Pitch = Input.GetAxis("Pitch");
        if (_Pitch < 0)
        {
            CurrentElevatorsAngle -= ElevatorsRotationSpeed * Time.deltaTime;
            CurrentElevatorsAngle = Mathf.Clamp(CurrentElevatorsAngle, -_Pitch * _MinElevatorsAngle, _MaxElevatorsAngle);
        }
        else if (_Pitch > 0)
        {
            CurrentElevatorsAngle += ElevatorsRotationSpeed * Time.deltaTime;
            CurrentElevatorsAngle = Mathf.Clamp(CurrentElevatorsAngle, _MinElevatorsAngle, _Pitch * _MaxElevatorsAngle);
        }
        if (_Roll > 0)
        {
            CurrentRightAileronAngle -= AileronsRotationSpeed * Time.deltaTime;
            CurrentLeftAileronAngle += AileronsRotationSpeed * Time.deltaTime;
            CurrentRightAileronAngle = Mathf.Clamp(CurrentRightAileronAngle, _MinAileronAngle * _Roll, _MaxAileronAngle);
            CurrentLeftAileronAngle = Mathf.Clamp(CurrentLeftAileronAngle, _MinAileronAngle, _MaxAileronAngle * _Roll);
        }
        else if (_Roll < 0)
        {
            CurrentRightAileronAngle += AileronsRotationSpeed * Time.deltaTime;
            CurrentLeftAileronAngle -= AileronsRotationSpeed * Time.deltaTime;
            CurrentRightAileronAngle = Mathf.Clamp(CurrentRightAileronAngle, _MinAileronAngle, _MaxAileronAngle * -_Roll);
            CurrentLeftAileronAngle = Mathf.Clamp(CurrentLeftAileronAngle, _MinAileronAngle * -_Roll, _MaxAileronAngle);
        }
        if (Input.GetKey(KeyCode.A))
            CurrentRuddersAngle += RuddersRotationSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D))
            CurrentRuddersAngle -= RuddersRotationSpeed * Time.deltaTime;

        CurrentLeftAileronAngle = Mathf.Clamp(CurrentLeftAileronAngle, _MinAileronAngle, _MaxAileronAngle);
        CurrentRightAileronAngle = Mathf.Clamp(CurrentRightAileronAngle, _MinAileronAngle, _MaxAileronAngle);
        CurrentElevatorsAngle = Mathf.Clamp(CurrentElevatorsAngle, _MinElevatorsAngle, _MaxElevatorsAngle);
        CurrentRuddersAngle = Mathf.Clamp(CurrentRuddersAngle, _MinRuddersAngle, _MaxRuddersAngle);
        CurrentRuddersAngle = Mathf.Clamp(CurrentRuddersAngle, _MinRuddersAngle, _MaxRuddersAngle);

        if (!Input.GetKey(KeyCode.E) & !Input.GetKey(KeyCode.Q) & (CurrentLeftAileronAngle != AileronsNeutralAngle | CurrentRightAileronAngle != AileronsNeutralAngle) & _Roll == 0)
            SetAileronsAngleToNeutral();
        if (!Input.GetKey(KeyCode.W) & !Input.GetKey(KeyCode.S) & CurrentElevatorsAngle != ElevatorsNeutralAngle & _Pitch == 0)
            SetElevatorsAngleToNeutral();
        if (!Input.GetKey(KeyCode.A) & !Input.GetKey(KeyCode.D) & CurrentRuddersAngle != 0)
            SetRuddersAngleToNeutral();

        if (CurrentLeftAileronAngle != LeftAileron.localEulerAngles.y | CurrentRightAileronAngle != RightAileron.localEulerAngles.y)
            SetAileronsAngle();
        if (CurrentElevatorsAngle != LeftElevator.localEulerAngles.y)
            SetElevatorsAngle();
        if (CurrentRuddersAngle != LeftRudder.localEulerAngles.z)
            SetRuddersAnlge();
        if (TrueThrottle != CommandThrottle)
            SetTrueThrottle();
        if (ElevatorsNeutralAngle != Trimmer * ElevatorsMaxTrimmerAngle)
            ElevatorsNeutralAngle = Trimmer * ElevatorsMaxTrimmerAngle;

        if (TrueThrottle > 0.8 & (!LeftEngineAfterburner.isPlaying | !RightEngineAfterburner.isPlaying))
        {
            LeftEngineAfterburner.Play();
            RightEngineAfterburner.Play();
        }
        else if (TrueThrottle < 0.8 & (LeftEngineAfterburner.isPlaying | RightEngineAfterburner.isPlaying))
        {
            LeftEngineAfterburner.Stop();
            RightEngineAfterburner.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (IPausable.IsGamePaused)
            return;
        PlaneRigidbody.AddRelativeForce(0, 0, TrueThrottle * MaxThrust * PlanePhysics.AtmosphericPressure);

        float _TrueAirSpeedSpeedSquared = Mathf.Pow(PlanePhysics.TrueAirSpeed, 2);

        PlaneRigidbody.AddRelativeTorque(PitchRotationCoefficient * _TrueAirSpeedSpeedSquared * PlanePhysics.AtmosphericPressure * (CurrentElevatorsAngle / 30), 
            YawRotationCoefficient * PlanePhysics.TrueAirSpeed * PlanePhysics.AtmosphericPressure * -(CurrentRuddersAngle / 30), 
            RollRotationCoefficient * _TrueAirSpeedSpeedSquared * PlanePhysics.AtmosphericPressure * (Mathf.Clamp(CurrentRightAileronAngle, -20, 20) / 20));
    }

    private void SetElevatorsAngle()
    {
        LeftElevator.transform.Rotate(0, CurrentElevatorsAngle - LeftElevator.localEulerAngles.y, 0);
        RightElevator.transform.Rotate(0, CurrentElevatorsAngle - RightElevator.localEulerAngles.y, 0);
    }

    private void SetElevatorsAngleToNeutral()
    {
        if (CurrentElevatorsAngle < ElevatorsNeutralAngle)
        {
            CurrentElevatorsAngle += ElevatorsRotationSpeed * Time.deltaTime;
            if (CurrentElevatorsAngle > ElevatorsNeutralAngle)
                CurrentElevatorsAngle = ElevatorsNeutralAngle;
        }
        else if (CurrentElevatorsAngle > ElevatorsNeutralAngle)
        {
            CurrentElevatorsAngle -= ElevatorsRotationSpeed * Time.deltaTime;
            if (CurrentElevatorsAngle < ElevatorsNeutralAngle)
                CurrentElevatorsAngle = ElevatorsNeutralAngle;
        }
    }

    private void SetAileronsAngle()
    {
        LeftAileron.Rotate(0, CurrentLeftAileronAngle - LeftAileron.localEulerAngles.y, 0);
        RightAileron.Rotate(0, CurrentRightAileronAngle - RightAileron.localEulerAngles.y, 0);
    }

    private void SetAileronsAngleToNeutral()
    {
        if (CurrentLeftAileronAngle < AileronsNeutralAngle)
        {
            CurrentLeftAileronAngle += AileronsRotationSpeed * Time.deltaTime;
            if (CurrentLeftAileronAngle > AileronsNeutralAngle)
                CurrentLeftAileronAngle = AileronsNeutralAngle;
        }
        else if (CurrentLeftAileronAngle > AileronsNeutralAngle)
        {
            CurrentLeftAileronAngle -= AileronsRotationSpeed * Time.deltaTime;
            if (CurrentLeftAileronAngle < AileronsNeutralAngle)
                CurrentLeftAileronAngle = AileronsNeutralAngle;
        }

        if (CurrentRightAileronAngle < AileronsNeutralAngle)
        {
            CurrentRightAileronAngle += AileronsRotationSpeed * Time.deltaTime;
            if (CurrentRightAileronAngle > AileronsNeutralAngle)
                CurrentRightAileronAngle = AileronsNeutralAngle;
        }
        else if (CurrentRightAileronAngle > AileronsNeutralAngle)
        {
            CurrentRightAileronAngle -= AileronsRotationSpeed * Time.deltaTime;
            if (CurrentRightAileronAngle < AileronsNeutralAngle)
                CurrentRightAileronAngle = AileronsNeutralAngle;
        }
    }

    private void SetRuddersAnlge()
    {
        LeftRudder.Rotate(0, 0, CurrentRuddersAngle - LeftRudder.localEulerAngles.z);
        RightRudder.Rotate(0, 0, CurrentRuddersAngle - RightRudder.localEulerAngles.z);
    }

    private void SetRuddersAngleToNeutral() 
    {
        if (CurrentRuddersAngle < 0)
        {
            CurrentRuddersAngle += RuddersRotationSpeed * Time.deltaTime;
            if (CurrentRuddersAngle > 0)
                CurrentRuddersAngle = 0;
        }
        else if (CurrentRuddersAngle > 0)
        {
            CurrentRuddersAngle -= RuddersRotationSpeed * Time.deltaTime;
            if (CurrentRuddersAngle < 0)
                CurrentRuddersAngle = 0;
        }
    }

    private void SetTrueThrottle()
    {
        if (TrueThrottle < CommandThrottle)
        {
            TrueThrottle += 0.1f * Time.deltaTime;
            if (TrueThrottle > CommandThrottle)
                TrueThrottle = CommandThrottle;
        }
        else if (TrueThrottle > CommandThrottle)
        {
            TrueThrottle -= 0.1f * Time.deltaTime;
            if (TrueThrottle < CommandThrottle)
                TrueThrottle = CommandThrottle;
        }
    }

    public void Pause()
    {
        LeftEngineAfterburner.Pause();
        RightEngineAfterburner.Pause();
    }

    public void Unpause()
    {
        LeftEngineAfterburner.Play();
        RightEngineAfterburner.Play();
    }
}
