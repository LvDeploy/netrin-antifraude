using Microsoft.Extensions.Logging;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Domain.Entities;
using Serilog.Core;

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
                logger.LogInformation($"Inicio de processamento -> correlationid: {correlationId}, transactionId: {transactionId}, idempotencyKey: {idempotencyKey} ");
                var entityTransactions = await transactionRepository.GetByIdEmpotency(idempotencyKey);
                var entity = entityTransactions?.FirstOrDefault(x => x.Id == transactionId);
                if (entityTransactions is null || entity is null)
                {
                    logger.LogError($"transação não encontrada, IdEmpotency: {idempotencyKey}");
                    return;
                }

                if (entity.Status != Domain.Enums.TransactionStatus.REVIEW)
                {
                    logger.LogError($"transacao já processada");
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
                logger.LogInformation($"Processamento concluído -> correlationid: {correlationId}, transactionId: {idempotencyKey}");
            }
            catch (Exception ex) 
            {
                logger.LogError($"Erro no processamento -> correlationid: {correlationId}, transactionId: {idempotencyKey}");
                throw;
            }
        }
    }
}
