using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerGrid.Shared.Data
{
    public class Response : IResponse
    {
        public ErrorResponse? Error { get; }

        protected Response() { }
        protected Response(ErrorResponse error) { Error = error; }
    }

    public class Response<TResult> : Response, IResponse<TResult>
    {
        public TResult Result { get; }

        public Response() { }
        public Response(TResult? result) { Result = result; }
        public Response(ErrorResponse error) : base(error) { }
    }
}
