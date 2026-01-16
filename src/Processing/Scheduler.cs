using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Processing
{
    public abstract class Scheduler
    {
        readonly object sync = new object();
        protected abstract Task<bool> Process();
        protected virtual void Exit() { }
        readonly AsyncEvent wakeup = new AsyncEvent(), stopped = new AsyncEvent(), stopping = new AsyncEvent();
        volatile bool stop;
        bool started;

        public void Wakeup() => wakeup.Set();
        protected bool IsStopped => stop;
        public Task Stop()
        {
            stop = true;
            stopping.Set();
            lock (sync)
            {
                if(!started) return Task.CompletedTask;
            }

            Wakeup();
            return stopped.Task;
        }
        protected Task Stopping => stopping.Task;

        public async void Start()
        {
            lock(sync)
            {
                started = true;
                if (stop)
                {
                    started = false;
                    return;
                }
            }

            while (!stop)
            {
                wakeup.Reset();
                while (!stop && await Process());
                if (stop) break;
                await wakeup.Task;
            }

            Exit();

            stopped.Set();
        }
    }
}
