using FluentValidation;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("O Campo Value não pode ser vazio");
        }
    }
}
