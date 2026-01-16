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
    public class EventBody : IAsyncSerializable
    {
        public string EventName;
        public string? Text;
        public byte[]? Binary;

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            Response<string?> text = await reader.ReadString();
            if (text.Error != null) return text.Error;
            if (string.IsNullOrEmpty(text.Result)) HttpStatusCode.UnprocessableContent.Error("Empty EventName");
            EventName = text.Result;
            text = await reader.ReadString();
            if(text.Error != null) return text.Error;
            Text = text.Result;
            Response<byte[]?> body = await reader.ReadBytes();
            Binary = body.Result;
            return body.Error;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer) {
            if (string.IsNullOrEmpty(EventName)) return HttpStatusCode.InternalServerError.Error("Empty EventName");
            return await writer.Write(EventName) ?? await writer.Write(Text) ?? await writer.Write(Binary);
        }
    }
}
