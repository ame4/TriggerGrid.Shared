using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Processing
{
    class Reference
    {
        int count = 1;
        internal void AddRef() => count++;
        internal int Release() {
            if (count <= 0) throw new ObjectDisposedException("invalid reference counter");
            return count;
        }
    }

    class Reference<TValue> : Reference
    {
        internal readonly TValue Value;
        internal Reference(TValue value) => Value = value;
    }
}
