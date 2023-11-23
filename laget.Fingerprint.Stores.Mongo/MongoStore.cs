using laget.Fingerprint.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace laget.Fingerprint.Stores
{
    public class MongoStore<TFingerprint, TEntity> : IStore<TFingerprint> where TFingerprint : IFingerprint
    {
        private readonly IMongoCollection<TFingerprint> _collection;
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

            _collection = database.GetCollection<TFingerprint>($"{typeof(TEntity).Name.ToLower()}.fingerprints");
            _ttl = ttl;

            EnsureIndexes();
        }

        public void Add(TFingerprint model)
        {
            var options = new ReplaceOptions { IsUpsert = true };
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, model.Hash);

            _collection.ReplaceOne(filter, model, options);
        }

        public async Task AddAsync(TFingerprint model)
        {
            var options = new ReplaceOptions { IsUpsert = true };
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, model.Hash);

            await _collection.ReplaceOneAsync(filter, model, options);
        }

        public TFingerprint Get(string hash)
        {
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return _collection.Find(filter).FirstOrDefault();
        }

        public async Task<TFingerprint> GetAsync(string hash)
        {
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public void Remove(string hash)
        {
            var filter = Builders<TFingerprint>.Filter.Eq(x => x.Hash, hash);

            _collection.DeleteOne(filter);
        }

        public async Task RemoveAsync(string hash)
        {
            var filter = Builders<TFingerprint>.Filter.Eq(x => x.Hash, hash);

            await _collection.DeleteOneAsync(filter);
        }

        public bool Exists(string hash)
        {
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return _collection.FindSync(filter).Any();
        }

        public async Task<bool> ExistsAsync(string hash)
        {
            var builder = Builders<TFingerprint>.Filter;
            var filter = builder.Eq(x => x.Hash, hash);

            return await _collection.Find(filter).AnyAsync();
        }

        public IEnumerable<TFingerprint> Items => _collection.Find(_ => true).ToList();

        private void EnsureIndexes()
        {
            var builder = Builders<TFingerprint>.IndexKeys;
            var indexes = new List<CreateIndexModel<TFingerprint>>
            {
                new CreateIndexModel<TFingerprint>(builder.Ascending(_ => _.Hash), new CreateIndexOptions { Background = true, Unique = true })
            };

            if (_ttl != null)
            {
                indexes.Add(new CreateIndexModel<TFingerprint>(builder.Ascending(_ => _.CreatedAt), new CreateIndexOptions { Background = true, ExpireAfter = _ttl }));
            }

            _collection.Indexes.CreateMany(indexes);
        }
    }
}
