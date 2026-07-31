using FluentValidation;

namespace NetrinAF.Application.Query.GetTransaction
{
    public class GetTransactionQueryValidator : AbstractValidator<GetTransactionQuery>
    {
        public GetTransactionQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("O Campo Id nao pode ser vazio");
        }
    }
}
