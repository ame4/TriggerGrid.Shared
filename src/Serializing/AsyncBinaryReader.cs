using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;

namespace TriggerGrid.Shared.Serializing
{
    public class AsyncBinaryReader {
        readonly static Response<byte[]?> nullBytes = new Response<byte[]?>(), emptyBytes = new Response<byte[]?>(Array.Empty<byte>());
        readonly static Response<string?> nullString = new Response<string?>(), emptyString = new Response<string?>(string.Empty);
        readonly Stream stream;
        readonly byte[] buffer = new byte[sizeof(double)];
        readonly CancellationToken cancellationToken;
        public AsyncBinaryReader(Stream stream, CancellationToken cancellationToken)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.cancellationToken = cancellationToken;
        }

        public async Task<Response<byte[]?>> ReadBytes()
        {
            Response<int> count = await ReadInt32();
            if(count.Error != null) return new Response<byte[]?>(count.Error);
            switch (count.Result)
            {
                case -1: return nullBytes;
                case 0: return emptyBytes;
            }

            if (count.Result < 0) return HttpStatusCode.UnprocessableContent.Error<byte[]?>($"Unexpected array length {count}");
            byte[] buffer;
            try
            {
                buffer = new byte[count.Result];
            }
            catch(Exception e)
            {
                return HttpStatusCode.UnprocessableContent.Error<byte[]?>($"Could not create array length {count}");
            }

            ErrorResponse? error = await Read(buffer, 0, buffer.Length);
            return error != null ? new Response<byte[]?>(error) : new Response<byte[]?>(buffer);
        }

        public async Task<ErrorResponse?> Read(byte[] buffer, int offset, int count)
        {
            while(count != 0)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
                }
                catch(HttpRequestException e)
                {
                    return e.StatusCode.Value.Error(e.Message);
                }
                catch(Exception e)
                {
                    return HttpStatusCode.InternalServerError.Error(e.Message);
                }

                if (bytesRead == 0)
                    return HttpStatusCode.UnprocessableEntity.Error("Unexpected end of stream");

                offset += bytesRead;
                count -= bytesRead;
            }

            return null;
        }

        public async Task<Response<int>> ReadInt32()
        {
            ErrorResponse? error = await Read(buffer, 0, sizeof(int));
            return error != null ? error.Error<int>() : BitConverter.ToInt32(buffer, 0).Success();
        }

        public async Task<Response<long>> ReadInt64()
        {
            ErrorResponse? error = await Read(buffer, 0, sizeof(long));
            return error != null ? error.Error<long>() : BitConverter.ToInt64(buffer, 0).Success();
        }


        public async Task<Response<byte>> ReadByte()
        {
            ErrorResponse? error = await Read(buffer, 0, sizeof(byte));
            return error != null ? new Response<byte>(error) : buffer[0].Success();
        }

        public async Task<Response<bool>> ReadBoolean()
        {
            ErrorResponse? error = await Read(buffer, 0, sizeof(byte));
            if(error != null) return error.Error<bool>();
            switch(buffer[0])
            {
                case 0: return false.Success();
                case 1: return true.Success();
            }

            return HttpStatusCode.UnprocessableContent.Error<bool>($"Invalid value for boolean {buffer[0]}");
        }


        public async Task<Response<string?>> ReadString()
        {
            Response<byte[]?> bytes = await ReadBytes();
            if(bytes.Error != null) return new Response<string?>(bytes.Error);
            if (bytes.Result == null) return nullString;
            if(bytes.Result.Length == 0) return emptyString;
            return new Response<string?>(Encoding.UTF8.GetString(bytes.Result));
        }

        public async Task<Response<DateTimeOffset>> ReadDateTimeOffset()
        {
            Response<long> response = await ReadInt64();
            return response.Error != null ? response.Error<DateTimeOffset>() : DateTimeOffset.FromUnixTimeMilliseconds(response.Result).Success();
        }

        public async Task<Response<DateTimeOffset?>> ReadNullableDateTimeOffset()
        {
            Response<bool> b = await ReadBoolean();
            if(b.Error != null) return b.Error<DateTimeOffset?>();
            if(!b.Result) return ((DateTimeOffset?)null).Success();
            Response<DateTimeOffset> response = await ReadDateTimeOffset();
            return response.Error != null ? response.Error.Error<DateTimeOffset?>() : ((DateTimeOffset?)response.Result).Success();
        }

    }
}
