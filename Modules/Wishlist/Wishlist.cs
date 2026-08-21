using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Wishlist
{
    public sealed class Wishlist : IEquatable<Wishlist>
    {
        private long?   _wishlistId;
        private long?   _userId;
        private long? _variantId;
        private string? _addedAt;

        public Wishlist() { }

        public Wishlist(long wishlistId, long userId, long variantId, string addedAt)
        {
            WishlistId  = wishlistId;
            UserId      = userId;
            VariantId   = variantId;
            AddedAt   = addedAt;
        }

        [JsonPropertyName("wishlist_id")]
        [JsonPropertyOrder(1)]
        public long WishlistId
        {
            get => _wishlistId ?? 0L;
            set => _wishlistId = value;
        }

        [JsonPropertyName("user_id")]
        [JsonPropertyOrder(2)]
        public long UserId
        {
            get => _userId ?? 0L;
            set => _userId = value;
        }

        [JsonPropertyName("variant_id")]
        [JsonPropertyOrder(3)]
        public long VariantId
        {
            get => _variantId ?? 0L;
            set => _variantId = value;
        }

        [JsonPropertyName("added_at")]
        [JsonPropertyOrder(4)]
        public string AddedAt
        {
            get => _addedAt ?? string.Empty;
            set => _addedAt = NormalizeSpaces(value);
        }


        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(Wishlist? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (WishlistId > 0 || other.WishlistId > 0) return WishlistId == other.WishlistId;
            return UserId == other.UserId
                && VariantId == other.VariantId
                && string.Equals(AddedAt, other.AddedAt, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Wishlist);

        public override int GetHashCode()
        {
            if (WishlistId > 0) return WishlistId.GetHashCode();
            return HashCode.Combine(UserId, VariantId, AddedAt?.ToLowerInvariant());
        }
    }
}
