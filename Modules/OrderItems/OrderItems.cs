using System;
using System.Text.Json.Serialization;

namespace Api.Modules.OrderItems
{
    public sealed class OrderItem : IEquatable<OrderItem>
    {
        private long?   _orderItemId;
        private long?   _orderId;
        private long?   _variantId;
        private int?    _quantity;
        private decimal?  _unitPrice;
        private decimal?  _subtotal;
        

        public OrderItem() { }

        public OrderItem(long orderItemId, long orderId, long variantId, int quantity, decimal unitPrice, decimal subtotal)
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
        public decimal UnitPrice
        {
            get => _unitPrice ?? 0m;
            set => _unitPrice = value;
        }

        [JsonPropertyName("subtotal")]
        [JsonPropertyOrder(6)]
        public decimal Subtotal
        {
            get => _subtotal ?? 0m;
            set => _subtotal = value;
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
                && UnitPrice == other.UnitPrice
                && Subtotal == other.Subtotal;
        }

        public override bool Equals(object? obj) => Equals(obj as OrderItem);

        public override int GetHashCode()
        {
            if (OrderItemId > 0) return OrderItemId.GetHashCode();
            return HashCode.Combine(OrderId, VariantId, UnitPrice, Subtotal);
        }
    }
}
