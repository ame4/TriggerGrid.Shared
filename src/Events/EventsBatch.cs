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
    public class EventsBatch : IAsyncSerializable
    {
        public EventRequest[] CreateEventRequests;
        public string[] DeleteIds;
        public ReportErrorRequest[] ReportErrorRequests;

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            Response<EventRequest[]?> createEventRequests = await reader.Read<EventRequest>();
            if (createEventRequests.Error != null) return createEventRequests.Error;
            if (createEventRequests.Result == null) return HttpStatusCode.BadRequest.Error($"{nameof(CreateEventRequests)} cannot be null");
            CreateEventRequests = createEventRequests.Result;

            Response<string?[]?> deleteIds = await reader.ReadStrings();
            if (deleteIds.Error != null) return deleteIds.Error;
            if (createEventRequests.Result == null) return HttpStatusCode.BadRequest.Error($"{nameof(DeleteIds)} cannot be null");
            foreach(string? id in deleteIds.Result) if(string.IsNullOrEmpty(id)) HttpStatusCode.BadRequest.Error("DeleteId cannot be empty");
            DeleteIds = deleteIds.Result;

            Response<ReportErrorRequest[]?> reportErrorRequests = await reader.Read<ReportErrorRequest>();
            if (reportErrorRequests.Error != null) return createEventRequests.Error;
            if (reportErrorRequests.Result == null) return HttpStatusCode.BadRequest.Error($"{nameof(ReportErrorRequests)} cannot be null");
            ReportErrorRequests = reportErrorRequests.Result;
            return null;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            if (CreateEventRequests == null) return HttpStatusCode.BadRequest.Error($"{nameof(CreateEventRequests)} cannot be null");
            if (DeleteIds == null) return HttpStatusCode.BadRequest.Error($"{nameof(DeleteIds)} cannot be null");
            if (ReportErrorRequests == null) return HttpStatusCode.BadRequest.Error($"{nameof(ReportErrorRequests)} cannot be null");
            return await writer.Write(CreateEventRequests) ?? await writer.Write(DeleteIds) ?? await writer.Write(ReportErrorRequests);
        }
    }
}
