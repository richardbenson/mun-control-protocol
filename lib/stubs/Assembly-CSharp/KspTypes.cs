// KSP Assembly-CSharp stub — compile-time only, no runtime use.
using System;
using System.Collections.Generic;

public static class HighLogic
{
    public static Game CurrentGame => throw new NotImplementedException();
    public static GameScenes LoadedScene => throw new NotImplementedException();
}

public class Game
{
    public GameParameters Parameters => throw new NotImplementedException();
    public FlightState flightState => throw new NotImplementedException();
    public KerbalRoster CrewRoster => throw new NotImplementedException();
}

public enum GameScenes { SPACECENTER, EDITOR, FLIGHT, TRACKSTATION }

public class GameParameters
{
    public T CustomParams<T>() where T : GameParameters.CustomParameterNode => throw new NotImplementedException();
    public DifficultyParams Difficulty => throw new NotImplementedException();
    public CareerParams Career => throw new NotImplementedException();

    public class CustomParameterNode { }

    public class DifficultyParams
    {
        public bool EnableCommNet;
        public double ReentryHeatScale;
        public bool MissingCrewsRespawn;
    }

    public class CareerParams
    {
        public double ScienceGainMultiplier;
        public double FundsGainMultiplier;
        public double RepGainMultiplier;
        public double FundsLossMultiplier;
        public double RepLossMultiplier;
    }
}

namespace CommNet
{
    public class CommNetParams : GameParameters.CustomParameterNode
    {
        public double rangeModifier;
        public double DSNModifier;
        public bool requireSignalForControl;
        public float occlusionMultiplierVac;
        public bool plasmaBlackout;
    }
}

public class FlightState
{
    public List<ProtoVessel> protoVessels => throw new NotImplementedException();
}

public class ProtoVessel
{
    public string vesselName;
    public List<ProtoCrewMember> GetVesselCrew() => throw new NotImplementedException();
}

public class ProtoCrewMember
{
    public string name;
    public int experienceLevel;
    public string trait;
    public RosterStatus rosterStatus;

    public enum RosterStatus { Available, Assigned, Dead, Missing }
}

public class KerbalRoster
{
    public IEnumerable<ProtoCrewMember> Crew => throw new NotImplementedException();
    public IEnumerable<ProtoCrewMember> Tourist => throw new NotImplementedException();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class KSPAddon : Attribute
{
    public KSPAddon(KSPAddon.Startup startup, bool once) { }
    public enum Startup { Instantly = 0 }
}

public static class ScenarioUpgradeableFacilities
{
    public static float GetFacilityLevel(string facilityName) => throw new NotImplementedException();
    public static int GetFacilityLevelCount(string facilityName) => throw new NotImplementedException();
}

public class ResearchAndDevelopment
{
    public static ResearchAndDevelopment Instance => throw new NotImplementedException();
    public static string GetTechnologyTitle(string techId) => throw new NotImplementedException();
    public static RDTech.State GetTechnologyState(string techId) => throw new NotImplementedException();
    public ProtoTechNode GetTechState(string techId) => throw new NotImplementedException();
    public static bool PartModelPurchased(AvailablePart ap) => throw new NotImplementedException();
}

public class ProtoTechNode
{
    public int scienceCost;
}

public class RDTech
{
    public enum State { Available }
}

public class GameDatabase
{
    public static GameDatabase Instance => throw new NotImplementedException();
    public ConfigNode[] GetConfigNodes(string typeName) => throw new NotImplementedException();
}

public class ConfigNode
{
    public string GetValue(string name) => throw new NotImplementedException();
    public ConfigNode[] GetNodes(string name) => throw new NotImplementedException();
}

public static class PartLoader
{
    public static List<AvailablePart> LoadedPartsList => throw new NotImplementedException();
}

public class AvailablePart
{
    public string name;
    public string title;
    public string TechRequired;
    public PartCategories category;
    public float cost;
    public Part partPrefab;
}

public enum PartCategories
{
    Propulsion, FuelTank, Control, Structural, Aero, Utility, Science,
    Communication, Coupling, Payload, Ground, Thermal
}

public class Part
{
    public float mass;
    public int CrewCapacity;
    public PartResourceList Resources => throw new NotImplementedException();
    public PartModuleList Modules => throw new NotImplementedException();
    public AvailablePart partInfo;
    public int inverseStage;
}

public enum EditorFacility { None = -1, VAB = 0, SPH = 1 }

public class ShipConstruct
{
    public string shipName;
    public EditorFacility shipFacility;
    public List<Part> parts => throw new NotImplementedException();
}

public class EditorLogic
{
    public static EditorLogic fetch => throw new NotImplementedException();
    public ShipConstruct ship => throw new NotImplementedException();
}

public class PartResourceList : IEnumerable<PartResource>
{
    public int Count => throw new NotImplementedException();
    public IEnumerator<PartResource> GetEnumerator() => throw new NotImplementedException();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

public class PartModuleList : IEnumerable<PartModule>
{
    public IEnumerator<PartModule> GetEnumerator() => throw new NotImplementedException();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

public class PartModule { }

public class PartResource
{
    public double amount;
    public double maxAmount;
    public string resourceName;
    public PartResourceDefinition info => throw new NotImplementedException();
}

public class PartResourceDefinition
{
    public double density;
}

public class ModuleEngines : PartModule
{
    public FloatCurve atmosphereCurve => throw new NotImplementedException();
    public double maxThrust;
    public float maxFuelFlow;
    public List<Propellant> propellants => throw new NotImplementedException();
}

public class FloatCurve
{
    public float Evaluate(float t) => throw new NotImplementedException();
}

public class Propellant
{
    public string name;
}

public class ModuleDataTransmitter : PartModule
{
    public double antennaPower;
    public AntennaType antennaType;
    public bool antennaCombinable;
    public float packetSize;
    public float packetInterval;
}

public enum AntennaType { INTERNAL, DIRECT, RELAY }

public class ModuleCommand : PartModule { }

public class ModuleSAS : PartModule
{
    public int SASServiceLevel;
}

public class ModuleDeployableSolarPanel : PartModule
{
    public float chargeRate;
    public bool retractable;
}

public static class FlightGlobals
{
    public static List<CelestialBody> Bodies => throw new NotImplementedException();
}

public class CelestialBody
{
    public string name;
}

public class ScienceSubject
{
    public double scienceCap;
    public double science;
    public double subjectValue;
    public double scientificValue;
    public string title;
    public string id;
}
