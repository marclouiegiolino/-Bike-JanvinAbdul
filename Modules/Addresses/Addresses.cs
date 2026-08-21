using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Addresses
{
    public sealed class Address : IEquatable<Address>
    {
        private long?   _addressId;
        private long?   _userId;
        private string? _recipientName;
        private string? _phoneNumber;
        private string? _city;
        private string? _provinceState;
        private string? _postalCode;
        private string? _country;
        private bool    _isDefault;

        public Address() { }

        public Address(long addressId, long userId, string recipientName, string phoneNumber,
                       string city, string provinceState, string postalCode, string country, bool isDefault)
        {
            AddressId       = addressId;
            UserId          = userId;
            RecipientName   = recipientName;
            PhoneNumber     = phoneNumber;
            City            = city;
            ProvinceState   = provinceState;
            PostalCode      = postalCode;
            Country         = country;
            IsDefault       = isDefault;
        }

        [JsonPropertyName("address_id")]
        [JsonPropertyOrder(1)]
        public long AddressId
        {
            get => _addressId ?? 0L;
            set => _addressId = value;
        }

        [JsonPropertyName("user_id")]
        [JsonPropertyOrder(2)]
        public long UserId
        {
            get => _userId ?? 0L;
            set => _userId = value;
        }

        [JsonPropertyName("recipient_name")]
        [JsonPropertyOrder(3)]
        public string RecipientName
        {
            get => _recipientName ?? string.Empty;
            set => _recipientName = NormalizeSpaces(value);
        }

        [JsonPropertyName("phone_number")]
        [JsonPropertyOrder(4)]
        public string PhoneNumber
        {
            get => _phoneNumber ?? string.Empty;
            set => _phoneNumber = value;
        }

        [JsonPropertyName("city")]
        [JsonPropertyOrder(5)]
        public string City
        {
            get => _city ?? string.Empty;
            set => _city = value;
        }

        [JsonPropertyName("province_state")]
        [JsonPropertyOrder(6)]
        public string ProvinceState
        {
            get => _provinceState ?? string.Empty;
            set => _provinceState = value;
        }

        [JsonPropertyName("postal_code")]
        [JsonPropertyOrder(7)]
        public string PostalCode
        {
            get => _postalCode ?? string.Empty;
            set => _postalCode = value;
        }

        [JsonPropertyName("country")]
        [JsonPropertyOrder(8)]
        public string Country
        {
            get => _country ?? string.Empty;
            set => _country = value;
        }

        [JsonPropertyName("is_default")]
        [JsonPropertyOrder(9)]
        public bool IsDefault
        {
            get => _isDefault;
            set => _isDefault = value;
        }

        public override string ToString() => "Address[" + string.Join(", ", new[]
        {
            "AddressId=" + AddressId,
            "UserId=" + UserId,
            "RecipientName=" + RecipientName,
            "PhoneNumber=" + PhoneNumber,
            "City=" + City,
            "ProvinceState=" + ProvinceState,
            "PostalCode=" + PostalCode,
            "Country=" + Country,
            "IsDefault=" + IsDefault
        }) + "]";

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (AddressId > 0 || other.AddressId > 0) return AddressId == other.AddressId;
            return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Address);

        public override int GetHashCode()
        {
            if (AddressId > 0) return AddressId.GetHashCode();
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
