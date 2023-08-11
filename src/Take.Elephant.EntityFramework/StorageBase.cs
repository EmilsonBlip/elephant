using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Elephant.EntityFramework
{
    public abstract class StorageBase<TEntity> : IQueryableStorage<TEntity>, IOrderedQueryableStorage<TEntity>, IDistinctQueryableStorage<TEntity>
    {
        public StorageBase()
        {
        }

        public Task<QueryResult<TEntity>> QueryAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, int skip, int take, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<QueryResult<TEntity>> QueryAsync<TResult, TOrderBy>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, Expression<Func<TEntity, TOrderBy>> orderBy, bool orderByAscending, int skip, int take, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<QueryResult<TEntity>> QueryAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> select, bool distinct, int skip, int take, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}