using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Interfaces
{
    public interface IPage
    {
        string? ContinuationToken { get; }
    }

    public interface IPage<out T> : IPage, IReadOnlyCollection<T>
    {
    }
}
