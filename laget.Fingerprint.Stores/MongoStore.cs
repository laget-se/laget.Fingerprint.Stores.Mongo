using System;
using System.Collections.Generic;
using laget.Fingerprint.Interfaces;
using MongoDB.Driver;

namespace laget.Fingerprint.Stores
{
    public class MongoStore<T> : IStore where T : IFingerprintable
    {
        private readonly IMongoCollection<IFingerprint> _collection;
        private readonly TimeSpan? _ttl;

        public MongoStore(MongoUrl url)
            : this(url, null)
        {
        }

        public MongoStore(MongoUrl url, TimeSpan? ttl)
        {
            var client = new MongoClient(url);

            var database = client.GetDatabase(url.DatabaseName, new MongoDatabaseSettings
            {
                ReadConcern = ReadConcern.Default,
                ReadPreference = ReadPreference.SecondaryPreferred,
                WriteConcern = WriteConcern.Acknowledged
            });

            _collection = database.GetCollection<IFingerprint>($"{typeof(T).Name.ToLower()}.fingerprints");
            _ttl = ttl;

            EnsureIndexes();
        }

        public void Add(IFingerprint model)
        {
            var options = new ReplaceOptions { IsUpsert = true };
            var builder = Builders<IFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, model.Hash);

            _collection.ReplaceOne(filter, model, options);
        }

        public IFingerprint Get(string hash)
        {
            var builder = Builders<IFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return _collection.Find(filter).FirstOrDefault();
        }

        public void Remove(string hash)
        {
            var filter = Builders<IFingerprint>.Filter.Eq(x => x.Hash, hash);

            _collection.DeleteOne(filter);
        }

        public bool Exists(string hash)
        {
            var builder = Builders<IFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return _collection.FindSync(filter).Any();
        }

        public IEnumerable<IFingerprint> Items => _collection.Find(_ => true).ToList();

        private void EnsureIndexes()
        {
            var builder = Builders<IFingerprint>.IndexKeys;
            var indexes = new List<CreateIndexModel<IFingerprint>>
            {
                new CreateIndexModel<IFingerprint>(builder.Ascending(_ => _.Hash), new CreateIndexOptions { Background = true, Unique = true })
            };

            if(_ttl != null)
            {
                indexes.Add(new CreateIndexModel<IFingerprint>(builder.Ascending(_ => _.CreatedAt), new CreateIndexOptions { Background = true, ExpireAfter = _ttl }));
            }

            _collection.Indexes.CreateMany(indexes);
        }
    }
}
