using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Reviews
{
    public sealed class Review : IEquatable<Review>
    {
        private long?   _reviewId;
        private long?   _productId;
        private long? _userId;
        private int? _rating;
        private string? _comment;
        private string? _createdAt;

        public Review() { }

        public Review(long reviewId, long productId, long userId, int rating, string comment, string createdAt)
        {
            ReviewId        = reviewId;
            ProductId       = productId;
            UserId          = userId;
            Rating          = rating;
            Comment         = comment;
            CreatedAt       = createdAt;
        }

        [JsonPropertyName("review_id")]
        [JsonPropertyOrder(1)]
        public long ReviewId
        {
            get => _reviewId ?? 0L;
            set => _reviewId = value;
        }

        [JsonPropertyName("product_id")]
        [JsonPropertyOrder(2)]
        public long ProductId
        {
            get => _productId ?? 0L;
            set => _productId = value;
        }

        [JsonPropertyName("user_id")]
        [JsonPropertyOrder(3)]
        public long UserId
        {
            get => _userId ?? 0L;
            set => _userId = value;
        }

        [JsonPropertyName("rating")]
        [JsonPropertyOrder(4)]
        public int Rating
        {
            get => _rating ?? 0;
            set => _rating = value;
        }

        [JsonPropertyName("comment")]
        [JsonPropertyOrder(5)]
        public string Comment
        {
            get => _comment ?? string.Empty;
            set => _comment = NormalizeSpaces(value);
        }

        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(6)]
        public string CreatedAt
        {
            get => _createdAt ?? string.Empty;
            set => _createdAt = NormalizeSpaces(value);
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(Review? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ReviewId > 0 || other.ReviewId > 0) return ReviewId == other.ReviewId;
            return ProductId == other.ProductId
                && UserId == other.UserId
                && string.Equals(Comment, other.Comment, StringComparison.OrdinalIgnoreCase)
                && string.Equals(CreatedAt, other.CreatedAt, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Review);

        public override int GetHashCode()
        {
            if (ReviewId > 0) return ReviewId.GetHashCode();
            return HashCode.Combine(ProductId, UserId, Comment?.ToLowerInvariant(), CreatedAt?.ToLowerInvariant());
        }
    }
}
