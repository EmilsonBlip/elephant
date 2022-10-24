﻿using System;
using System.Data;
using System.Reflection;
using Take.Elephant.Sql;
using Take.Elephant.Sql.Mapping;

namespace Take.Elephant.Tests.Sql
{    
    public abstract class SqlGuidItemKeyValueMapQueryableStorageFacts : GuidItemKeyValueMapQueryableStorageFacts
    {
        private readonly ISqlFixture _serverFixture;

        protected SqlGuidItemKeyValueMapQueryableStorageFacts(ISqlFixture serverFixture)
        {
            _serverFixture = serverFixture;
        }

        public override IMap<Guid, Item> Create()
        {
            var columns = typeof(Item)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToSqlColumns();
            columns.Add("Key", new SqlType(DbType.Guid));
            var table = new Table("GuidItems", new[] { "Key" }, columns, synchronizationStrategy: SchemaSynchronizationStrategy.UntilSuccess);
            _serverFixture.DropTable(table.Schema, table.Name);

            var keyMapper = new ValueMapper<Guid>("Key");
            var valueMapper = new TypeMapper<Item>(table);
            return new SqlMap<Guid, Item>(_serverFixture.DatabaseDriver, _serverFixture.ConnectionString, table, keyMapper, valueMapper);
        }
    }
}
