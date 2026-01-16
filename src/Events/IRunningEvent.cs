using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Interfaces;

namespace TriggerGrid.Shared.Events
{
    public interface IRunningEvent : IAsyncSerializable
    {
        string Id { get; }
        EventRequest Request { get; }

    }
}
