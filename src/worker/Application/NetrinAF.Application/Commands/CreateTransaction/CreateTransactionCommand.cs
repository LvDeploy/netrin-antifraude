using NetrinAF.Application.Abstractions.Command;
using NetrinAF.Application.Abstractions.Request;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public record CreateTransactionCommand(decimal Value) : BaseRequest, ICommand
    {
        public override bool IsValid() 
        {
            ValidationResult = new CreateTransactionCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
