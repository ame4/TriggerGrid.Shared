using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;

namespace TriggerGrid.Shared.Serializing
{
    public class AsyncBinaryWriter
    {
        readonly byte[] b = new byte[sizeof(double)];
        readonly Stream stream;
        readonly CancellationToken cancellationToken;

        public AsyncBinaryWriter(Stream stream, CancellationToken cancellationToken)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.cancellationToken = cancellationToken;
        }

        public async Task<ErrorResponse?> Write(byte[]? data)
        {
            if (data == null) return await Write(-1);
            ErrorResponse? error = await Write(data.Length);
            return error != null || data.Length == 0 ? error : await Write(data, 0, data.Length);

        }

        public async Task<ErrorResponse?> Write(byte[] data, int offset, int count)
        {
            try
            {
                await stream.WriteAsync(data, offset, count, cancellationToken);
            }
            catch(TaskCanceledException e)
            {
                return HttpStatusCode.RequestTimeout.Error("Request was cancelled");
            }
            catch(HttpRequestException e)
            {
                return e.StatusCode.Value.Error(e.Message);
            }
            catch(Exception e)
            {
                return HttpStatusCode.InternalServerError.Error(e.Message);
            }

            return null;
        }

        public Task<ErrorResponse?> Write(int value)
        {
            Span<byte> span = b;
            BinaryPrimitives.WriteInt32LittleEndian(span, value);
            return Write(b, 0, sizeof(int));
        }

        public Task<ErrorResponse?> Write(long value)
        {
            Span<byte> span = b;
            BinaryPrimitives.WriteInt64LittleEndian(span, value);
            return Write(b, 0, sizeof(long));
        }

        public Task<ErrorResponse?> Write(string? value)
        {
            if (value == null) return Write(-1);
            if (value.Length == 0) return Write(0);
            return Write(Encoding.UTF8.GetBytes(value));
        }

        public Task<ErrorResponse?> Write(byte value)
        {
            b[0] = value;
            return Write(b, 0, sizeof(byte));
        }

        public Task<ErrorResponse?> Write(bool value) => Write((byte)(value ? 1 : 0));

        public Task<ErrorResponse?> Write(DateTimeOffset value) => Write(value.ToUnixTimeMilliseconds());
    }
}
