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
    public class EventRequest : IAsyncSerializable
    {
        public readonly EventHeader Header = new();
        public readonly EventBody Body = new();

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader) => await Header.Read(reader) ?? await Body.Read(reader);

        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer) => await Header.Write(writer) ?? await Body.Write(writer);
    }
}
