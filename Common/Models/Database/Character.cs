using System.Numerics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlAdmin.Common.Models.Database;

public class Character
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonElement("accountId")] public string AccountId { get; set; }

    [BsonElement("characterName")] public required string CharacterName { get; set; }
    
    [BsonElement("money")] public int Money { get; set; }

    [BsonElement("rank")] public int Rank { get; set; }

    [BsonElement("repGroup")] public string? RepGroup { get; set; }

    [BsonElement("pos")] public Vector3? Pos { get; set; }

    [BsonElement("rot")] public Vector3? Rot { get; set; }

    [BsonElement("interface")] public int InterfaceState { get; set; }

    [BsonElement("hullStatus")] public float HullStatus { get; set; }

    [BsonElement("baseHullStatus")] public float BaseHullStatus { get; set; }

    [BsonElement("canDock")] public bool CanDock { get; set; }

    [BsonElement("canTradeLane")] public bool CanTradeLane { get; set; }

    [BsonElement("lastDockedBase")] public long LastDockedBase { get; set; }

    [BsonElement("currentBase")] public long CurrentBase { get; set; }

    [BsonElement("currentRoom")] public long CurrentRoom { get; set; }

    [BsonElement("numOfKills")] public int KillCount { get; set; }

    [BsonElement("numOfFailedMissions")] public int MissionFailureCount { get; set; }

    [BsonElement("numOfSuccessMissions")] public int MissionSuccessCount { get; set; }

    [BsonElement("shipHash")] public long ShipHash { get; set; }

    [BsonElement("system")] public long System { get; set; }

    [BsonElement("totalTimePlayed")] public long TotalTimePlayed { get; set; }

    [BsonElement("baseCostume")] public Costume? BaseCostume { get; set; }

    [BsonElement("commCostume")] public Costume? CommCostume { get; set; }

    [BsonElement("reputation")] public Dictionary<string, float> Reputation { get; set; } = new();

    [BsonElement("equipment")] public List<Equipment> Equipment { get; set; } = new();

    [BsonElement("baseEquipment")] public List<Equipment> BaseEquipment { get; set; } = new();

    [BsonElement("cargo")] public List<Cargo> Cargo { get; set; } = new();

    [BsonElement("baseCargo")] public List<Cargo> BaseCargo { get; set; } = new();

    [BsonElement("collisionGroups")] public Dictionary<string, float> CollisionGroups { get; set; } = new();

    [BsonElement("visits")] public List<List<int>> Visits { get; set; } = new();

    [BsonElement("shipTypesKilled")] public Dictionary<string, int> ShipTypesKilled { get; set; } = new();

    [BsonElement("systemsVisited")] public List<long> SystemsVisited { get; set; } = new();

    [BsonElement("basesVisited")] public List<long> BasesVisited { get; set; } = new();

    [BsonElement("jumpHolesVisited")] public List<long> JumpHolesVisited { get; set; } = new();

    [BsonElement("npcVisits")] public List<NpcVisit> NpcVisits { get; set; } = new();

    [BsonElement("weaponGroups")] public Dictionary<string, List<string>> WeaponGroups { get; set; } = new();

    [BsonExtraElements] public BsonDocument? Extra { get; set; }
}