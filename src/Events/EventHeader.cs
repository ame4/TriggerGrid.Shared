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
    public class EventHeader : IAsyncSerializable
    {
        public string Target;
        public byte PriorityLevel;
        public byte Priority;

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            IResponse<string> target = await reader.ReadNotEmpty(nameof(Target));
            if (target.Error != null) return target.Error;
            Target = target.Result;

            Response<byte> b = await reader.ReadByte();
            if (b.Error != null) return b.Error;
            PriorityLevel = b.Result;

            b = await reader.ReadByte();
            Priority = b.Result;
            return b.Error;
        }

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Target)) return HttpStatusCode.BadRequest.Error("Empty Target");
            return await writer.Write(Target) ?? await writer.Write(PriorityLevel) ?? await writer.Write(Priority);
        }
    }
}
