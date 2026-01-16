using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Interfaces
{
    public interface IResourceFactory<TResourceId, TResource>
    {
        Task<TResource> Create(TResourceId resourceId);
    }
}
