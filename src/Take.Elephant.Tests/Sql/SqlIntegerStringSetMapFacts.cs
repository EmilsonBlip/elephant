﻿using System.Collections.Generic;
using System.Data;
using AutoFixture;
using Take.Elephant.Memory;
using Take.Elephant.Sql;
using Take.Elephant.Sql.Mapping;

namespace Take.Elephant.Tests.Sql
{
    public abstract class SqlIntegerStringSetMapFacts : IntegerStringSetMapFacts
    {
        private readonly ISqlFixture _serverFixture;

        protected SqlIntegerStringSetMapFacts(ISqlFixture serverFixture)
        {
            _serverFixture = serverFixture;
        }

        public override IMap<int, ISet<string>> Create()
        {
            var table = new Table(
                "IntegerStrings",
                new[] { "Key", "Value" },
                new Dictionary<string, SqlType>
                {
                    {"Key", new SqlType(DbType.Int32)},
                    {"Value", new SqlType(DbType.String)}
                }, synchronizationStrategy: SchemaSynchronizationStrategy.UntilSuccess);
            _serverFixture.DropTable(table.Schema, table.Name);
            var keyMapper = new ValueMapper<int>("Key");
            var valueMapper = new ValueMapper<string>("Value");
            return new SqlSetMap<int, string>(_serverFixture.DatabaseDriver, _serverFixture.ConnectionString, table, keyMapper, valueMapper);
        }

        public override ISet<string> CreateValue(int key, bool populate)
        {
            var set = new Set<string>();
            if (populate)
            {
                set.AddAsync(Fixture.Create<string>()).Wait();
                set.AddAsync(Fixture.Create<string>()).Wait();
                set.AddAsync(Fixture.Create<string>()).Wait();
            }
            return set;
        }
    }
}
