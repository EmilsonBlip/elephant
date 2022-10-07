﻿using Take.Elephant.Sql;
using Take.Elephant.Sql.Mapping;

namespace Take.Elephant.Tests.Sql
{
    public abstract class SqlItemListFacts : ItemListFacts
    {
        private readonly ISqlFixture _serverFixture;

        protected SqlItemListFacts(ISqlFixture serverFixture)
        {
            _serverFixture = serverFixture;
        }

        public override IList<Item> Create()
        {
            var table = TableBuilder
                .WithName("ItemsSet")
                .WithKeyColumnsFromTypeProperties<Item>()
                .WithKeyColumnFromType<int>("Id", true)
                .WithSynchronizationStrategy(SchemaSynchronizationStrategy.UntilSuccess)
                .Build();
            _serverFixture.DropTable(table.Schema, table.Name);
            var mapper = new TypeMapper<Item>(table);
            return new SqlList<Item>(_serverFixture.DatabaseDriver, _serverFixture.ConnectionString, table, mapper);
        }
    }
}
