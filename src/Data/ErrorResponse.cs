using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Interfaces;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.Data
{
    public class ErrorResponse : IAsyncSerializable
    {
        public ErrorResponse() { }
        internal ErrorResponse(HttpStatusCode statusCode, string message, string? details)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public HttpStatusCode StatusCode { get; set;  }
        public string Message { get; set; }
        public string? Details { get; set; }

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            Response<int> statusCode = await reader.ReadInt32();
            if (statusCode.Error != null) return statusCode.Error;
            StatusCode = (HttpStatusCode)statusCode.Result;
            Response<string?> message = await reader.ReadString();
            if (message.Error != null) return message.Error;
            Message = message.Result;
            message = await reader.ReadString();
            if (message.Error != null) return message.Error;
            Details = message.Result;
            return null;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            ErrorResponse? error = await writer.Write((int)StatusCode);
            if (error != null) return error;
            error = await writer.Write(Message);
            return error == null ? await writer.Write(Details) : error;
        }
    }
}
