using NetrinAF.Domain.Enums;
using NetrinAF.Domain.ValueObjects;

namespace NetrinAF.Application.Abstractions.Response
{
    public static class ResponseBuilder
    {
        public static BaseResponse<T> Success<T>(T? data, string? message = null)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static BaseResponse<T> Failure<T>(ErrorData error, ErrorType errorType = ErrorType.BadRequest)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Errors = error is not null ? new[] { error } : Array.Empty<ErrorData>(),
                ErrorType = errorType
            };
        }

        public static BaseResponse<T> Failure<T>(IEnumerable<ErrorData>? errors, ErrorType errorType = ErrorType.BadRequest)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Errors = errors?.ToArray() ?? Array.Empty<ErrorData>(),
                ErrorType = errorType
            };
        }
    }
}
