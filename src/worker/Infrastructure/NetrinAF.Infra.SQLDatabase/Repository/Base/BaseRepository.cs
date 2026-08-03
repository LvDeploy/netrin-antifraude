using Microsoft.EntityFrameworkCore;
using NetrinAF.Domain.Contracts.Repositories.Base;
using NetrinAF.Domain.Entities.Base;
using NetrinAF.Infra.SQLDatabase.Context;

namespace NetrinAF.Infra.SQLDatabase.Repository.Base
{
    public class BaseRepository<T>(EFContext context) : IBaseRepository<T> where T : BaseEntity
    {
        public void Create(T entity)
        {
            context.Add(entity);
        }
        public void Update(T entity)
        {
            context.Update(entity);
        }

        public void Delete(T entity)
        {
            context.Remove(entity);
        }

        public async Task<T> Get(Guid id, CancellationToken cancellationToken)
        {
            return await context.Set<T>().SingleOrDefaultAsync(x => x.Id == id);
        }
    }
}
