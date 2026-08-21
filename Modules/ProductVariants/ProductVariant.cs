using System;
using System.Text.Json.Serialization;

namespace Api.Modules.ProductVariants
{
    public sealed class ProductVariant : IEquatable<ProductVariant>
    {
        private long?   _variantId;
        private long?   _productId;
        private string? _sku;
        private string? _frameSize;
        private string? _color;
        private string? _wheelSize;
        private string? _price;
        private int? _stockQuantity;
        private bool?   _isActive;

        public ProductVariant() { }

        public ProductVariant(long variantId, long productId, string sku, string frameSize,
                       string color, string wheelSize, string price, 
                       int stockQuantity, bool isActive)
        {
            VariantId       = variantId;
            ProductId       = productId;
            SKU             = sku;
            FrameSize       = frameSize;
            Color           = color;
            WheelSize       = wheelSize;
            Price           = price;
            StockQuantity   = stockQuantity;
            IsActive        = isActive;
        }

        [JsonPropertyName("variant_id")]
        [JsonPropertyOrder(1)]
        public long VariantId
        {
            get => _variantId ?? 0L;
            set => _variantId = value;
        }

        [JsonPropertyName("product_id")]
        [JsonPropertyOrder(2)]
        public long ProductId
        {
            get => _productId ?? 0L;
            set => _productId = value;
        }

        [JsonPropertyName("sku")]
        [JsonPropertyOrder(3)]
        public string SKU
        {
            get => _sku ?? string.Empty;
            set => _sku = NormalizeSpaces(value);
        }

        [JsonPropertyName("frame_size")]
        [JsonPropertyOrder(4)]
        public string FrameSize
        {
            get => _frameSize ?? string.Empty;
            set => _frameSize = NormalizeSpaces(value);
        }

        [JsonPropertyName("color")]
        [JsonPropertyOrder(5)]
        public string Color
        {
            get => _color ?? string.Empty;
            set => _color = NormalizeSpaces(value);
        }

        [JsonPropertyName("wheel_size")]
        [JsonPropertyOrder(6)]
        public string WheelSize
        {
            get => _wheelSize ?? string.Empty;
            set => _wheelSize = NormalizeSpaces(value);
        }

        [JsonPropertyName("price")]
        [JsonPropertyOrder(7)]
        public string Price
        {
            get => _price ?? string.Empty;
            set => _price = NormalizeSpaces(value);
        }

        [JsonPropertyName("stock_quantity")]
        [JsonPropertyOrder(8)]
        public int StockQuantity
        {
            get => _stockQuantity ?? 0;
            set => _stockQuantity = value;
        }

        [JsonPropertyName("is_active")]
        [JsonPropertyOrder(9)]
        public bool IsActive
        {
            get => _isActive ?? false;
            set => _isActive = value;
        }

        private static string NormalizeSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            var parts   = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }

        public bool Equals(ProductVariant? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (VariantId > 0 || other.VariantId > 0) return VariantId == other.VariantId;
            return ProductId == other.ProductId
                && string.Equals(SKU, other.SKU, StringComparison.OrdinalIgnoreCase)
                && string.Equals(FrameSize, other.FrameSize, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Color, other.Color, StringComparison.OrdinalIgnoreCase)
                && string.Equals(WheelSize, other.WheelSize, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Price, other.Price, StringComparison.OrdinalIgnoreCase)
                && StockQuantity == other.StockQuantity
                && IsActive == other.IsActive;
        }

        public override bool Equals(object? obj) => Equals(obj as ProductVariant);

        public override int GetHashCode()
        {
            if (VariantId > 0) return VariantId.GetHashCode();
            return HashCode.Combine(ProductId, SKU?.ToLowerInvariant(), FrameSize?.ToLowerInvariant(), Color?.ToLowerInvariant(), WheelSize?.ToLowerInvariant(), Price?.ToLowerInvariant(), StockQuantity, IsActive);
        }
    }
}
