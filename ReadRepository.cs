using Microsoft.EntityFrameworkCore;

public interface IReadRepository<T>
    where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IList<T>> GetAllAsync();
}
