using NetrinAF.Application.Abstractions.Command;
using NetrinAF.Application.Abstractions.Response;

namespace NetrinAF.Application.Abstractions.Handler
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task<BaseResponse<Guid>> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand, TResponse>
       where TCommand : ICommand<TResponse>
    {
        Task<BaseResponse<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
    }
}
