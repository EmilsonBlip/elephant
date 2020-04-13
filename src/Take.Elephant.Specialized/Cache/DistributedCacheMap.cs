using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Take.Elephant.Memory;

namespace Take.Elephant.Specialized.Cache
{
    /// <summary>
    /// Implements a <see cref="IMap{TKey,TValue}"/> that cache the values on demand in memory, using a bus to invalidate the cache between instances. 
    /// </summary>
    public class DistributedCacheMap<TKey, TValue> : OnDemandCacheMap<TKey, TValue>
    {
        private readonly IBus<string, SynchronizationEvent<TKey>> _synchronizationBus;
        private readonly string _synchronizationChannel;
        private readonly Guid _instance;
        private readonly Task _subscriptionTask;

        public DistributedCacheMap(
            IMap<TKey, TValue> source,
            IBus<string, SynchronizationEvent<TKey>> synchronizationBus,
            string synchronizationChannel,
            TimeSpan cacheExpiration = default,
            TimeSpan cacheFaultTolerance = default)
            : base(
                source,
                new Map<TKey, TValue>(),
                cacheExpiration,
                cacheFaultTolerance)
        {
            _synchronizationBus = synchronizationBus;
            _synchronizationChannel = synchronizationChannel;
            _instance = Guid.NewGuid();
            _subscriptionTask = _synchronizationBus.SubscribeAsync(_synchronizationChannel, HandleEventAsync, CancellationToken.None);
        }
        
        public override async Task<bool> TryAddAsync(TKey key, TValue value, bool overwrite = false, CancellationToken cancellationToken = new CancellationToken())
        {
            await EnsureSubscribedAsync(cancellationToken);
            
            if (await base.TryAddAsync(key, value, overwrite, cancellationToken))
            {
                await PublishEventAsync(key, cancellationToken);
                return true;
            }

            return false;
        }
        
        public override async Task<bool> TryRemoveAsync(TKey key, CancellationToken cancellationToken = new CancellationToken())
        {
            await EnsureSubscribedAsync(cancellationToken);

            if (await base.TryRemoveAsync(key, cancellationToken))
            {
                await PublishEventAsync(key, cancellationToken);
                return true;
            }

            return false;
        }

        public override async Task MergeAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            await EnsureSubscribedAsync(cancellationToken);
            
            await base.MergeAsync(key, value, cancellationToken);
            await PublishEventAsync(key, cancellationToken);
        }

        public override async Task SetPropertyValueAsync<TProperty>(TKey key, string propertyName,
            TProperty propertyValue, CancellationToken cancellationToken = default)
        {
            await EnsureSubscribedAsync(cancellationToken);
            
            await base.SetPropertyValueAsync(key, propertyName, propertyValue, cancellationToken);
            await PublishEventAsync(key, cancellationToken);
        }
        
        private async Task EnsureSubscribedAsync(CancellationToken cancellationToken = default)
        {
            if (!_subscriptionTask.IsCompleted)
            {
                var tcs = new TaskCompletionSource<object>();
                await using var _ = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
                await Task.WhenAny(_subscriptionTask, tcs.Task);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        
        private async Task HandleEventAsync(string synchronizationChannel, SynchronizationEvent<TKey> @event, CancellationToken cancellationToken)
        {
            // Ignore events generated by the current instance.
            if (@event.Instance == _instance) return;

            // Remove from the cache either if it is a new key (which will force the value to be reloaded from the source) or if it was removed. 
            await Cache.TryRemoveAsync(@event.Key, cancellationToken);
        }
        
        private async Task PublishEventAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _synchronizationBus.PublishAsync(
                _synchronizationChannel,
                new SynchronizationEvent<TKey>()
                {
                    Key = key,
                    Instance = _instance
                },
                cancellationToken);
        }
    }
    
    [DataContract]
    public class SynchronizationEvent<TKey>
    {
        [DataMember]
        public TKey Key { get; set; }

        [DataMember]
        public Guid Instance { get; set; }
    }
}