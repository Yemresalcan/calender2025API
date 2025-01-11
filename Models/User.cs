using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CalendarAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;
        
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("resetPasswordToken")]
        public string? ResetPasswordToken { get; set; }

        [BsonElement("resetPasswordExpiry")]
        public DateTime? ResetPasswordExpiry { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 