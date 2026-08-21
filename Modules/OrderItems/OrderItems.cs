using System;
using System.Text.Json.Serialization;

namespace Api.Modules.OrderItems
{
    public sealed class OrderItem : IEquatable<OrderItem>
    {
        private long?   _orderItemId;
        private long?   _orderId;
        private long? _variantId;
        private int? _quantity;
        private string? _unitPrice;
        private string? _subtotal;
        

        public OrderItem() { }

        public OrderItem(long orderItemId, long orderId, long variantId, int quantity, string unitPrice, string subtotal)
        {
            OrderItemId = orderItemId;
            OrderId     = orderId;
            VariantId   = variantId;
            Quantity    = quantity;
            UnitPrice   = unitPrice;
            Subtotal    = subtotal;
        }

        [JsonPropertyName("order_item_id")]
        [JsonPropertyOrder(1)]
        public long OrderItemId
        {
            get => _orderItemId ?? 0L;
            set => _orderItemId = value;
        }

        [JsonPropertyName("order_id")]
        [JsonPropertyOrder(2)]
        public long OrderId
        {
            get => _orderId ?? 0L;
            set => _orderId = value;
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

        [JsonPropertyName("unit_price")]
        [JsonPropertyOrder(5)]
        public string UnitPrice
        {
            get => _unitPrice ?? string.Empty;
            set => _unitPrice = NormalizeSpaces(value);
        }

        [JsonPropertyName("subtotal")]
        [JsonPropertyOrder(6)]
        public string Subtotal
        {
            get => _subtotal ?? string.Empty;
            set => _subtotal = NormalizeSpaces(value);
        }
        

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(OrderItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (OrderItemId > 0 || other.OrderItemId > 0) return OrderItemId == other.OrderItemId;
            return OrderId == other.OrderId
                && VariantId == other.VariantId
                && string.Equals(UnitPrice, other.UnitPrice, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Subtotal, other.Subtotal, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as OrderItem);

        public override int GetHashCode()
        {
            if (OrderItemId > 0) return OrderItemId.GetHashCode();
            return HashCode.Combine(OrderId, VariantId, UnitPrice?.ToLowerInvariant(), Subtotal?.ToLowerInvariant());
        }
    }
}
