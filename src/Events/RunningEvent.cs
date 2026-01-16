using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Interfaces;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.Events
{
    public class RunningEvent : IRunningEvent, IAsyncSerializable
    {
        public string Id { get; set; }
        public EventRequest Request { get; set; }

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            IResponse<string> id = await reader.ReadNotEmpty(nameof(Id));
            if(id.Error != null) return id.Error;
            Id = id.Result;
            EventRequest request = new EventRequest();
            ErrorResponse? requestError = await request.Read(reader);
            if(requestError == null) Request = request;
            return requestError;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            if(string.IsNullOrEmpty(Id)) return HttpStatusCode.BadRequest.Error($"{nameof(Id)} cannot be null or empty.");
            if(Request == null) return HttpStatusCode.BadRequest.Error($"{nameof(Request)} cannot be null.");   
            return await writer.Write(Id) ?? await Request.Write(writer);
        }
    }
}
