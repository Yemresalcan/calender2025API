using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CalendarAPI.Models
{
    public class Month
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("days")]
        public int Days { get; set; }

        [BsonElement("startDay")]
        public int StartDay { get; set; }  // 0: Pazar, 1: Pazartesi, ..., 6: Cumartesi

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
} 