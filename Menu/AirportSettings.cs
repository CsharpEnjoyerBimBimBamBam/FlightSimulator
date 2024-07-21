using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AirportSettings : MonoBehaviour
{
    public IReadOnlyAirport SelectedAirport { get; private set; }
    public IReadOnlyRunway SelectedRunway { get; private set; }
    [SerializeField] private GameObject _Main;
    [SerializeField] private GameObject _Settings;
    [SerializeField] private TMP_Dropdown _AirportDropdown;
    [SerializeField] private TMP_InputField _AirportInputField;
    [SerializeField] private Button _AirportInfoButton;
    [SerializeField] private TMP_Text _NameLabel;
    [SerializeField] private TMP_Text _CoordinatesLabel;
    [SerializeField] private TMP_Text _ICAOLabel;
    [SerializeField] private TMP_Text _IATALabel;
    [SerializeField] private TMP_Text _ElevationLabel;
    [SerializeField] private TMP_Text _TransitionAltLabel;
    [SerializeField] private TMP_Dropdown _RunwaysDropdown;
    [SerializeField] private TMP_Text _LengthLabel;
    [SerializeField] private TMP_Text _CourseAltLabel;
    [SerializeField] private TMP_Text _RunwayElevationLabel;
    [SerializeField] private Button _SaveButton;
    private List<IReadOnlyAirport> _Airports;
    private TMP_Text _ButtonText;
    private DistanceUnits _Units;

    void Start()
    {
        _ButtonText = _AirportInfoButton.GetComponentInChildren<TMP_Text>();
        _Airports = Airac.GetInstance().Airports.ToList();
        _Units = Airac.GetInstance().Units;
        SelectedAirport = _Airports[0];
        SelectedRunway = SelectedAirport.Runways[0];
        _AirportInfoButton.interactable = false;
        _AirportInfoButton.onClick.AddListener(() =>
        {
            if (_ButtonText.text == null)
                return;
        
            IReadOnlyAirport Airport = _Airports.Find(X => X.Name == _ButtonText.text);
        
            if (Airport == null)
                return;
        
            CreateAirportInfo(Airport);
        });

        _SaveButton.onClick.AddListener(() =>
        {
            _Settings.SetActive(false);
            SetMainSelectablesInteractable(true);
        });

        _RunwaysDropdown.onValueChanged.AddListener((e) =>
        {
            string RunwayName = _RunwaysDropdown.options[e].text;
            IReadOnlyRunway Runway = SelectedAirport.Runways.FirstOrDefault(X => X.Name == RunwayName);
            if (Runway == null)
                return;
            SelectedRunway = Runway;
            _LengthLabel.text = $"{Runway.Length} {_Units}";
            _CourseAltLabel.text = $"{Runway.Heading}°";
            _RunwayElevationLabel.text = $"{Runway.Elevation} {_Units}";
        });

        _AirportInputField.onFocusSelectAll = false;

        UnityAction<string> OnInputFieldValueChanged = (e) =>
        {
            string Text = _AirportInputField.text.ToUpper();
            List<TMP_Dropdown.OptionData> Airports = _Airports.FindAll(X => X.ICAO.StartsWith(Text)).
                Select(X => new TMP_Dropdown.OptionData(X.ICAO)).
                Take(20).
                ToList();
            _AirportDropdown.options = Airports;
            _AirportDropdown.Hide();
            _AirportDropdown.Show();
            _AirportInputField.ActivateInputField();
        };

        _AirportInputField.onSelect.AddListener((e) => { _AirportDropdown.Show(); OnInputFieldValueChanged.Invoke(e); });

        _AirportInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

        _AirportDropdown.onValueChanged.AddListener((e) =>
        {
            _AirportInputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            string ICAO = _AirportDropdown.options[e].text;
            IReadOnlyAirport Airport = _Airports.FirstOrDefault(X => X.ICAO == ICAO);
            SelectedAirport = Airport;
            _AirportInfoButton.interactable = true;
            _ButtonText.text = Airport.Name;
            _AirportInputField.text = ICAO;
            _AirportInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        });
    }

    void Update()
    {
        
    }

    private void CreateAirportInfo(IReadOnlyAirport Airport)
    {
        if (Airport == null)
            return;

        SetMainSelectablesInteractable(false);
        _NameLabel.text = Airport.Name;
        _CoordinatesLabel.text = $"{Airport.Coordinates.Latitude.ToDMS()} {Airport.Coordinates.Longitude.ToDMS()}";
        _ICAOLabel.text = Airport.ICAO;
        _IATALabel.text = Airport.IATA;
        _ElevationLabel.text = $"{Airport.Elevation} {_Units}";
        _TransitionAltLabel.text = $"{Airport.TransitionAltitude} {_Units}";
        _RunwaysDropdown.options = Airport.Runways.Select(X => new TMP_Dropdown.OptionData(X.Name)).ToList();
        _RunwaysDropdown.onValueChanged.Invoke(0);
        _Settings.SetActive(true);
    }

    private void SetMainSelectablesInteractable(bool Interactable)
    {
        foreach (Selectable Current in _Main.GetComponentsInChildren<Selectable>())
            Current.interactable = Interactable;
    }
}
