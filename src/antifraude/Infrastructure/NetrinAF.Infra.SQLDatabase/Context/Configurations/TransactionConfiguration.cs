using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetrinAF.Domain.Entities;

namespace NetrinAF.Infra.SQLDatabase.Context.Configurations
{
    internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(transaction => transaction.Id);
        }
    }
}
