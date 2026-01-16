using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Collections
{
    public class ListSection<T> : IReadOnlyList<T>
    {
        readonly IReadOnlyList<T> source;
        readonly int start, count;
        public ListSection(IReadOnlyList<T> source, int start, int count)
        {
            this.start = start;
            this.count = count;
            this.source = source;
            if (start < 0) throw new ArgumentException($"Invalid start {start}");
            if(count < 0 || start + count > source.Count) throw new ArgumentException($"Invalid count {count}");
        }

        public T this[int index] => source[start + index];

        public int Count => count;

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        class Enumerator : IEnumerator<T>
        {
            readonly ListSection<T> list;
            int index = -1;
            internal Enumerator(ListSection<T> list)
            {
                this.list = list;
            }

            public T Current => list[index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if(index >= list.Count - 1) return false;
                index++;
                return true;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}
