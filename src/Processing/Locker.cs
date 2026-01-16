using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Processing;

namespace TriggerGrid.Shared.Processing
{
    public class Locker<TKey> : IDisposable
        where TKey : notnull
    {
        readonly LockerCache<TKey> cache;
        readonly List<TKey> keys = new List<TKey>();
        readonly TaskCompletionSource tcs = new TaskCompletionSource();

        internal Locker(LockerCache<TKey> cache)
        {
            this.cache = cache;
        }

        internal Task Task => tcs.Task;

        public void Dispose() {
            if(cache.Remove(keys)) tcs.SetResult();
        }

        public Task Lock(params TKey[] keys) => cache.Lock(this, keys);
        internal bool Add(TKey key)
        {
            if (keys.Contains(key)) return false;
            keys.Add(key);
            return true;
        }
    }


    public class Locker<TOwner, TKey> : Locker<TKey> where TKey : notnull
    {
        readonly static LockerCache<TKey> cache = new LockerCache<TKey>();

        public Locker() : base(cache) { }
    }
}       
