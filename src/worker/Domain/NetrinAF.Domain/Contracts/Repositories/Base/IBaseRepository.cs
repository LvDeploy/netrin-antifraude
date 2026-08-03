using NetrinAF.Domain.Entities.Base;

namespace NetrinAF.Domain.Contracts.Repositories.Base
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T> Get(Guid id, CancellationToken cancellationToken);
    }
}
