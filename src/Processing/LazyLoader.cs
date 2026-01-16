using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Processing
{
    public class LazyLoader<TData>
    {
        readonly object sync = new object();
        readonly Func<Task<TData>> loader;
        readonly TaskCreationOptions options;

        Task<TData>? task;
        public LazyLoader(Func<Task<TData>> loader, TaskCreationOptions options) { 
            this.loader = loader;
            this.options = options;
        }

        public Task<TData> Wait()
        {
            TaskCompletionSource<TData> tcs = new TaskCompletionSource<TData>(options);
            lock(sync)
            {
                if (task != null) return task;
                task = tcs.Task;
            }

            Task tsk = Load(tcs);
            return tcs.Task;
        }

        async Task Load(TaskCompletionSource<TData> tcs)
        {
            try
            {
                tcs.SetResult(await loader());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                Task task = tcs.Task;
                lock (sync)
                {
                    if (this.task == task) this.task = null;
                }
            }
        }
    }
}
