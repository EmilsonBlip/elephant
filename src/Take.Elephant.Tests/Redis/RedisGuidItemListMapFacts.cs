﻿using AutoFixture;
using System;
using System.Threading.Tasks;
using Take.Elephant.Memory;
using Take.Elephant.Redis;
using Xunit;

namespace Take.Elephant.Tests.Redis
{
    [Trait("Category", nameof(Redis))]
    [Collection(nameof(Redis))]
    public class RedisGuidItemListMapFacts : GuidItemListMapFacts
    {
        private readonly RedisFixture _redisFixture;
        private readonly int _db;
        public const string MapName = "guid-list-items";

        public RedisGuidItemListMapFacts(RedisFixture redisFixture)
        {
            _redisFixture = redisFixture;
            _db = 1;
        }

        public override IMap<Guid, IPositionList<Item>> Create()
        {
            _redisFixture.Server.FlushDatabase(_db);
            var setMap = new RedisListMap<Guid, Item>(MapName, _redisFixture.Connection.Configuration, new ItemSerializer(), _db);
            return setMap;
        }

        public override IPositionList<Item> CreateValue(Guid key, bool populate)
        {
            var set = new List<Item>();
            if (populate)
            {
                set.AddAsync(Fixture.Create<Item>()).Wait();
                set.AddAsync(Fixture.Create<Item>()).Wait();
                set.AddAsync(Fixture.Create<Item>()).Wait();
            }
            return set;
        }

        public override Task AddExistingKeyConcurrentlyReturnsFalse()
        {
            // Not supported by this class
            return Task.CompletedTask;
        }
    }
}