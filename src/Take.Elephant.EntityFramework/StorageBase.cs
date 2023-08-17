using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Take.Elephant.EntityFramework
{
    public abstract class StorageBase<TEntity> : IQueryableStorage<TEntity>, IOrderedQueryableStorage<TEntity>, IDistinctQueryableStorage<TEntity> where TEntity : class
    {
        protected DbSet<TEntity> DbSet { get; }

        protected DbContext DbContext { get; }

        public StorageBase(DbContext DbContext, DbSet<TEntity> DbSet)
        {
            this.DbContext = DbContext ?? throw new ArgumentNullException(nameof(DbContext));
            this.DbSet = DbSet ?? throw new ArgumentNullException(nameof(DbSet));
        }

        public async Task<QueryResult<TEntity>> QueryAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, int skip, int take, CancellationToken cancellationToken)
        {
            var query = DbSet.AsQueryable();
            if (where != null)
            {
                query = query.Where(where);
            }
            if (select != null)
            {
                query = query.Select(select);
            }
            if (skip > 0)
            {
                query = query.Skip(skip);
            }
            if (take > 0)
            {
                query = query.Take(take);
            }
            var result = await query.ToListAsync(cancellationToken);
            return new QueryResult<TEntity>(result, result.Count);
        }

        public async Task<QueryResult<TEntity>> QueryAsync<TResult, TOrderBy>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, Expression<Func<TEntity, TOrderBy>> orderBy, bool orderByAscending, int skip, int take, CancellationToken cancellationToken) => throw new NotImplementedException();

        public async Task<QueryResult<TEntity>> QueryAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, bool distinct, int skip, int take, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}