using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Processing
{
    public class AsyncEvent
    {
        readonly object sync = new object();
        TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public AsyncEvent(bool initialState = false)
        {
            if (initialState) tcs.SetResult();
        }

        public Task Task
        {
            get
            {
                TaskCompletionSource tcs;
                lock (sync)
                {
                    tcs = this.tcs;
                }

                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource tcs;
            lock (sync)
            {
                tcs = this.tcs;
            }

            tcs.TrySetResult();
        }

        public void Reset()
        {
            lock(sync)
            {
                if (!this.tcs.Task.IsCompleted) return;
                this.tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
    }
}
