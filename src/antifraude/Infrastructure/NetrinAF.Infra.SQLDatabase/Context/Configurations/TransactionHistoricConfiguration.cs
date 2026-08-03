using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetrinAF.Domain.Entities;

namespace NetrinAF.Infra.SQLDatabase.Context.Configurations
{
    internal sealed class TransactionHistoricConfiguration : IEntityTypeConfiguration<TransactionHistoric>
    {
        public void Configure(EntityTypeBuilder<TransactionHistoric> builder)
        {
            builder.HasKey(transaction => transaction.Id);

            builder
                .HasOne(transaction => transaction.Transaction)
                .WithMany(transaction => transaction.TrackLog)
                .HasForeignKey(transaction => transaction.TransactionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
