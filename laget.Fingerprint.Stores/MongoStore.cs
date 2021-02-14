using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace laget.Fingerprint.Stores
{

    public class MongoStore : IStore
    {
        private readonly IMongoCollection<Models.IFingerprint> _store;

        public MongoStore(MongoUrl url)
            : this(url, TimeSpan.FromHours(1), "calls")
        {
        }

        public MongoStore(MongoUrl url, TimeSpan ttl)
            : this(url, ttl, "calls")
        {
        }

        public MongoStore(MongoUrl url, TimeSpan ttl, string collection = "calls")
        {
            var client = new MongoClient(url);

            var database = client.GetDatabase(url.DatabaseName, new MongoDatabaseSettings
            {
                ReadConcern = ReadConcern.Default,
                ReadPreference = ReadPreference.SecondaryPreferred,
                WriteConcern = WriteConcern.Acknowledged
            });

            _store = database.GetCollection<Models.Fingerprint>(collection);

            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            var builder = Builders<Models.Fingerprint>.IndexKeys;
            var indexes = new List<CreateIndexModel<Models.Fingerprint>>
            {
                new CreateIndexModel<Models.Fingerprint>(builder.Ascending(_ => _.Hash), new CreateIndexOptions { Background = true, Unique = true })
            };
            _store.Indexes.CreateMany(indexes);
        }

        public void Add(Models.Fingerprint model)
        {
            throw new NotImplementedException();
        }

        public Models.Fingerprint Get(string hash)
        {
            throw new NotImplementedException();
        }

        public void Remove(string hash)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string hash)
        {
            throw new NotImplementedException();
        }
    }
}
