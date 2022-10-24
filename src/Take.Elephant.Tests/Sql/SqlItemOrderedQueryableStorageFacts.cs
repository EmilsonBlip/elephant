﻿using System.Threading.Tasks;
using Take.Elephant.Sql;
using Take.Elephant.Sql.Mapping;

namespace Take.Elephant.Tests.Sql
{
    public abstract class SqlItemOrderedQueryableStorageFacts : ItemOrderedQueryableStorageFacts
    {
        private readonly ISqlFixture _serverFixture;

        protected SqlItemOrderedQueryableStorageFacts(ISqlFixture serverFixture)
        {
            _serverFixture = serverFixture;
        }

        public override async Task<IOrderedQueryableStorage<Item>> CreateAsync(params Item[] values)
        {
            var table = TableBuilder
                .WithName("OrderedItemsSet")
                .WithKeyColumnsFromTypeProperties<Item>()
                .WithSynchronizationStrategy(SchemaSynchronizationStrategy.UntilSuccess)
                .Build();
            _serverFixture.DropTable(table.Schema, table.Name);
            var mapper = new TypeMapper<Item>(table);
            var set = new SqlSet<Item>(_serverFixture.DatabaseDriver, _serverFixture.ConnectionString, table, mapper);
            foreach (var value in values)
            {
                await set.AddAsync(value);
            }
            return set;
        }
    }
}
