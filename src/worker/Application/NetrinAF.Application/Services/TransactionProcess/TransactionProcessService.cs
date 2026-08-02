using Microsoft.Extensions.Logging;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Domain.Entities;

namespace NetrinAF.Application.Services.TransactionProcess
{
    public class TransactionProcessService(
        ITransactionHistoricRepository transactionHistoricRepository,
        ITransactionRepository transactionRepository,
        ILogger<TransactionProcessService> logger,
        IUnitOfWork unitOfWork)  : ITransactionProcessService
    {
        public async Task Process(Guid transactionId, string idempotencyKey, string correlationId)
        {
            try
            {
                logger.LogInformation(
                    "Transaction processing started. CorrelationId: {CorrelationId}; TransactionId: {TransactionId}; IdempotencyKey: {IdempotencyKey}",
                    correlationId,
                    transactionId,
                    idempotencyKey);
                var entityTransactions = await transactionRepository.GetByIdEmpotency(idempotencyKey);
                var entity = entityTransactions?.FirstOrDefault(x => x.Id == transactionId);
                if (entityTransactions is null || entity is null)
                {
                    logger.LogError(
                        "Transaction was not found. IdempotencyKey: {IdempotencyKey}; TransactionId: {TransactionId}",
                        idempotencyKey,
                        transactionId);
                    return;
                }

                if (entity.Status != Domain.Enums.TransactionStatus.REVIEW)
                {
                    logger.LogError(
                        "Transaction was already processed. TransactionId: {TransactionId}; Status: {TransactionStatus}",
                        transactionId,
                        entity.Status);
                    return;
                }
                var failedTransaction = entityTransactions.ToList();
                failedTransaction.Remove(entity);

                entity.CheckTransaction();
                var entityTransactionHistoric = TransactionHistoric.CreateNew(entity, 
                    entity.IdEmpotency, 
                    entity.Status == Domain.Enums.TransactionStatus.APPROVED ? "Transação Aceita" : "Transação Rejeitada");
                transactionRepository.Update(entity);
                transactionHistoricRepository.Create(entityTransactionHistoric);

                foreach(var item in failedTransaction)
                {
                    item!.Reject();
                    var itemTransactionHistoric = TransactionHistoric.CreateNew(entity, entity.IdEmpotency, "Transação Rejeitada por duplicidade");
                    transactionHistoricRepository.Create(entityTransactionHistoric);
                }

                await unitOfWork.CommitAsync(default);
                logger.LogInformation(
                    "Transaction processing completed. CorrelationId: {CorrelationId}; TransactionId: {TransactionId}; TransactionStatus: {TransactionStatus}",
                    correlationId,
                    transactionId,
                    entity.Status);
            }
            catch (Exception ex) 
            {
                logger.LogError(
                    ex,
                    "Transaction processing failed. CorrelationId: {CorrelationId}; TransactionId: {TransactionId}",
                    correlationId,
                    transactionId);
                throw;
            }
        }
    }
}
