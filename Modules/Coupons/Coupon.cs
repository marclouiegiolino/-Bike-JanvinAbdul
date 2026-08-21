using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Coupons
{
    public sealed class Coupon
    {
        private long? _couponId;
        private string? _code;
        private string? _discountType;
        private string? _discountValue;
        private string? _validFrom;
        private string? _validUntil;
        private bool? _isActive;

        public Coupon() { }

        public Coupon(long couponId, string code, string discountType, string discountValue, string validFrom, string validUntil, bool isActive)
        {
            CouponId     = couponId;
            Code         = code;
            DiscountType = discountType;
            DiscountValue= discountValue;
            ValidFrom    = validFrom;
            ValidUntil   = validUntil;
            IsActive     = isActive;
        }

        [JsonPropertyName("coupon_id")]
        [JsonPropertyOrder(1)]
        public long CouponId
        {
            get => _couponId ?? 0L;
            set => _couponId = value;
        }

        [JsonPropertyName("code")]
        [JsonPropertyOrder(2)]
        public string Code
        {
            get => _code ?? string.Empty;
            set => _code = NormalizeSpaces(value);
        }

        [JsonPropertyName("discount_type")]
        [JsonPropertyOrder(3)]
        public string DiscountType
        {
            get => _discountType ?? string.Empty;
            set => _discountType = NormalizeSpaces(value);
        }

        [JsonPropertyName("discount_value")]
        [JsonPropertyOrder(4)]
        public string DiscountValue
        {
            get => _discountValue ?? string.Empty;
            set => _discountValue = NormalizeSpaces(value);
        }

        [JsonPropertyName("valid_from")]
        [JsonPropertyOrder(5)]
        public string ValidFrom
        {
            get => _validFrom ?? string.Empty;
            set => _validFrom = NormalizeSpaces(value);
        }

        [JsonPropertyName("valid_until")]
        [JsonPropertyOrder(6)]
        public string ValidUntil
        {
            get => _validUntil ?? string.Empty;
            set => _validUntil = NormalizeSpaces(value);
        }

        [JsonPropertyName("is_active")]
        [JsonPropertyOrder(7)]
        public bool IsActive
        {
            get => _isActive ?? false;
            set => _isActive = value;
        }

        private static string NormalizeSpaces(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}