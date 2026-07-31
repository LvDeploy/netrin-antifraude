using NetrinAF.Domain.Enums;

namespace NetrinAF.Application.Query.GetTransaction
{
    public record GetTransactionQueryResponse(
        Guid TransactionId,
        TransactionStatus Status,
        decimal Value,
        DateTime CreatedAt,
        IEnumerable<GetTransactionQueryDetailResponse> TrackLog);
}
