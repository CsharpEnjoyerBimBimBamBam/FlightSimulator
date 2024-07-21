using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using TMPro;
using System;
using UnityEditor.Experimental.GraphView;

public class HUDController : MonoBehaviour
{    
    [SerializeField] private GameObject HUD;
    [SerializeField] private Image VelocityVectorImage;
    [SerializeField] private Canvas Canvas;
    [SerializeField] private TMP_Text TrueAirSpeedLabel;
    [SerializeField] private TMP_Text GroundSpeedLabel;
    [SerializeField] private TMP_Text IndicatedAirSpeedLabel;
    [SerializeField] private TMP_Text VerticalSpeedLabel;
    [SerializeField] private TMP_Text MachNumberLabel;
    [SerializeField] private TMP_Text AngleOfAttackLabel;
    [SerializeField] private TMP_Text GForceLabel;
    [SerializeField] private Image PitchIndicatorImage;
    [SerializeField] private Image PitchIndicatorPlaneImage;
    [SerializeField] private TMP_Text AltitudeLabel;
    [SerializeField] private Sprite PitchIndicatorSprte;
    [SerializeField] private TMP_Text CoordinatesLabel;
    [SerializeField] private List<TMP_Text> SpeedLabels;
    [SerializeField] private Button SpeedLabelsButton;
    [SerializeField] private TMP_Text _CourseLabel;
    private Transform _CameraTarget;
    private Rigidbody _CameraTargetRigidbody;
    private FlyingObject _TargetPhysics;
    private int _AllPitchValuesCount = 37;
    private int _VisiblePitchValuesCount = 3;
    private float _RescaledSpriteHeight;
    private Vector3 _CameraTargetVelocity = Vector3.zero;
    private int _CurrentSpeedLabel = 0;

    private void Start()
    {
        UpdateCameraTarget();
        _RescaledSpriteHeight = PitchIndicatorSprte.rect.height / _AllPitchValuesCount * _VisiblePitchValuesCount;
        CameraMovement.OnTargetChanged += UpdateCameraTarget;
        SpeedLabelsButton.onClick.AddListener(ChangeSpeedLabel);
    }

    private void Update()
    {
        if (PauseSwithcer.IsGamePaused)
            return;

        GeoPosition _CameraTargetCoordinates = _TargetPhysics.Coordinates;

        AltitudeLabel.text = $"Alt: {_TargetPhysics.Altitude}";
        TrueAirSpeedLabel.text = $"TAS: {(int)(_TargetPhysics.TrueAirSpeed * Constants.MpsToKn)} Kn";
        GroundSpeedLabel.text = $"GS: {(int)(_TargetPhysics.GroundSpeed * Constants.MpsToKn)} Kn";
        IndicatedAirSpeedLabel.text = $"IAS: {(int)(_TargetPhysics.IndicatedAirSpeed * Constants.MpsToKn)} Kn";
        VerticalSpeedLabel.text = $"VS: {(int)(_TargetPhysics.VerticalSpeed * Constants.MpsToKn * 60)} Ft/M";
        MachNumberLabel.text = $"M: {Math.Round(_TargetPhysics.M, 2)}";
        AngleOfAttackLabel.text = $"AOA: {Math.Round(_TargetPhysics.PitchAngleOfAttack, 1)}";
        CoordinatesLabel.text = $"Lat: {_CameraTargetCoordinates.Latitude.ToDMS()}" +
            $"\nLng: {_CameraTargetCoordinates.Longitude.ToDMS()}";
        _CourseLabel.text = $"Hdg: {Mathf.FloorToInt(_TargetPhysics.Heading)}°";
        if (_TargetPhysics.PitchAngleOfAttack > 20)
            AngleOfAttackLabel.faceColor = Color.yellow;
        if (_TargetPhysics.PitchAngleOfAttack > 30)
            AngleOfAttackLabel.faceColor = Color.red;
        if (_TargetPhysics.PitchAngleOfAttack < 20)
            AngleOfAttackLabel.faceColor = Color.black;
        GForceLabel.text = $"G: {Math.Round(_TargetPhysics.GForce, 2)}";
        if (_TargetPhysics.GForce > 8)
            GForceLabel.faceColor = Color.yellow;
        if (_TargetPhysics.GForce > 10)
            GForceLabel.faceColor = Color.red;
        if (_TargetPhysics.GForce < 8)
            GForceLabel.faceColor = Color.black;
    }

    private void LateUpdate()
    {
        Vector3 _VelocityWorldPosition = _CameraTargetRigidbody.transform.position + (_CameraTargetVelocity * 1000000);
        Vector3 _VelocityScreenPosition = Camera.main.WorldToScreenPoint(_VelocityWorldPosition);
        if (_VelocityScreenPosition.z < 0 && VelocityVectorImage.enabled)
            VelocityVectorImage.enabled = false;
        else if (_VelocityScreenPosition.z >= 0 && !VelocityVectorImage.enabled)
            VelocityVectorImage.enabled = true;
        _VelocityScreenPosition.z = 0;
        _VelocityScreenPosition.x -= Screen.width / Canvas.scaleFactor / 2;
        _VelocityScreenPosition.y -= Screen.height / Canvas.scaleFactor / 2;        
        VelocityVectorImage.transform.localPosition = _VelocityScreenPosition;

        if (PauseSwithcer.IsGamePaused)
            return;

        _CameraTargetVelocity = _CameraTargetRigidbody.velocity;

        Rect _PitchIndicatorArea = new Rect(0, PitchIndicatorSprte.rect.height / 2, PitchIndicatorSprte.rect.width, _RescaledSpriteHeight);
        _PitchIndicatorArea.y += _TargetPhysics.Pitch / 90 * (PitchIndicatorSprte.rect.height / 2);
        _PitchIndicatorArea.y -= _RescaledSpriteHeight / 2;
        if (_PitchIndicatorArea.y < 0)
            _PitchIndicatorArea.y = 0;
        else if (_PitchIndicatorArea.y > PitchIndicatorSprte.rect.height - _PitchIndicatorArea.height)
            _PitchIndicatorArea.y = PitchIndicatorSprte.rect.height - _PitchIndicatorArea.height;
        Sprite _RescaledSprite = Sprite.Create(PitchIndicatorSprte.texture, _PitchIndicatorArea, new Vector2(0.5f, 0.5f), 100);
        PitchIndicatorImage.sprite = _RescaledSprite;
        PitchIndicatorPlaneImage.transform.localEulerAngles = new Vector3(0, 0, _TargetPhysics.Roll);
    }

    private void UpdateCameraTarget()
    {
        _CameraTarget = CameraMovement.target;

        if (_CameraTarget == null)
            return;

        _CameraTarget.TryGetComponent(out _TargetPhysics);

        if (_TargetPhysics == null)
            return;

        _CameraTargetRigidbody = _TargetPhysics.Rigidbody;
    }

    private void ChangeSpeedLabel()
    {
        _CurrentSpeedLabel++;
        if (_CurrentSpeedLabel > SpeedLabels.Count-1)
            _CurrentSpeedLabel = 0;
        for (int i = 0; i < SpeedLabels.Count; i++)
        {
            SpeedLabels[i].gameObject.SetActive(false);
            if (i == _CurrentSpeedLabel)
                SpeedLabels[i].gameObject.SetActive(true);
        }
    }
}
