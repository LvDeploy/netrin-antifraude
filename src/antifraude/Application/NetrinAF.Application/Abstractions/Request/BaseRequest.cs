using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace NetrinAF.Application.Abstractions.Request
{
    public abstract record BaseRequest
    {
        [JsonIgnore]
        public ValidationResult ValidationResult { get; set; } = new();

        public abstract bool IsValid();
    }
}
