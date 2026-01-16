using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Interfaces;

namespace TriggerGrid.Shared.Collections
{
    public class Page : IPage
    {
        public string? ContinuationToken { get; set; }
    }

    public class Page<TItem> : Page, IPage<TItem>
    {
        readonly TItem[] rows;

        public Page(TItem[] rows)
        {
            this.rows = rows;
        }

        public int Count => rows.Length;

        public IEnumerator<TItem> GetEnumerator()
        {
            IEnumerable<TItem> enumerable = rows;
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => rows.GetEnumerator();
    }
}
