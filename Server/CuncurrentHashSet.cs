using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class CuncurrentHashSet<T> where T : class
    {
        private readonly ConcurrentDictionary<T, byte> dictionary = new ConcurrentDictionary<T, byte>();

        public bool Add(T item)
        {
            return dictionary.TryAdd(item, 0);
        }

        public bool Remove(T item)
        {
            return dictionary.TryRemove(item, out _);
        }

        public IEnumerable<T> Items
        {
            get { return dictionary.Keys; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}
