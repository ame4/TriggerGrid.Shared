using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.Interfaces
{
    public interface IAsyncSerializable
    {
        Task<ErrorResponse?> Write(AsyncBinaryWriter writer);
        Task<ErrorResponse?> Read(AsyncBinaryReader reader);
    }
}
