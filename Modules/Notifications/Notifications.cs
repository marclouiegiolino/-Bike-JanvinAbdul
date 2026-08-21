using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Notifications
{
    public sealed class Notification : IEquatable<Notification>
    {
        private long?   _notificationId;
        private long?   _userId;
        private string? _type;
        private string? _message;
        private bool? _isRead;
        private string? _createdAt;

        public Notification() { }

        public Notification(long notificationId, long userId, string type, string message,
                       bool isRead, string createdAt)
        {
            NotificationId  = notificationId;
            UserId          = userId;
            Type            = type;
            Message         = message;
            IsRead          = isRead;
            CreatedAt       = createdAt;
        }

        [JsonPropertyName("notification_id")]
        [JsonPropertyOrder(1)]
        public long NotificationId
        {
            get => _notificationId ?? 0L;
            set => _notificationId = value;
        }

        [JsonPropertyName("user_id")]
        [JsonPropertyOrder(2)]
        public long UserId
        {
            get => _userId ?? 0L;
            set => _userId = value;
        }

        [JsonPropertyName("type")]
        [JsonPropertyOrder(3)]
        public string Type
        {
            get => _type ?? string.Empty;
            set => _type = NormalizeSpaces(value);
        }

        [JsonPropertyName("message")]
        [JsonPropertyOrder(4)]
        public string Message
        {
            get => _message ?? string.Empty;
            set => _message = NormalizeSpaces(value);
        }

        [JsonPropertyName("is_read")]
        [JsonPropertyOrder(5)]
        public bool IsRead
        {
            get => _isRead ?? false;
            set => _isRead = value;
        }

        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(6)]
        public string CreatedAt
        {
            get => _createdAt ?? string.Empty;
            set => _createdAt = value;
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(Notification? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (NotificationId > 0 || other.NotificationId > 0) return NotificationId == other.NotificationId;
            return UserId == other.UserId
                && string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Message, other.Message, StringComparison.OrdinalIgnoreCase)
                && IsRead == other.IsRead
                && string.Equals(CreatedAt, other.CreatedAt, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Notification);

        public override int GetHashCode()
        {
            if (NotificationId > 0) return NotificationId.GetHashCode();
            return HashCode.Combine(UserId, Type?.ToLowerInvariant(), Message?.ToLowerInvariant(), IsRead, CreatedAt?.ToLowerInvariant());
        }
    }
}
