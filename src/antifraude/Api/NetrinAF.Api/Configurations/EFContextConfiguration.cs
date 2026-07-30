using Microsoft.EntityFrameworkCore;
using NetrinAF.Infra.SQLDatabase.Context;

namespace NetrinAF.Api.Configurations
{
    internal static class EFContextConfiguration
    {
        internal static void AddEFContextConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<EFContext>(opt => opt.UseSqlServer(builder.Configuration["ConnectionStrings:SqlServer"]));
        }
    }
}
