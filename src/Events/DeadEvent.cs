using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Interfaces;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.Events
{
    public class DeadEvent : IDeadEvent
    {
        readonly IRunningEvent runningEvent = new RunningEvent();
        public DeadEvent(IRunningEvent running) { this.runningEvent = running; }
        public string Id => runningEvent.Id;
        public EventRequest Request => runningEvent.Request;
        public DateTimeOffset Timestamp { get; set; }
        public ErrorResponse? Error { get; set;  }


        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
            => await runningEvent.Write(writer)
                ?? await writer.Write(Timestamp)
                ?? await writer.WriteNullable(Error);

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            ErrorResponse? error = await runningEvent.Read(reader);
            if (error != null) return error;
            IResponse<DateTimeOffset> timestamp = await reader.ReadDateTimeOffset();
            if(timestamp.Error != null) return timestamp.Error;
            Timestamp = timestamp.Result;
            IResponse<ErrorResponse?> errorResponse = await reader.ReadNullable<ErrorResponse>();
            Error = errorResponse.Result;
            return errorResponse.Error;
        }
    }
}
