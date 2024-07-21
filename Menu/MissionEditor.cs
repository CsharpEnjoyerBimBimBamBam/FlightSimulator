using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MissionEditor : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown PlanesDropdown;
    [SerializeField] private TMP_Dropdown ArmSuspensionDropdown;
    [SerializeField] private UnityEngine.UI.Image PlaneImage;
    [SerializeField] private GameObject Menu;
    [SerializeField] private GameObject MissionEditorMainMenu;
    [SerializeField] private GameObject PlaneConfigurationMenu;
    [SerializeField] private Button PlaneConfigurationButton;
    [SerializeField] private GameObject FuelLoadMenu;
    [SerializeField] private TMP_Text SuspensionLabel;
    [SerializeField] private UnityEngine.UI.Image MapImage;
    [SerializeField] private Scrollbar TotalFuelCountScrollbar;
    [SerializeField] private Scrollbar FuelTankScrollbar;
    [SerializeField] private TMP_Text FuelCountLabel;
    [SerializeField] private TMP_Dropdown UnitsDropdown;
    [SerializeField] private TMP_Text TotalWeightLabel;
    public AirplaneParameters SelectedPlaneParameters
    {
        get
        {
            _SelectedPlaneParameters.SetEquipments(_SelectedEquipmentNames);

            foreach(var _Element in _ScrollbarFuelTanks)
            {
                for (int i = 0; i < _SelectedPlaneParameters.InternalFuelTanks.Count; i++)
                {
                    if (_SelectedPlaneParameters.InternalFuelTanks[i].Name == _Element.Value.Name)
                    {
                        float _FuelMass = _Element.Value.MaxWeight * _Element.Key.value;
                        if (_CurrentUnits == "Lb")
                            _FuelMass *= Constants.LbToKg;
                        _SelectedPlaneParameters.InternalFuelTanks[i].CurrentWeight = _FuelMass;
                        break;
                    }
                }
            }
            return _SelectedPlaneParameters;
        }
    }

    private static List<TMP_Dropdown> _ArmamentDropdowns = new List<TMP_Dropdown>();
    private List<TMP_Text> _ArmamentLabels = new List<TMP_Text>();
    private List<UnityEngine.Object> _FuelLoadMenuElements = new List<UnityEngine.Object>();
    private List<AirplaneEquipment> _SelectedEquipment = new List<AirplaneEquipment>();
    private List<string> _SelectedEquipmentNames;
    private AirplaneParameters _SelectedPlaneParameters;
    private string _CurrentUnits { get { return UnitsDropdown.options[UnitsDropdown.value].text; } }
    private Dictionary<string, string> _PlanesNames = new Dictionary<string, string>()
    {
        {"F/A 18C", "FA18C"},
        {"A10C", "A10C"}
    };
    private Dictionary<Scrollbar, FuelTank> _ScrollbarFuelTanks = new Dictionary<Scrollbar, FuelTank>();
    private Dictionary<Scrollbar, TMP_Text> _ScrollbarLabels = new Dictionary<Scrollbar, TMP_Text>();

    private void Start()
    {
        PlaneConfigurationButton.onClick.AddListener(ChangePlane);
        PlaneConfigurationButton.onClick.AddListener(() => ChangeTotalAirplaneWeight());

        ChangePlane();
        CreateAirplaneFuelLoadElements(); 
    }

    public void ChangePlane()
    {
        string _PlaneName = _PlanesNames[PlanesDropdown.options[PlanesDropdown.value].text];
        _SelectedPlaneParameters = Airplane.LoadParameters(_PlaneName);
        PlaneImage.sprite = Resources.Load<Sprite>(ResourcesPath.SpritesPlanes + _SelectedPlaneParameters.Name);
        CreateArmamentDropdowns(_SelectedPlaneParameters.WeaponStations.ToList());
    }

    public void StartGame()
    {
        if (_SelectedEquipment != null)
        {
            AirportSettings Settings = GetComponent<AirportSettings>();
            SceneLoader.Parameters = SelectedPlaneParameters;
            SceneLoader.SelectedAirport = Settings.SelectedAirport;
            SceneLoader.SelectedRunway = Settings.SelectedRunway;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    public void DeleteArmamentDropdowns()
    {
        for (int i = 0; i < _ArmamentDropdowns.Count; i++)
        {
            Destroy(_ArmamentDropdowns[i].gameObject);
            _ArmamentDropdowns[i].onValueChanged.RemoveAllListeners();
            Destroy(_ArmamentLabels[i].gameObject);
        }
        _ArmamentDropdowns.Clear();
        _ArmamentLabels.Clear();
    }

    public void SaveSelectedArmament(int e)
    {
        _SelectedEquipmentNames = GetSelectedArmamentNames();
        _SelectedEquipment = GetSelectedArmament();
    }

    public void DeleteSelectedArmament()
    {
        _SelectedEquipmentNames = null;
        _SelectedEquipment = null;
    }    

    public void DeleteAirplaneFuelLoadElements()
    {
        foreach(UnityEngine.Object _Element in _FuelLoadMenuElements)
        {
            Destroy(_Element);
        }
        _FuelLoadMenuElements.Clear();
        _ScrollbarFuelTanks.Clear();
        _ScrollbarLabels.Clear();
    }

    public void UpdateTotalFuelCount(float e)
    {
        float _CurrentFuelMass = 0;
        foreach(var _Element in _ScrollbarFuelTanks)
        {
            _Element.Key.value = TotalFuelCountScrollbar.value;
            _CurrentFuelMass += _Element.Value.MaxWeight * TotalFuelCountScrollbar.value;
        }
        if (_CurrentUnits == "Lb")
            FuelCountLabel.text = ((int)(_CurrentFuelMass * Constants.KgToLb)).ToString();
        else
            FuelCountLabel.text = ((int)_CurrentFuelMass).ToString();
        ChangeTotalAirplaneWeight();
    }

    public void UpdateFuelCount(float e)
    {
        float _CurrentFuelMass = 0;
        float _MaxFuelMass = 0;
        foreach (var _Element in _ScrollbarFuelTanks)
        {
            _CurrentFuelMass += _Element.Value.MaxWeight * _Element.Key.value;
            _MaxFuelMass += _Element.Value.MaxWeight;
        }
        if (_CurrentUnits == "Lb")
            FuelCountLabel.text = ((int)(_CurrentFuelMass * Constants.KgToLb)).ToString();
        else
            FuelCountLabel.text = ((int)_CurrentFuelMass).ToString();
        //TotalFuelCountScrollbar.value = _CurrentFuelMass / _MaxFuelMass;
        ChangeTotalAirplaneWeight();
    }

    public void ChangeTotalAirplaneWeight()
    {
        float _FuelMass = float.TryParse(FuelCountLabel.text, out float FuelCount) ? FuelCount : 0;
        float _ArmamentMass = 0;
        if (_SelectedEquipment != null)
        {
            foreach (var _Armament in _SelectedEquipment)
            {
                if (_Armament == null)
                    continue;

                _ArmamentMass += _Armament.Mass;
            }
        }
        float _TotalWeight = _SelectedPlaneParameters.EmptyWeight + _FuelMass + _ArmamentMass;
        float _MaxWeight = _SelectedPlaneParameters.MaxWeight;
        if (_CurrentUnits == "Lb")
        {
            _TotalWeight = (_SelectedPlaneParameters.EmptyWeight * Constants.KgToLb) + _FuelMass + (_ArmamentMass * Constants.KgToLb);
            _MaxWeight *= Constants.KgToLb;
        }           
        TotalWeightLabel.text = _TotalWeight.ToString();
        if (_TotalWeight > _MaxWeight)
            TotalWeightLabel.faceColor = Color.red;
        else
            TotalWeightLabel.faceColor = Color.black;
    }

    private void CreateArmamentDropdowns(List<WeaponStation> _WeaponStations)
    {
        RectTransform rectTransform = (RectTransform)ArmSuspensionDropdown.transform;
        float _X = PlaneImage.transform.localPosition.x + (rectTransform.rect.width * ((float)_WeaponStations.Count / 2 - 1));
        float _Y = PlaneImage.transform.localPosition.y - PlaneImage.rectTransform.rect.height - 100;

        DeleteArmamentDropdowns();

        for (int i = 0; i < _WeaponStations.Count; i++)
        {
            TMP_Dropdown _Dropdown = Instantiate(ArmSuspensionDropdown, PlaneConfigurationMenu.transform);
            TMP_Text _Label = Instantiate(SuspensionLabel, PlaneConfigurationMenu.transform);
            _Dropdown.ClearOptions();
            List<string> PossibleEquipment = new List<string> { AirplaneEquipment.None };
            PossibleEquipment.AddRange(_WeaponStations[i].PossibleEquipment);
            _Dropdown.AddOptions(PossibleEquipment);
            _Dropdown.transform.localPosition = new Vector3(_X, _Y, 0);
            _Label.text = _WeaponStations[i].Number.ToString();
            _Label.transform.localPosition = new Vector3(_X, _Y + 40);
            _ArmamentDropdowns.Add(_Dropdown);
            _ArmamentLabels.Add(_Label);
            if (_SelectedEquipmentNames != null)
            {
                IReadOnlyList<string> _PossibleArmament = _WeaponStations[i].PossibleEquipment;
                for (int j = 0; j < _PossibleArmament.Count; j ++)
                {
                    if (_PossibleArmament[j] == _SelectedEquipmentNames[i])
                    {
                        _Dropdown.value = j;
                    }
                }
            }
            _Dropdown.onValueChanged.AddListener(SaveSelectedArmament);
            _Dropdown.onValueChanged.AddListener((e) => ChangeTotalAirplaneWeight());
            _X -= 150;
        }
    }

    public void CreateAirplaneFuelLoadElements()
    {
        float _Y = 450;
        for (int i = 0; i < _SelectedPlaneParameters.InternalFuelTanks.Count; i++)
        {
            Scrollbar _Scrollbar = Instantiate(FuelTankScrollbar, FuelLoadMenu.transform);
            TMP_Text _FuelCountLabel = Instantiate(FuelCountLabel, FuelLoadMenu.transform);
            TMP_Text _TankNameLabel = Instantiate(FuelCountLabel, FuelLoadMenu.transform);
            _FuelCountLabel.fontSize = 30;
            _TankNameLabel.fontSize = 20;
            _FuelCountLabel.text = "0";
            RectTransform _TankNameRectTransform = (RectTransform)_TankNameLabel.transform;
            RectTransform _FuelCountRectTransform = (RectTransform)_FuelCountLabel.transform;
            RectTransform _ScrollbarRectTransform = (RectTransform)_FuelCountLabel.transform;
            _TankNameLabel.text = _SelectedPlaneParameters.InternalFuelTanks[i].Name;
            _TankNameLabel.transform.localPosition = new Vector3(0, _Y);
            _FuelCountLabel.transform.localPosition = new Vector3(0, _Y - _TankNameRectTransform.rect.height - 5);
            _Scrollbar.transform.localPosition = new Vector3(0, _FuelCountLabel.transform.localPosition.y - _FuelCountRectTransform.rect.height - 10);
            _ScrollbarFuelTanks[_Scrollbar] = _SelectedPlaneParameters.InternalFuelTanks[i];
            _ScrollbarLabels[_Scrollbar] = _FuelCountLabel;
            _FuelLoadMenuElements.Add(_Scrollbar);
            _FuelLoadMenuElements.Add(_FuelCountLabel);
            _FuelLoadMenuElements.Add(_TankNameLabel);
            _Scrollbar.onValueChanged.AddListener(UpdateFuelLoadMenu);
            _Scrollbar.onValueChanged.AddListener(UpdateFuelCount);
            _Scrollbar.onValueChanged.Invoke(0);
            _Y -= _TankNameRectTransform.rect.height + _FuelCountRectTransform.rect.height + _ScrollbarRectTransform.rect.height + 50;
        }
    }

    public void UpdateFuelLoadMenu(float _ScrollbarValue)
    {
        foreach(var _Element in _ScrollbarFuelTanks)
        {
            Scrollbar _CurrentScrollbar = _Element.Key;
            TMP_Text _CurrentFuelCountLabel = _ScrollbarLabels[_CurrentScrollbar];
            FuelTank _CurrentFuelTank = _ScrollbarFuelTanks[_CurrentScrollbar];
            float _FuelCount = _CurrentFuelTank.MaxWeight * _CurrentScrollbar.value;

            if (_CurrentUnits == "Lb")
                _FuelCount *= Constants.KgToLb;
            _CurrentFuelCountLabel.text = _FuelCount.ToString() + " " + _CurrentUnits;
        }
    }

    private List<AirplaneEquipment> GetSelectedArmament()
    {
        List<AirplaneEquipment> _Equipments = new List<AirplaneEquipment>();
        for (int i = 0; i < _ArmamentDropdowns.Count; i++)
        {
            string _CurrentArmamentName = _ArmamentDropdowns[i].options[_ArmamentDropdowns[i].value].text;

            if (_CurrentArmamentName == AirplaneEquipment.None)
            {
                _Equipments.Add(null);
                continue;
            }

            _Equipments.Add(AirplaneEquipment.Load(_CurrentArmamentName));
        }
        return _Equipments;
    }

    private List<string> GetSelectedArmamentNames()
    {
        List<string> _SelectedArmamentNames = new List<string>();
        for (int i = 0; i < _ArmamentDropdowns.Count; i++)
        {
            string _CurrentArmament = _ArmamentDropdowns[i].options[_ArmamentDropdowns[i].value].text; 
            _SelectedArmamentNames.Add(_CurrentArmament);
        }
        return _SelectedArmamentNames;
    }
}
