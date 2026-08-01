using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Application.Services.TransactionProcess
{
    public interface ITransactionProcessService
    {
        Task Process(Guid transactionId, string idEmpotencykey, string correlationId);
    }
}
