using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.IO;

public class MissionMapController : MonoBehaviour
{
    [SerializeField] private GameObject _MissionEditorMainMenu;
    [SerializeField] private Image _MapImage;
    [SerializeField] private TMP_Text _CoordinatesLabel;
    [SerializeField] private Toggle _AirportToggle;
    [SerializeField] private Sprite _NonScaledMapSprite;
    private float _ZoomSpeed = 35;
    private Rect _MapArea;
    private float _MouseHorizontal;
    private float _MouseVertical;
    private float _MinMapAreaWidth = 5;
    private float _MinMapAreaHeight = 5;
    private PointerEventData _EventData = new PointerEventData(EventSystem.current);
    private List<RaycastResult> _RayCastResults = new List<RaycastResult>();
    private List<Toggle> _AirportToggles = new List<Toggle>();
    private Dictionary<Toggle, Airport> _ReferenceTogglesCoordinates = new Dictionary<Toggle, Airport>();
    private Toggle _SelectedToggle;

    private void Start()
    {
        _MapArea = new Rect(0, 0, _NonScaledMapSprite.texture.width, _NonScaledMapSprite.texture.height);
        _EventData = new PointerEventData(EventSystem.current);
    }

    private void Update()
    {
        _MouseHorizontal = Input.GetAxis("Mouse X");
        _MouseVertical = Input.GetAxis("Mouse Y");

        if (!CheckIfMouseOnMap())
            return;

        UpdateCoordinates();

        if (!CheckForInteractionWithMap())
            return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float _TempX = _MapArea.x;
            float _TempY = _MapArea.y;
            float _CurrentMapScaleX = Input.GetAxis("Mouse ScrollWheel") * _ZoomSpeed * Time.deltaTime * _NonScaledMapSprite.texture.width;
            float _CurrentMapScaleY = Input.GetAxis("Mouse ScrollWheel") * _ZoomSpeed * Time.deltaTime * _NonScaledMapSprite.texture.height;
            _MapArea.x += _CurrentMapScaleX;
            _MapArea.y += _CurrentMapScaleY;
            _MapArea.width -= _CurrentMapScaleX * 2;
            _MapArea.height -= _CurrentMapScaleY * 2;
            _MapArea.x *= (_MapArea.width - _MinMapAreaWidth) / 
                Mathf.Clamp(_MapArea.width - _MinMapAreaWidth, 0.0000001f, _MapArea.width - _MinMapAreaWidth);
            _MapArea.y *= (_MapArea.height - _MinMapAreaHeight) /
                Mathf.Clamp(_MapArea.height - _MinMapAreaHeight, 0.0000001f, _MapArea.height - _MinMapAreaHeight);
            _MapArea.x = Mathf.Clamp(_MapArea.x, 0, _NonScaledMapSprite.texture.width - _MapArea.width);
            _MapArea.y = Mathf.Clamp(_MapArea.y, 0, _NonScaledMapSprite.texture.height - _MapArea.height);
            _MapArea.width = Mathf.Clamp(_MapArea.width, _MinMapAreaWidth, _NonScaledMapSprite.texture.width - _MapArea.x);
            _MapArea.height = Mathf.Clamp(_MapArea.height, _MinMapAreaHeight, _NonScaledMapSprite.texture.height - _MapArea.y);

            foreach (var _Toggle in _AirportToggles)
            {
                Vector2 _TogglePosition = _Toggle.transform.localPosition;
                _TogglePosition.x += (_MapArea.x - _TempX) * (_NonScaledMapSprite.texture.width / _MapArea.width) / _NonScaledMapSprite.texture.width * 
                    _MapImage.rectTransform.rect.width * (_Toggle.transform.localPosition.x / (_MapImage.rectTransform.rect.width / 2));            
                _TogglePosition.y += (_MapArea.y - _TempY) * (_NonScaledMapSprite.texture.height / _MapArea.height) / _NonScaledMapSprite.texture.height * 
                    _MapImage.rectTransform.rect.height * (_Toggle.transform.localPosition.y / (_MapImage.rectTransform.rect.height / 2));
                _Toggle.transform.localPosition = _TogglePosition;
            }
        }
        if (Input.GetMouseButton(0) && (_MouseHorizontal != 0 || _MouseVertical != 0))
        {
            float _TempX = _MapArea.x;
            float _TempY = _MapArea.y;
            _MapArea.y += -_MouseVertical * _NonScaledMapSprite.texture.height * Time.deltaTime;
            _MapArea.x += -_MouseHorizontal * _NonScaledMapSprite.texture.width * Time.deltaTime;
            _MapArea.x = Mathf.Clamp(_MapArea.x, 0, _NonScaledMapSprite.texture.width - _MapArea.width);
            _MapArea.y = Mathf.Clamp(_MapArea.y, 0, _NonScaledMapSprite.texture.height - _MapArea.height);

            foreach (var _Toggle in _AirportToggles)
            {
                Vector2 _TogglePosition = _Toggle.transform.localPosition;
                _TogglePosition.x -= (_MapArea.x - _TempX) /_NonScaledMapSprite.texture.width * _MapImage.rectTransform.rect.width * 
                    (_NonScaledMapSprite.texture.width / _MapArea.width);
                _TogglePosition.y -= (_MapArea.y - _TempY) / _NonScaledMapSprite.texture.height * _MapImage.rectTransform.rect.height * 
                    (_NonScaledMapSprite.texture.height / _MapArea.height);
                _Toggle.transform.localPosition = _TogglePosition;
            }
        }
        Sprite _RescaledMapSprite = Sprite.Create(_NonScaledMapSprite.texture, _MapArea, new Vector2(0.5f, 0.5f), 100);
        _MapImage.sprite = _RescaledMapSprite;
        UpdateToggles();
    }

    private bool CheckForInteractionWithMap()
    {
        if ((Input.GetAxis("Mouse ScrollWheel") != 0) || (Input.GetMouseButton(0) & (_MouseHorizontal != 0 || _MouseVertical != 0)))
            return true;
        return false;
    }

    private bool CheckIfMouseOnMap()
    {
        _EventData.position = Input.mousePosition;
        _RayCastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_EventData, _RayCastResults);

        foreach (var _RayCastResult in _RayCastResults)
        {
            if (_RayCastResult.gameObject == _MapImage.gameObject)
                return true;
        }
        return false;
    }

    private void UpdateCoordinates()
    {
        foreach (var _RayCastResult in _RayCastResults)
        {
            if (_RayCastResult.gameObject == _MapImage.gameObject)
            {
                Vector2 _MouseScreenPosition = _RayCastResult.screenPosition;
                Vector3 _MouseLocalCoordinates = _MapImage.transform.InverseTransformPoint(_MouseScreenPosition);
                _MouseLocalCoordinates.x += _MapImage.rectTransform.rect.width / 2;
                _MouseLocalCoordinates.y += _MapImage.rectTransform.rect.height / 2;
                Vector2 _NormalizedMousePosition = new Vector2(_MouseLocalCoordinates.x / _MapImage.rectTransform.rect.width, 
                    _MouseLocalCoordinates.y / _MapImage.rectTransform.rect.height);
                Vector2 _UnityCooridinates = new Vector2();
                //_UnityCooridinates.x = _MapArea.x / _NonScaledMapSprite.texture.width * _CurrentMap.Size.x;
                //_UnityCooridinates.y = _MapArea.y / _NonScaledMapSprite.texture.height * _CurrentMap.Size.y;
                //_UnityCooridinates.x += _NormalizedMousePosition.x * (_MapArea.width / _NonScaledMapSprite.texture.width) * _CurrentMap.Size.x;
                //_UnityCooridinates.y += _NormalizedMousePosition.y * (_MapArea.height / _NonScaledMapSprite.texture.height) * _CurrentMap.Size.y;
                //CoordinatesLabel.text = _UnityCooridinates.x.ToString() + "\n" + _UnityCooridinates.y.ToString();
                return;
            }
        }
    }

    private void UpdateToggles()
    {
        float _MapImageWidth = _MapImage.rectTransform.rect.width / 2;
        float _MapImageHeight = _MapImage.rectTransform.rect.height / 2;

        foreach (var _Toggle in _AirportToggles)
        {
            Vector2 _TogglePosition = _Toggle.transform.localPosition;

            if (_TogglePosition.x < -_MapImageWidth | _TogglePosition.x > _MapImageWidth | _TogglePosition.y < -_MapImageHeight | _TogglePosition.y > _MapImageHeight)
                _Toggle.gameObject.SetActive(false);
            else
                _Toggle.gameObject.SetActive(true);
        }
    }

    //private void CreateAirportsToggles()
    //{
    //    foreach(var _Airport in _CurrentMap.Airports)
    //    {
    //        Vector2 _NormalizedCoordinates = new Vector2();
    //        _NormalizedCoordinates.x = _Airport.Position.x / _CurrentMap.Size.x;
    //        _NormalizedCoordinates.y = _Airport.Position.y / _CurrentMap.Size.y;
    //        Vector2 _LocalMapCoordiantes = new Vector2();
    //        _LocalMapCoordiantes.x = (_NormalizedCoordinates.x * MapImage.rectTransform.rect.width) - MapImage.rectTransform.rect.width / 2;
    //        _LocalMapCoordiantes.y = (_NormalizedCoordinates.y * MapImage.rectTransform.rect.height) - MapImage.rectTransform.rect.height / 2;
    //        Toggle _CreatedToggle = Instantiate(AirportToggle, MapImage.transform);
    //        _CreatedToggle.transform.localPosition = _LocalMapCoordiantes;
    //        _CreatedToggle.onValueChanged.AddListener(delegate { ChangeSelectedToggleColor(_CreatedToggle);});
    //        _AirportToggles.Add(_CreatedToggle);
    //        _ReferenceTogglesCoordinates[_CreatedToggle] = _Airport;
    //    }
    //}

    private void ChangeSelectedToggleColor(Toggle _Toggle)
    {
        ColorBlock _ColorBlock = new ColorBlock();
        Color _Color;
        if (_Toggle.isOn)
            _Color = Color.red;
        else
            _Color = Color.black;
        _Color.a = 1;
        _ColorBlock.normalColor = _Color;
        _ColorBlock.selectedColor = _Color;
        _ColorBlock.disabledColor = _Color;
        _ColorBlock.pressedColor = _Color;
        _ColorBlock.highlightedColor = _Color;
        _ColorBlock.colorMultiplier = 1;
        _Toggle.colors = _ColorBlock;

        if (_SelectedToggle != null)
            _SelectedToggle.isOn = false;
        _SelectedToggle = _Toggle;
    }

    private void DeleteAirportsToggles()
    {
        foreach(var _Toggle in _AirportToggles)
        {
            Destroy(_Toggle);
        }
        _AirportToggles.Clear();
        _ReferenceTogglesCoordinates.Clear();
        _SelectedToggle = null;
    }
}