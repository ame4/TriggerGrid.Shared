using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Data
{
    public interface IResponse
    {
        ErrorResponse? Error { get; }
    }

    public interface IResponse<out TResult> : IResponse
    {
        TResult Result { get; }
    }
}
