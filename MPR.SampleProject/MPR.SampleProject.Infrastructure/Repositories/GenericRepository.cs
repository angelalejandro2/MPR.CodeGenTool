using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MPR.SampleProject.Domain.Repositories;

namespace MPR.SampleProject.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;

        public GenericRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<TEntity?> GetByIdAsync(object key, CancellationToken cancellationToken)
        => await _context.Set<TEntity>().FindAsync(new object[] { key }, cancellationToken);
    
        public async Task<TEntity?> GetByIdAsync(object[] keys, CancellationToken cancellationToken)
        => await _context.Set<TEntity>().FindAsync(keys, cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Set<TEntity>().ToListAsync(cancellationToken);

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _context.Set<TEntity>().AddAsync(entity, cancellationToken);

        public void Update(TEntity entity)
            => _context.Set<TEntity>().Update(entity);

        public void Remove(TEntity entity)
            => _context.Set<TEntity>().Remove(entity);
    }
}