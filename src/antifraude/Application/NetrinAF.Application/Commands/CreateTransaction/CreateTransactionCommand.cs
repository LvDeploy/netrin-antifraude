using NetrinAF.Application.Abstractions.Command;
using NetrinAF.Application.Abstractions.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public record CreateTransactionCommand : BaseRequest, ICommand
    {
        public override bool IsValid() => true;
    }
}
