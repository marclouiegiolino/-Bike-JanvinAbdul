using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Orders
{
    public sealed class Order : IEquatable<Order>
    {
        private long?   _orderId;
        private long?   _userId;
        private string? _shippingAddress;
        private string? _subtotal;
        private string? _discountAmount;
        private string? _shippingFee;
        private string? _totalAmount;
        private string? _status;
        private string? _placedAt;

        public Order() { }

        public Order(long userId, string shippingAddress, string subtotal, string discountAmount,
                       string shippingFee, string totalAmount, string status, string placedAt)
        {
            UserId          = userId;
            ShippingAddress = shippingAddress;
            Subtotal        = subtotal;
            DiscountAmount  = discountAmount;
            ShippingFee     = shippingFee;
            TotalAmount     = totalAmount;
            Status          = status;
            PlacedAt        = placedAt;
        }

        [JsonPropertyName("order_id")]
        [JsonPropertyOrder(1)]
        public long OrderId
        {
            get => _orderId ?? 0L;
            set => _orderId = value;
        }

        [JsonPropertyName("user_id")]
        [JsonPropertyOrder(2)]
        public long UserId
        {
            get => _userId ?? 0L;
            set => _userId = value;
        }

        [JsonPropertyName("shipping_address")]
        [JsonPropertyOrder(3)]
        public string ShippingAddress
        {
            get => _shippingAddress ?? string.Empty;
            set => _shippingAddress = NormalizeSpaces(value);
        }

        [JsonPropertyName("subtotal")]
        [JsonPropertyOrder(4)]
        public string Subtotal
        {
            get => _subtotal ?? string.Empty;
            set => _subtotal = value;
        }

        [JsonPropertyName("discount_amount")]
        [JsonPropertyOrder(5)]
        public string DiscountAmount
        {
            get => _discountAmount ?? string.Empty;
            set => _discountAmount = value;
        }

        [JsonPropertyName("shipping_fee")]
        [JsonPropertyOrder(6)]
        public string ShippingFee
        {
            get => _shippingFee ?? string.Empty;
            set => _shippingFee = value;
        }

        [JsonPropertyName("total_amount")]
        [JsonPropertyOrder(7)]
        public string TotalAmount
        {
            get => _totalAmount ?? string.Empty;
            set => _totalAmount = value;
        }

        [JsonPropertyName("status")]
        [JsonPropertyOrder(8)]
        public string Status
        {
            get => _status ?? string.Empty;
            set => _status = value;
        }

        [JsonPropertyName("placed_at")]
        [JsonPropertyOrder(9)]
        public string PlacedAt
        {
            get => _placedAt ?? string.Empty;
            set => _placedAt = value;
        }

        public override string ToString() => "Order[" + string.Join(", ", new[]
        {
            "OrderId=" + OrderId,
            "UserId=" + UserId,
            "ShippingAddress=" + ShippingAddress,
            "Subtotal=" + Subtotal,
            "DiscountAmount=" + DiscountAmount,
            "ShippingFee=" + ShippingFee,
            "TotalAmount=" + TotalAmount,
            "Status=" + Status,
            "PlacedAt=" + PlacedAt
        }) + "]";

        public bool Equals(Order? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (OrderId > 0 || other.OrderId > 0) return OrderId == other.OrderId;
            return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Order);

        public override int GetHashCode()
        {
            if (OrderId > 0) return OrderId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }
    }
}
