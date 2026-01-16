using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;

namespace TriggerGrid.Shared.Events
{
    public interface IDeadEvent : IRunningEvent
    {
        DateTimeOffset Timestamp { get; }
        ErrorResponse? Error { get; }
    }
}
