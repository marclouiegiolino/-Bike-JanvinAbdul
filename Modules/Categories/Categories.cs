using System;
using System.Text.Json.Serialization;

namespace Api.Modules.Categories
{
    public sealed class Categories
    {
        private long? _categoryId;
        private string? _categoryName = string.Empty;

        public Categories() { }

        public Categories(long categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }

        [JsonPropertyName("category_id")]
        [JsonPropertyOrder(1)]
        public long CategoryId
        {
            get => _categoryId ?? 0L;
            set => _categoryId = value;
        }

        [JsonPropertyName("category_name")]
        [JsonPropertyOrder(2)]
        public string CategoryName
        {
            get => _categoryName ?? string.Empty;
            set => _categoryName = NormalizeSpaces(value);
        }

        private static string NormalizeSpaces(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}