using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Processing
{
    public class LockerCache<TKey> where TKey : notnull
    {
        readonly object sync = new object();
        readonly Dictionary<TKey, Locker<TKey>> locks = new Dictionary<TKey, Locker<TKey>>();
        
        
        public async Task<Locker<TKey>> Lock(params TKey[] keys)
        {
            Locker<TKey> locker = new Locker<TKey>(this);
            await locker.Lock(keys);
            return locker;
        } 
        internal bool Remove(List<TKey> keys)
        {
            lock (locks)
            {
                if (keys.Count == 0) return false;
                for (int i = 0; i < keys.Count; i++)
                    locks.Remove(keys[i]);
                keys.Clear();
            }

            return true;
        }

        internal async Task Lock(Locker<TKey> locker, TKey[] keys)
        {
            for (; ; )
            {
                Locker<TKey>? current = null;
                lock (sync)
                {
                    for (int i = 0; i < keys.Length; i++)
                        if (locks.TryGetValue(keys[i], out current) && current != locker)
                            break;

                    if (current == null || current == locker)
                    {
                        current = locker;
                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (locker.Add(keys[i])) locks.Add(keys[i], locker);
                        }
                    }
                }

                if (current == locker) break;
                await current.Task;
            }
        }
    }
}
