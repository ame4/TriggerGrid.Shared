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
    public class ReportErrorRequest : IAsyncSerializable
    {
        public string Id;
        public ErrorResponse? Error;

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            Response<string?> id = await reader.ReadString();
            if(id.Error != null) return id.Error;
            if (string.IsNullOrEmpty(id.Result)) return HttpStatusCode.BadRequest.Error("Report error request does not contain Id");
            Id = id.Result;
            Response<bool> b = await reader.ReadBoolean();
            if(b.Error != null) return b.Error;
            if (!b.Result) return null;
            Error = new ErrorResponse();
            return await Error.Read(reader);
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Id)) return HttpStatusCode.InternalServerError.Error("Report error request does not contain Id");
            return await writer.Write(Id) ?? await writer.Write(Error != null)
                ?? (Error != null ? await Error.Write(writer) : null);
        }
    }
}
