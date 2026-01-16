using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;

namespace TriggerGrid.Shared.Processing
{
    public class ResponseLazyLoader<TData>
    {
        readonly object sync = new object();
        readonly Func<Task<IResponse<TData>>> loader;
        readonly TaskCreationOptions options;
        
        LazyLoader<IResponse<TData>> lazy;
        public ResponseLazyLoader(Func<Task<IResponse<TData>>> loader, TaskCreationOptions options)
        {
            this.loader = loader;
            this.options = options;
            lazy = new LazyLoader<IResponse<TData>>(loader, options);
        }

        public async Task<IResponse<TData>> Wait()
        {
            LazyLoader<IResponse<TData>> lazy;
            lock (sync)
            {
                lazy = this.lazy;
            }

            IResponse<TData> response = await lazy.Wait();
            if (response.Error == null) return response;
            lock(sync)
            {
                if (this.lazy == lazy) this.lazy = new LazyLoader<IResponse<TData>>(loader, options);
            }

            return response;
        }
    }
}
