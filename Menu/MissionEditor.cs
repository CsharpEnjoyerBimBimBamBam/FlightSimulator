using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MissionEditor : MonoBehaviour
{
    [SerializeField] public TMP_Dropdown PlanesDropdown;
    [SerializeField] public TMP_Dropdown ArmSuspensionDropdown;
    [SerializeField] public UnityEngine.UI.Image PlaneImage;
    [SerializeField] public GameObject Menu;
    [SerializeField] public GameObject MissionEditorMainMenu;
    [SerializeField] public GameObject PlaneConfigurationMenu;
    [SerializeField] public GameObject FuelLoadMenu;
    [SerializeField] public TMP_Text SuspensionLabel;
    [SerializeField] public UnityEngine.UI.Image MapImage;
    [SerializeField] public Scrollbar TotalFuelCountScrollbar;
    [SerializeField] public Scrollbar FuelTankScrollbar;
    [SerializeField] public TMP_Text FuelCountLabel;
    [SerializeField] public TMP_Dropdown UnitsDropdown;
    [SerializeField] public TMP_Text TotalWeightLabel;
    public Airplane SelectedPlane
    {
        get
        {
            _SelectedPlane.Armament = _SelectedArmament;
            foreach (var _Armament in _SelectedArmament)
            {
                _Armament.ParentAirplane = _SelectedPlane;
            }

            foreach(var _Element in _ScrollbarFuelTanks)
            {
                for (int i = 0; i < _SelectedPlane.InternalFuelTanks.Count; i++)
                {
                    if (_SelectedPlane.InternalFuelTanks[i].Name == _Element.Value.Name)
                    {
                        float _FuelMass = _Element.Value.MaxWeight * _Element.Key.value;
                        if (_CurrentUnits == "Lb")
                            _FuelMass *= Constants.LbToKg;
                        _SelectedPlane.InternalFuelTanks[i].CurrentWeight = _FuelMass;
                        break;
                    }
                }
            }
            return _SelectedPlane;
        }
    }

    private static List<TMP_Dropdown> _ArmamentDropdowns = new List<TMP_Dropdown>();
    private List<TMP_Text> _ArmamentLabels = new List<TMP_Text>();
    private List<UnityEngine.Object> _FuelLoadMenuElements = new List<UnityEngine.Object>();
    private List<AirplaneArmament> _SelectedArmament = new List<AirplaneArmament>();
    private List<ArmamentName> _SelectedArmamentNames;
    private Airplane _SelectedPlane;
    private string _CurrentUnits { get { return UnitsDropdown.options[UnitsDropdown.value].text; } }
    private Dictionary<string, PlaneName> _PlanesNames = new Dictionary<string, PlaneName>()
    {
        {"F/A 18C", PlaneName.FA18C},
        {"A10C", PlaneName.A10C}
    };
    private Dictionary<Scrollbar, FuelTank> _ScrollbarFuelTanks = new Dictionary<Scrollbar, FuelTank>();
    private Dictionary<Scrollbar, TMP_Text> _ScrollbarLabels = new Dictionary<Scrollbar, TMP_Text>();

    private void Start()
    {
        PlaneName _PlaneName = _PlanesNames[PlanesDropdown.options[PlanesDropdown.value].text];
        _SelectedPlane = Airplane.GetPlaneByName(_PlaneName);
        CreateAirplaneFuelLoadElements();
    }

    public void ChangePlane()
    {
        PlaneName _PlaneName = _PlanesNames[PlanesDropdown.options[PlanesDropdown.value].text];
        _SelectedPlane = Airplane.GetPlaneByName(_PlaneName);
        PlaneImage.sprite = Resources.Load<Sprite>("Sprites/Planes/" + _SelectedPlane.Name);
        CreateArmamentDropdowns(_SelectedPlane.WeaponStations);
    }

    public void StartGame()
    {
        if (_SelectedArmament != null)
        {
            SceneLoader.Airplane = SelectedPlane;
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
        _SelectedArmamentNames = GetSelectedArmamentNames();
        _SelectedArmament = GetSelectedArmament();
    }

    public void DeleteSelectedArmament()
    {
        _SelectedArmamentNames = null;
        _SelectedArmament = null;
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
        ChangeTotalAirplaneWeight(0);
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
        ChangeTotalAirplaneWeight(0);
    }

    public void ChangeTotalAirplaneWeight(int e)
    {
        float _FuelMass = float.Parse(FuelCountLabel.text);
        float _ArmamentMass = 0;
        if (_SelectedArmament != null)
        {
            foreach (var _Armament in _SelectedArmament)
            {
                _ArmamentMass += _Armament.Mass;
            }
        }
        float _TotalWeight = _SelectedPlane.EmptyWeight + _FuelMass + _ArmamentMass;
        float _MaxWeight = _SelectedPlane.MaxWeight;
        if (_CurrentUnits == "Lb")
        {
            _TotalWeight = (_SelectedPlane.EmptyWeight * Constants.KgToLb) + _FuelMass + (_ArmamentMass * Constants.KgToLb);
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
            _Dropdown.AddOptions(_WeaponStations[i].EnumToString());
            _Dropdown.transform.localPosition = new Vector3(_X, _Y, 0);
            _Label.text = _WeaponStations[i].Number.ToString();
            _Label.transform.localPosition = new Vector3(_X, _Y + 40);
            _ArmamentDropdowns.Add(_Dropdown);
            _ArmamentLabels.Add(_Label);
            if (_SelectedArmamentNames != null)
            {
                List<string> _PossibleArmament = _WeaponStations[i].EnumToString();
                for (int j = 0; j < _PossibleArmament.Count; j ++)
                {
                    if (_PossibleArmament[j] == _SelectedArmamentNames[i].ToString())
                    {
                        _Dropdown.value = j;
                    }
                }
            }
            _Dropdown.onValueChanged.AddListener(SaveSelectedArmament);
            _Dropdown.onValueChanged.AddListener(ChangeTotalAirplaneWeight);
            _X -= 150;
        }
    }

    public void CreateAirplaneFuelLoadElements()
    {
        float _Y = 450;
        for (int i = 0; i < _SelectedPlane.InternalFuelTanks.Count; i++)
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
            _TankNameLabel.text = _SelectedPlane.InternalFuelTanks[i].Name;
            _TankNameLabel.transform.localPosition = new Vector3(0, _Y);
            _FuelCountLabel.transform.localPosition = new Vector3(0, _Y - _TankNameRectTransform.rect.height - 5);
            _Scrollbar.transform.localPosition = new Vector3(0, _FuelCountLabel.transform.localPosition.y - _FuelCountRectTransform.rect.height - 10);
            _ScrollbarFuelTanks[_Scrollbar] = _SelectedPlane.InternalFuelTanks[i];
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

    private List<AirplaneArmament> GetSelectedArmament()
    {
        List<AirplaneArmament> _Armaments = new List<AirplaneArmament>();
        for (int i = 0; i < _ArmamentDropdowns.Count; i++)
        {
            string _CurrentArmamentName = _ArmamentDropdowns[i].options[_ArmamentDropdowns[i].value].text;
            if (_CurrentArmamentName != ArmamentName.None.ToString() & _CurrentArmamentName != ArmamentName.Pylon.ToString())
            {
                AirplaneArmament _CurrentArmament = AirplaneArmament.GetArmamentByName(AirplaneArmament.NameStringToEnum(_CurrentArmamentName));
                _CurrentArmament.WeaponStationNumber = i + 1;
                _Armaments.Add(_CurrentArmament);
            }
        }
        return _Armaments;
    }

    private List<ArmamentName> GetSelectedArmamentNames()
    {
        List<ArmamentName> _SelectedArmamentNames = new List<ArmamentName>();
        for (int i = 0; i < _ArmamentDropdowns.Count; i++)
        {
            ArmamentName _CurrentArmament = WeaponStation.StringToEnum(_ArmamentDropdowns[i].options[_ArmamentDropdowns[i].value].text); 
            _SelectedArmamentNames.Add(_CurrentArmament);
        }
        return _SelectedArmamentNames;
    }
}
