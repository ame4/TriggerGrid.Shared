using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Interfaces;

namespace TriggerGrid.Shared.Consumers
{
    public interface IConsumer : IAsyncSerializable
    {
        string ApplicationId { get; }
        string TenantId { get; }
    }
}
