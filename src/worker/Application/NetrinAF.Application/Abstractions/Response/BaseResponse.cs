using NetrinAF.Domain.Enums;
using NetrinAF.Domain.ValueObjects;

namespace NetrinAF.Application.Abstractions.Response
{
    public class BaseResponse<T> 
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public IEnumerable<ErrorData>? Errors { get; init; }
        public string? Message { get; init; }
        public ErrorType ErrorType { get; init; } = ErrorType.BadRequest;
    }
}
