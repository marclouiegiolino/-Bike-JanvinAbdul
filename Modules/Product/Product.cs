using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Product
{
    public sealed class Product : IEquatable<Product>
    {
        private long?   _productId;
        private long?   _categoryId;
        private string? _brand;
        private string? _productName;
        private string? _description;
        private string? _imgUrl;
        private string? _status = "active";
        private string? _createdAt;

        public Product() { }

        public Product(long productId, long categoryId, string brand, string productName,
                       string description, string imgUrl, string status,
                       string createdAt)
        {
            ProductId       = productId;
            CategoryId      = categoryId;
            Brand           = brand;
            ProductName     = productName;
            Description     = description;
            ImgUrl          = imgUrl;
            Status          = string.IsNullOrWhiteSpace(status) ? "active" : status;
            CreatedAt       = createdAt;
        }

        [JsonPropertyName("product_id")]
        [JsonPropertyOrder(1)]
        public long ProductId
        {
            get => _productId ?? 0L;
            set => _productId = value;
        }

        [JsonPropertyName("category_id")]
        [JsonPropertyOrder(2)]
        public long CategoryId
        {
            get => _categoryId ?? 0L;
            set => _categoryId = value;
        }

        [JsonPropertyName("brand")]
        [JsonPropertyOrder(3)]
        public string Brand
        {
            get => _brand ?? string.Empty;
            set => _brand = NormalizeSpaces(value);
        }

        [JsonPropertyName("product_name")]
        [JsonPropertyOrder(4)]
        public string ProductName
        {
            get => _productName ?? string.Empty;
            set => _productName = NormalizeSpaces(value);
        }

        [JsonPropertyName("description")]
        [JsonPropertyOrder(5)]
        public string Description
        {
            get => _description ?? string.Empty;
            set => _description = NormalizeSpaces(value);
        }

        [JsonPropertyName("image_url")]
        [JsonPropertyOrder(6)]
        public string ImgUrl
        {
            get => _imgUrl ?? string.Empty;
            set => _imgUrl = value;
        }

        [JsonIgnore]
        public string ImageUrl
        {
            get => ImgUrl;
            set => ImgUrl = value;
        }

        [JsonPropertyName("status")]
        [JsonPropertyOrder(7)]
        public string Status
        {
            get => string.IsNullOrWhiteSpace(_status) ? "active" : _status;
            set => _status = NormalizeSpaces(value);
        }

        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(8)]
        public string CreatedAt
        {
            get => _createdAt ?? string.Empty;
            set => _createdAt = value;
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(Product? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ProductId > 0 || other.ProductId > 0) return ProductId == other.ProductId;
            return CategoryId == other.CategoryId
                && string.Equals(Brand, other.Brand, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ProductName, other.ProductName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ImgUrl, other.ImgUrl, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Status, other.Status, StringComparison.OrdinalIgnoreCase)
                && string.Equals(CreatedAt, other.CreatedAt, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Product);

        public override int GetHashCode()
        {
            if (ProductId > 0) return ProductId.GetHashCode();
            return HashCode.Combine(CategoryId, Brand?.ToLowerInvariant(), ProductName?.ToLowerInvariant(), Description?.ToLowerInvariant(), ImgUrl?.ToLowerInvariant(), Status?.ToLowerInvariant(), CreatedAt?.ToLowerInvariant());
        }
    }
}
