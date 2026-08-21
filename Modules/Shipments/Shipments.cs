using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Shipments
{
    public sealed class Shipment : IEquatable<Shipment>
    {
        private long?   _shipmentId;
        private long?   _orderId;
        private string? _carrierName;
        private string? _trackingNumber;
        private string? _status;
        private string? _shippedAt;
        private string? _deliveredAt;

        public Shipment() { }

        public Shipment(long shipmentId, long orderId, string carrierName, string trackingNumber,
                       string status, string shippedAt, string deliveredAt)
        {
            ShipmentId       = shipmentId;
            OrderId          = orderId;
            CarrierName      = carrierName;
            TrackingNumber   = trackingNumber;
            Status          = string.IsNullOrWhiteSpace(status) ? "active" : status;
            ShippedAt       = shippedAt;
            DeliveredAt     = deliveredAt;
        }

        [JsonPropertyName("shipment_id")]
        [JsonPropertyOrder(1)]
        public long ShipmentId
        {
            get => _shipmentId ?? 0L;
            set => _shipmentId = value;
        }

        [JsonPropertyName("order_id")]
        [JsonPropertyOrder(2)]
        public long OrderId
        {
            get => _orderId ?? 0L;
            set => _orderId = value;
        }

        [JsonPropertyName("carrier_name")]
        [JsonPropertyOrder(3)]
        public string CarrierName
        {
            get => _carrierName ?? string.Empty;
            set => _carrierName = NormalizeSpaces(value);
        }

        [JsonPropertyName("tracking_number")]
        [JsonPropertyOrder(4)]
        public string TrackingNumber
        {
            get => _trackingNumber ?? string.Empty;
            set => _trackingNumber = NormalizeSpaces(value);
        }

        [JsonPropertyName("status")]
        [JsonPropertyOrder(5)]
        public string Status
        {
            get => _status ?? string.Empty;
            set => _status = NormalizeSpaces(value);
        }

        [JsonPropertyName("shipped_at")]
        [JsonPropertyOrder(6)]
        public string ShippedAt
        {
            get => _shippedAt ?? string.Empty;
            set => _shippedAt = value;
        }

        [JsonPropertyName("delivered_at")]
        [JsonPropertyOrder(7)]
        public string DeliveredAt
        {
            get => string.IsNullOrWhiteSpace(_deliveredAt) ? "active" : _deliveredAt;
            set => _deliveredAt = NormalizeSpaces(value);
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(Shipment? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ShipmentId > 0 || other.ShipmentId > 0) return ShipmentId == other.ShipmentId;
            return OrderId == other.OrderId
                && string.Equals(CarrierName, other.CarrierName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(TrackingNumber, other.TrackingNumber, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Status, other.Status, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ShippedAt, other.ShippedAt, StringComparison.OrdinalIgnoreCase)
                && string.Equals(DeliveredAt, other.DeliveredAt, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Shipment);

        public override int GetHashCode()
        {
            if (ShipmentId > 0) return ShipmentId.GetHashCode();
            return HashCode.Combine(OrderId, CarrierName?.ToLowerInvariant(), TrackingNumber?.ToLowerInvariant(), Status?.ToLowerInvariant(), ShippedAt?.ToLowerInvariant(), DeliveredAt?.ToLowerInvariant());
        }
    }
}
