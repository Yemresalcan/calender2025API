using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class WeeklyTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [Required]
    public string UserId { get; set; }

    [BsonElement("monthId")]
    [Required]
    public string MonthId { get; set; }

    [BsonElement("weekNumber")]
    [Required]
    public int WeekNumber { get; set; }

    [BsonElement("startDate")]
    [Required]
    public string StartDate { get; set; }

    [BsonElement("endDate")]
    [Required]
    public string EndDate { get; set; }

    [BsonElement("days")]
    public List<int> Days { get; set; } = new List<int>();

    [BsonElement("color")]
    [Required]
    public string Color { get; set; }

    [BsonElement("taskText")]
    [Required]
    public string TaskText { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 