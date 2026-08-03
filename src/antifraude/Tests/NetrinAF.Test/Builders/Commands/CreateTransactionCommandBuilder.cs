using NetrinAF.Application.Commands.CreateTransaction;

namespace NetrinAF.Test.Builders.Commands;

public sealed class CreateTransactionCommandBuilder
{
    private decimal _value = 100m;

    public CreateTransactionCommandBuilder WithValue(decimal value)
    {
        _value = value;
        return this;
    }

    public CreateTransactionCommand Build() => new(_value);
}
