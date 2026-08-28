using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Payments
{
    public sealed class Payment : IEquatable<Payment>
    {
        private long?   _paymentId;
        private long?   _orderId;
        private string? _paymentType;
        private decimal? _amount;
        private string? _status;
        private string? _paidAt;

        public Payment() { }

        public Payment(long paymentId, long orderId, string paymentType, decimal amount, string status, string paidAt)
        {
            PaymentId       = paymentId;
            OrderId         = orderId;
            PaymentType     = paymentType;
            Amount          = amount;
            Status          = status;
            PaidAt          = paidAt;
        }

        [JsonPropertyName("payment_id")]
        [JsonPropertyOrder(1)]
        public long PaymentId
        {
            get => _paymentId ?? 0L;
            set => _paymentId = value;
        }

        [JsonPropertyName("order_id")]
        [JsonPropertyOrder(2)]
        public long OrderId
        {
            get => _orderId ?? 0L;
            set => _orderId = value;
        }

        [JsonPropertyName("payment_type")]
        [JsonPropertyOrder(3)]
        public string PaymentType
        {
            get => _paymentType ?? string.Empty;
            set => _paymentType = value;
        }

        [JsonPropertyName("amount")]
        [JsonPropertyOrder(4)]
        public decimal Amount
        {
            get => _amount ?? 0m;
            set => _amount = value;
        }

        [JsonPropertyName("status")]
        [JsonPropertyOrder(5)]
        public string Status
        {
            get => _status ?? string.Empty;
            set => _status = value;
        }

        [JsonPropertyName("paid_at")]
        [JsonPropertyOrder(6)]
        public string PaidAt
        {
            get => _paidAt ?? string.Empty;
            set => _paidAt = value;
        }


        public override string ToString() => "Payments[" + string.Join(", ", new[]
        {
            "PaymentId=" + PaymentId,
            "OrderId=" + OrderId,
            "PaymentType=" + PaymentType,
            "Amount=" + Amount,
            "Status=" + Status,
            "PaidAt=" + PaidAt
        }) + "]";

        public bool Equals(Payment? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (PaymentId > 0 || other.PaymentId > 0) return PaymentId == other.PaymentId;
            return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Payment);

        public override int GetHashCode()
        {
            if (PaymentId > 0) return PaymentId.GetHashCode();
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
