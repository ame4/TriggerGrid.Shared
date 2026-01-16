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
    public class EventsBatchResponse : IAsyncSerializable
    {
        public ErrorResponse?[] CreateEventResponses;
        public ErrorResponse?[] DeleteEventResponses;
        public ErrorResponse?[] ReportErrorResponses;

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            IResponse<ErrorResponse?[]> response = await ReadArray(reader);
            if(response.Error != null) return response.Error;
            CreateEventResponses = response.Result!;

            response = await ReadArray(reader);
            if (response.Error != null) return response.Error;
            DeleteEventResponses = response.Result!;

            response = await ReadArray(reader);
            if (response.Error != null) return response.Error;
            ReportErrorResponses = response.Result!;
            
            return null;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            if(CreateEventResponses == null || DeleteEventResponses == null || ReportErrorResponses == null)
                return HttpStatusCode.BadRequest.Error("Cannot write EventsBatchResponse with null arrays");
            return await writer.WriteArrayNullable(CreateEventResponses)
                ?? await writer.WriteArrayNullable(DeleteEventResponses)
                ?? await writer.WriteArrayNullable(ReportErrorResponses);
        }

        static async Task<IResponse<ErrorResponse?[]>> ReadArray(AsyncBinaryReader reader)
        {
            IResponse<ErrorResponse?[]?> response = await reader.ReadArrayNullable<ErrorResponse>();
            return response.Error == null && response.Result == null ? HttpStatusCode.UnprocessableContent.Error<ErrorResponse?[]>("Could not read responses") : response;
        }


    }
}
