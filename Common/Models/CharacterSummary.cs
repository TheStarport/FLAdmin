using MongoDB.Bson;

namespace FlAdmin.Common.Models;

public class CharacterSummary
{
    public required ObjectId Id { get; set; }
    public required string Name { get; set; }
    public required int Money { get; set; }
    public string AccountId { get; set; }
    public long Base { get; set; }
    public string Rep { get; set; }
}