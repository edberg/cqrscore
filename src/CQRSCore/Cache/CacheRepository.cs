﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using CQRSCore.Domain;
using CQRSCore.Events;
using Microsoft.Extensions.Caching.Memory;

namespace CQRSCore.Cache
{
    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly IMemoryCache _cache;
        private readonly Func<MemoryCacheEntryOptions> _policyFactory;
        private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public CacheRepository(IRepository repository, IEventStore eventStore, IMemoryCache cache)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            _repository = repository;
            _eventStore = eventStore;
            _cache = cache;

            _policyFactory = () => 
            {
                var option = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(new TimeSpan(0, 0, 15, 0));
                option.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration { EvictionCallback = (k, v, r, s) => 
                    {
                        object o;
                        _locks.TryRemove(k as string, out o);
                    } });
                return option;
            };
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var idstring = aggregate.Id.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
                        _cache.Set(idstring, aggregate, _policyFactory.Invoke());
                    _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var idstring = aggregateId.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    T aggregate;
                    if (IsTracked(aggregateId))
                    {
                        aggregate = (T) _cache.Get(idstring);
                        var events = _eventStore.Get<T>(aggregateId, aggregate.Version);
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
                            _cache.Remove(idstring);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return aggregate;
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId);
                    _cache.Set(aggregateId.ToString(), aggregate, _policyFactory.Invoke());
                    return aggregate;
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        private bool IsTracked(Guid id)
        {
            object o;
            return _cache.TryGetValue(id.ToString(), out o);
        }
    }
}