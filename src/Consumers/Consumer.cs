using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.Consumers
{
    public class Consumer : IConsumer
    {
        public string ApplicationId { get; set; }

        public string TenantId { get; set; }

        public async Task<ErrorResponse?> Read(AsyncBinaryReader reader)
        {
            IResponse<string> text = await Read(reader, nameof(TenantId));
            if (text.Error != null) return text.Error;
            TenantId = text.Result;

            text = await Read(reader, nameof(ApplicationId));
            if (text.Error != null) return text.Error;
            ApplicationId = text.Result;
            return null;
        }


        public async Task<ErrorResponse?> Write(AsyncBinaryWriter writer)
            => await Write(writer, nameof(TenantId), TenantId)
                ?? await Write(writer, nameof(ApplicationId), ApplicationId);

        static Task<ErrorResponse?> Write(AsyncBinaryWriter writer, string attributeName, string value)
        {
            if (string.IsNullOrEmpty(value)) return Task.FromResult(HttpStatusCode.InternalServerError.Error($"Empty {attributeName}"));
            if(value.Contains('_')) return Task.FromResult(HttpStatusCode.InternalServerError.Error($"Invalid character '_' in {attributeName}: {value}"));
            return writer.Write(value);
        }

        static async Task<IResponse<string>> Read(AsyncBinaryReader reader, string attributeName)
        {
            IResponse<string?> text = await reader.ReadString();
            if (text.Error != null) return text.Error.Error<string>();
            if (string.IsNullOrEmpty(text.Result)) return HttpStatusCode.UnprocessableContent.Error<string>($"Empty {attributeName} is not expected");
            if (text.Result.Contains("_")) return HttpStatusCode.UnprocessableContent.Error<string>($"Invalid '_' charachter in {attributeName} : {text.Result}");
            return text;
        }
    }
}
