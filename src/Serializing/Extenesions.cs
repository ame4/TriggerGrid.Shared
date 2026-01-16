using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Collections;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TriggerGrid.Shared.Serializing
{
    public static class Extenesions
    {
        public static Task<Response<TItem[]?>> Read<TItem>(this AsyncBinaryReader reader)
            where TItem : IAsyncSerializable, new()
            => reader.Read<TItem>(
                async () =>
                {
                    TItem item = new TItem();
                    return item.Response(await item.Read(reader));
                });

        public static Task<Response<string?[]?>> ReadStrings(this AsyncBinaryReader reader)
            => reader.Read(reader.ReadString);

        public static async Task<Response<TItem[]?>> Read<TItem>(this AsyncBinaryReader reader, Func<Task<Response<TItem>>> factory)
        {
            Response<int> count = await reader.ReadInt32();
            if (count.Error != null) return count.Error.Error<TItem[]?>();
            switch (count.Result)
            {
                case -1: return new Response<TItem[]?>();
                case 0: return new Response<TItem[]?>(Array.Empty<TItem>());
            }

            if (count.Result < 0) return HttpStatusCode.UnprocessableContent.Error<TItem[]?>("Invalid array length {count}");
            TItem[] items = new TItem[count.Result];
            for (int i = 0; i < items.Length; i++)
            {
                Response<TItem> item = await factory();
                if (item.Error != null) return item.Error.Error<TItem[]?>();
                items[i] = item.Result;
            }

            return new Response<TItem[]?>(items);
        }

        public static async Task<Response<TItem?[]?>> ReadArrayNullable<TItem>(this AsyncBinaryReader reader)
            where TItem : IAsyncSerializable, new()
            => await reader.Read<TItem?>(async () =>
            {
                Response<bool> b = await reader.ReadBoolean();
                if (b.Error != null) return b.Error.Error<TItem?>();
                if (!b.Result) return new Response<TItem?>();
                TItem item = new TItem();
                ErrorResponse? error = await item.Read(reader);
                return error != null ? error.Error<TItem?>() : new Response<TItem?>(item);
            });

        public static async Task<IResponse<string>> ReadNotEmpty(this AsyncBinaryReader reader, string fieldName)
        {
            IResponse<string?> response = await reader.ReadString();
            return (response.Error == null && string.IsNullOrEmpty(response.Result)) ?
                HttpStatusCode.UnprocessableContent.Error<string>($"Empty value for {fieldName}")
                : response;
        }

        public static async Task<IResponse<TItem?>> ReadNullable<TItem>(this AsyncBinaryReader reader)
            where TItem : IAsyncSerializable, new()
        {
            IResponse<bool> b = await reader.ReadBoolean();
            if (!b.Result) return b.Error.Error<TItem?>();
            TItem item = new TItem();
            ErrorResponse? error = await item.Read(reader);
            return error != null ? error.Error<TItem>() : item.Success();
        }

        public static async Task<IResponse<IPage<TItem>>> ReadPage<TItem>(this AsyncBinaryReader reader)
            where TItem : IAsyncSerializable, new()
        {
            IResponse<string?> continuationToken = await reader.ReadString();
            if (continuationToken.Error != null) return continuationToken.Error.Error<IPage<TItem>>();
            IResponse<TItem[]?> items = await reader.Read<TItem>();
            if(items.Error != null) return items.Error.Error<IPage<TItem>>();
            if (items.Result == null) return HttpStatusCode.UnprocessableContent.Error<IPage<TItem>>("Could not read page items");
            return new Page<TItem>(items.Result) { ContinuationToken = continuationToken.Result }.Success<IPage<TItem>>();
        }



        public static Task<ErrorResponse?> Write<TItem>(this AsyncBinaryWriter writer, IReadOnlyCollection<TItem>? items)
            where TItem : IAsyncSerializable => Write<TItem>(writer, items, item => item.Write(writer));

        public static Task<ErrorResponse?> Write(this AsyncBinaryWriter writer, IReadOnlyCollection<string?>? items)
            => Write(writer, items, item => writer.Write(item));

        public static async Task<ErrorResponse?> Write<TItem>(this AsyncBinaryWriter writer, IReadOnlyCollection<TItem>? items, Func<TItem, Task<ErrorResponse?>> itemWriter)
        {
            if (items == null) return await writer.Write(-1);
            ErrorResponse? error = await writer.Write(items.Count);
            if (error != null) return error;
            foreach(TItem item in items)
            {
                error = await itemWriter(item);
                if (error != null) return error;
            }

            return null;
        }

        public static async Task<ErrorResponse?> WriteArrayNullable<TItem>(this AsyncBinaryWriter writer, IReadOnlyCollection<TItem?>? items)
            where TItem : IAsyncSerializable
            => await writer.Write<TItem?>(items,
                async item =>
                {
                    if (item == null) return await writer.Write(false);
                    else return await writer.Write(true) ?? await item.Write(writer);
                });

        public static async Task<ErrorResponse?> WriteNullable(this AsyncBinaryWriter writer, IAsyncSerializable? serializable)
        {
            if (serializable == null) return await writer.Write(false);
            return await writer.Write(true) ?? await serializable.Write(writer);
        }

        public static async Task<ErrorResponse?> Write<TItem>(this AsyncBinaryWriter writer, IPage<TItem> page)
            where TItem : IAsyncSerializable
            => await writer.Write(page.ContinuationToken) ?? await writer.Write((IReadOnlyCollection<TItem>)page);
    }
}
