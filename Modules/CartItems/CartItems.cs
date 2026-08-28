using System;
using System.Text.Json.Serialization;

namespace Api.Modules.CartItems
{
    public sealed class CartItems : IEquatable<CartItems>
    {
        private long?   _cartItemId;
        private long?   _userId;
        private long?   _variantId;
        private int?    _quantity;
        private string? _addedAt;

        public CartItems() { }

        public CartItems(long cartItemId, long userId, long variantId, int quantity, string addedAt)
        {
            CartItemId      = cartItemId;
            UserId          = userId;
            VariantId       = variantId;
            Quantity        = quantity;
            AddedAt         = addedAt;
        }

        [JsonPropertyName("cart_item_id")]
        [JsonPropertyOrder(1)]
        public long CartItemId
        {
            get => _cartItemId ?? 0L;
            set => _cartItemId = value;
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

        [JsonPropertyName("quantity")]
        [JsonPropertyOrder(4)]
        public int Quantity
        {
            get => _quantity ?? 0;
            set => _quantity = value;
        }

        [JsonPropertyName("added_at")]
        [JsonPropertyOrder(5)]
        public string AddedAt
        {
            get => _addedAt ?? DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
            set => _addedAt = value;
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(CartItems? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (CartItemId > 0 || other.CartItemId > 0) return CartItemId == other.CartItemId;
            return CartItemId == other.CartItemId
                && UserId == other.UserId
                && VariantId == other.VariantId
                && Quantity == other.Quantity
                && AddedAt == other.AddedAt;
        }

        public override bool Equals(object? obj) => Equals(obj as CartItems);

        public override int GetHashCode()
        {
            if (CartItemId > 0) return CartItemId.GetHashCode();
            return HashCode.Combine(CartItemId, UserId, VariantId, Quantity, AddedAt);
        }
    }
}
