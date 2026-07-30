using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public sealed class CreateTransactionCommandHandler() : ICommandHandler<CreateTransactionCommand>
    {
        public async Task<BaseResponse<Guid>> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            return  ResponseBuilder.Success(Guid.Empty);
        }
    }
}
