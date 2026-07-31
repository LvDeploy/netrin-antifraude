using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Enums;
using NetrinAF.Domain.ValueObjects;

namespace NetrinAF.Application.Query.GetTransaction
{
    public sealed class GetTransactionQueryHandler(ITransactionRepository transactionRepository)
        : IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse>
    {
        public async Task<BaseResponse<GetTransactionQueryResponse>> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid())
            {
                return ResponseBuilder.Failure<GetTransactionQueryResponse>(query.ValidationResult.Errors.Select(x => ErrorData.Set(x.ErrorMessage)), ErrorType.BadRequest);
            }

            var transaction = await transactionRepository.GetWithTrackLog(query.Id, cancellationToken);

            if (transaction is null)
            {
                return ResponseBuilder.Failure<GetTransactionQueryResponse>(ErrorData.Set("Transacao nao encontrada"), ErrorType.NotFound);
            }

            var trackLog = transaction.TrackLog
                .OrderByDescending(historic => historic.EventTime)
                .Select(historic => new GetTransactionQueryDetailResponse(
                    historic.EventTime,
                    historic.Status,
                    historic.StatusMessage));

            return ResponseBuilder.Success(new GetTransactionQueryResponse(
                transaction.Id,
                transaction.Status,
                transaction.Value,
                transaction.CreatedAt,
                trackLog));
        }
    }
}
