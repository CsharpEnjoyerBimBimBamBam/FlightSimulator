using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[JsonConverter(typeof(EquipmentConverter))]
public class Pylon : AirplaneEquipment
{
    public Pylon() => EquipmentType = EquipmentType.Pylon;

    public IReadOnlyList<PylonWeaponStation> WeaponStations { get; private set; } = new List<PylonWeaponStation>();

    public override GameObject InstantiateGameObject()
    {
        GameObject Pylon = base.InstantiateGameObject();

        foreach (PylonWeaponStation Station in WeaponStations)
        {
            GameObject Armament = Station.CurrentEquipment?.InstantiateGameObject();

            if (Armament == null)
                continue;

            Armament.transform.parent = Pylon.transform;
            Armament.transform.localPosition = Station.LocalPosition;
            Armament.transform.localEulerAngles = Vector3.zero;
        }

        return Pylon;
    }

    public Pylon CopyInstance() => new Pylon { WeaponStations = new List<PylonWeaponStation>(WeaponStations) };

    public class PylonWeaponStation
    {
        public int Number { get; private set; }
        public int Index => Number - 1;
        public IReadOnlyList<string> PossibleArmament { get; private set; } = new List<string>();
        public Vector3 LocalPosition { get; private set; }
        public AirplaneEquipment? CurrentEquipment;
    }
}
