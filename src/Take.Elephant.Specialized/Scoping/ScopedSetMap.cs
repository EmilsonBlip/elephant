using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Elephant.Specialized.Scoping
{
    public class ScopedSetMap<TKey, TItem> : ScopedMap<TKey, ISet<TItem>>, ISetMap<TKey, TItem>, IItemSetMap<TKey, TItem>, IQueryableStorage<TItem>, IQueryableStorage<KeyValuePair<TKey, TItem>>, IKeyQueryableMap<TKey, TItem>
    {
        private readonly MapScope _scope;
        private readonly ISerializer<TKey> _keySerializer;
        private readonly bool _removeOnEmptySet;

        public ScopedSetMap(ISetMap<TKey, TItem> map, IScope scope, string identifier, ISerializer<TKey> keySerializer, bool removeOnEmptySet = false) 
            : base(map, scope, identifier, keySerializer)
        {
            _scope = (MapScope)scope;
            _keySerializer = keySerializer;
            _removeOnEmptySet = removeOnEmptySet;
        }

        public override async Task<ISet<TItem>> GetValueOrDefaultAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var items = await base.GetValueOrDefaultAsync(key).ConfigureAwait(false);
            if (items == null) return null;
            return _removeOnEmptySet ? new SetWrapper(key, items, _scope, Identifier, _keySerializer) : items;
        }

        public virtual async Task<ISet<TItem>> GetValueOrEmptyAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var setMap = (ISetMap<TKey, TItem>) Map;
            var items = await setMap.GetValueOrEmptyAsync(key);            
            await Scope.AddKeyAsync(Identifier, KeySerializer.Serialize(key)).ConfigureAwait(false);            
            return _removeOnEmptySet ? new SetWrapper(key, items, _scope, Identifier, _keySerializer) : items;
        }

        public virtual Task<TItem> GetItemOrDefaultAsync(TKey key, TItem item) =>
            CastMapOrThrow<IItemSetMap<TKey, TItem>>().GetItemOrDefaultAsync(key, item);

        public virtual Task<QueryResult<TItem>> QueryAsync<TResult>(Expression<Func<TItem, bool>> @where, Expression<Func<TItem, TResult>> @select, int skip, int take, CancellationToken cancellationToken) =>
            CastMapOrThrow<IQueryableStorage<TItem>>().QueryAsync(@where, @select, skip, take, cancellationToken);

        public virtual Task<QueryResult<KeyValuePair<TKey, TItem>>> QueryAsync<TResult>(Expression<Func<KeyValuePair<TKey, TItem>, bool>> @where, Expression<Func<KeyValuePair<TKey, TItem>, TResult>> @select, int skip, int take, CancellationToken cancellationToken) =>
            CastMapOrThrow<IQueryableStorage<KeyValuePair<TKey, TItem>>>().QueryAsync(@where, @select, skip, take, cancellationToken);

        public virtual Task<QueryResult<TKey>> QueryForKeysAsync<TResult>(Expression<Func<TItem, bool>> @where, Expression<Func<TKey, TResult>> @select, int skip, int take, CancellationToken cancellationToken) =>
            CastMapOrThrow<IKeyQueryableMap<TKey, TItem>>().QueryForKeysAsync(@where, @select, skip, take, cancellationToken);

        protected class SetWrapper : ISet<TItem>
        {
            private readonly TKey _key;
            private readonly ISet<TItem> _set;
            private readonly MapScope _scope;
            private readonly string _identifier;
            private readonly ISerializer<TKey> _keySerializer;

            public SetWrapper(TKey key, ISet<TItem> set, MapScope scope, string identifier, ISerializer<TKey> keySerializer)
            {
                _key = key;
                _set = set;
                _scope = scope;
                _identifier = identifier;
                _keySerializer = keySerializer;
            }

            public virtual IAsyncEnumerable<TItem> AsEnumerableAsync(CancellationToken cancellationToken = default) =>
                _set.AsEnumerableAsync(cancellationToken);

            public virtual Task<long> GetLengthAsync(CancellationToken cancellationToken = default) => 
                _set.GetLengthAsync(cancellationToken);

            public virtual Task AddAsync(TItem value, CancellationToken cancellationToken = default) => 
                _set.AddAsync(value, cancellationToken);

            public virtual async Task<bool> TryRemoveAsync(TItem value, CancellationToken cancellationToken = default)
            {
                if (!await _set.TryRemoveAsync(value, cancellationToken).ConfigureAwait(false)) return false;
                if (await GetLengthAsync(cancellationToken) == 0)
                {
                    await _scope.RemoveKeyAsync(_identifier, _keySerializer.Serialize(_key)).ConfigureAwait(false);
                }
                return true;
            }

            public virtual Task<bool> ContainsAsync(TItem value, CancellationToken cancellationToken = default) => 
                _set.ContainsAsync(value, cancellationToken);
        }
    }
}