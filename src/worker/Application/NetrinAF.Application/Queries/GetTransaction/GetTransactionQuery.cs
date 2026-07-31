using NetrinAF.Application.Abstractions.Query;
using NetrinAF.Application.Abstractions.Request;

namespace NetrinAF.Application.Query.GetTransaction
{
    public record GetTransactionQuery(Guid Id) : BaseRequest, IQuery<GetTransactionQueryResponse>
    {
        public override bool IsValid()
        {
            ValidationResult = new GetTransactionQueryValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
