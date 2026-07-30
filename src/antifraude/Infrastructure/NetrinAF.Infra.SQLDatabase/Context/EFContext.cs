using Microsoft.EntityFrameworkCore;
using NetrinAF.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Infra.SQLDatabase.Context
{
    public class EFContext : DbContext
    {
        public EFContext(DbContextOptions<EFContext> options) : base(options)
        {
        
        }

        public DbSet<Transaction> Transactions {  get; set; }
        public DbSet<TransactionHistoric> TransactionHistorics {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EFContext).Assembly);
        }
    }
}
