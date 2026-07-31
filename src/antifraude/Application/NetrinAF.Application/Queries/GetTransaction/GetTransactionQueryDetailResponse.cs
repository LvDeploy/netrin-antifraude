using NetrinAF.Domain.Enums;

namespace NetrinAF.Application.Query.GetTransaction
{
    public record GetTransactionQueryDetailResponse(
        DateTime EventTime,
        TransactionStatus Status,
        string StatusMessage);
}
