using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Airplane;

[Serializable]
[JsonConverter(typeof(EquipmentConverter))]
public class AirplaneEquipment
{
    public virtual float Mass { get; protected set; } = 227;
    public string Name { get; protected set; } = "Default";
    public EquipmentType EquipmentType { get; protected set; } = EquipmentType.UnguidedBomb;
    public float Diameter { get; protected set; } = 0.273f;
    public Vector3 LocalAttachmentPoint { get; protected set; }
    public Airplane? ParentAirplane => Station?.ParentAirplane;
    public WeaponStation? Station;
    public GameObject EquipmentGameObject { get; protected set; }
    public Rigidbody EquipmentRigidbody { get; protected set; }
    public static string None => "None";
    private static Dictionary<string, string> _Equipments = new Dictionary<string, string>();

    public static GameObject LoadPrefab(string _EquipmentName) => Resources.Load<GameObject>(ResourcesPath.Equipments + _EquipmentName);

    public GameObject LoadPrefab() => LoadPrefab(Name);

    public virtual GameObject InstantiateGameObject()
    {
        Debug.Log(Name);
        if (LoadPrefab() == null)
            return new DeafaultAirplaneEquipment().InstantiateGameObject();
        
        GameObject Equipment = MonoBehaviour.Instantiate(LoadPrefab());
        EquipmentGameObject = Equipment;
        return Equipment;
    }

    public static AirplaneEquipment? Load(string _EquipmentName)
    {
        if (_EquipmentName == None)
            return null;

        Debug.Log(_EquipmentName);
        if (_Equipments.ContainsKey(_EquipmentName))
            return JsonConvert.DeserializeObject<AirplaneEquipment>(_Equipments[_EquipmentName]);
        string FileData = "";
        string FilePath = AbsolutePath.EquipmentsSettings + $"{_EquipmentName}.json";

        if (File.Exists(FilePath))
            FileData = File.ReadAllText(FilePath);

        AirplaneEquipment Equipment = JsonConvert.DeserializeObject<AirplaneEquipment>(FileData);
        _Equipments[_EquipmentName] = FileData;
        Debug.Log(Equipment.Name);
        return Equipment;
    }

    public class DeafaultAirplaneEquipment : AirplaneEquipment
    {
        public DeafaultAirplaneEquipment()
        {
            Mass = 1;
            Name = "Default";
            EquipmentType = EquipmentType.Pylon;
            Diameter = 1;
        }

        public override GameObject InstantiateGameObject() => GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
}
