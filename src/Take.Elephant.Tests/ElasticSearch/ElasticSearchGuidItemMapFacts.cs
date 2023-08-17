using System;
using Nest;
using Take.Elephant.ElasticSearch;
using Take.Elephant.ElasticSearch.Mapping;
using Take.Elephant.Tests.Search;
using Xunit;

namespace Take.Elephant.Tests.ElasticSearch
{
    [Trait("Category", nameof(Elasticsearch))]
    public class ElasticsearchGuidItemMapFacts : GuidItemMapFacts
    {
        public override IMap<Guid, Item> Create()
        {
            var mapping = MappingBuilder
                .WithIndex(Guid.NewGuid().ToString())
                .WithKeyField("id")
                .Build();

            var settings =
                new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                .DefaultIndex("tests");

            return new DelayedMapDecorator<Guid, Item>(
                    new ElasticsearchMap<Guid, Item>(
                        new ElasticClient(settings), mapping), 1000);
        }
    }
}