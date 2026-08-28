using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Orders
{
    public sealed class Order : IEquatable<Order>
    {
        private long?     _orderId;
        private long?     _userId;
        private string?   _shippingAddress;
        private decimal?  _subtotal;
        private decimal?  _discountAmount;
        private decimal?  _shippingFee;
        private decimal?  _totalAmount;
        private string?   _status;
        private string?   _placedAt;

        public Order() { }

        public Order(long userId, string shippingAddress, decimal subtotal, decimal discountAmount,
                       decimal shippingFee, decimal totalAmount, string status, string placedAt)
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
        public decimal Subtotal
        {
            get => _subtotal ?? 0m;
            set => _subtotal = value;
        }

        [JsonPropertyName("discount_amount")]
        [JsonPropertyOrder(5)]
        public decimal DiscountAmount
        {
            get => _discountAmount ?? 0m;
            set => _discountAmount = value;
        }

        [JsonPropertyName("shipping_fee")]
        [JsonPropertyOrder(6)]
        public decimal ShippingFee
        {
            get => _shippingFee ?? 0m;
            set => _shippingFee = value;
        }

        [JsonPropertyName("total_amount")]
        [JsonPropertyOrder(7)]
        public decimal TotalAmount
        {
            get => _totalAmount ?? 0m;
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
