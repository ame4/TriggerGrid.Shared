using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TriggerGrid.Shared.Data
{
    public static class Extensions
    {
        readonly static Response<bool> trueResponse = new Response<bool>(true), falseResponse = new Response<bool>(false);
        public static Response<TOutput>  Translate<TInput, TOutput>(this IResponse<TInput> input, Func<TInput, TOutput> translater)
        {
            if(input.Error != null) return new Response<TOutput>(input.Error);
            return input.Result != null ? new Response<TOutput>(translater(input.Result)) : new Response<TOutput>();
        }

        public static async Task<Response<TOutput>> Translate<TInput, TOutput>(this Task<IResponse<TInput>> task, Func<TInput, TOutput> translater) => (await task).Translate(translater);

        public static ErrorResponse Error(this HttpStatusCode statusCode, string message) => new ErrorResponse(statusCode, message, null);
        public static ErrorResponse Error(this HttpStatusCode statusCode, string message, string? details) => new ErrorResponse(statusCode, message, details);
        public static Response<TResult> Error<TResult>(this HttpStatusCode statusCode, string message) => new Response<TResult>(statusCode.Error(message, null));
        public static Response<TResult> Response<TResult>(this HttpStatusCode statusCode, string message, string? details) => new Response<TResult>(statusCode.Error(message, details));
        public static ErrorResponse AddDetails(this ErrorResponse error, string details) => new ErrorResponse(error.StatusCode, error.Message, string.IsNullOrEmpty(error.Details) ? details : error.Details + "\r\n" + details);

        public static Response<TResult> AddDetails<TResult>(this Response<TResult> response, string details) => new Response<TResult>(response.Error.AddDetails(details));
        public static Response<bool> Success(this bool value) => value ? trueResponse : falseResponse;
        public static Response<TItem> Success<TItem>(this TItem item) => new Response<TItem>(item);
        public static Response<TItem> Error<TItem>(this ErrorResponse? error) => new Response<TItem>(error);
        public static Response<TItem> Error<TItem>(this Response source) => new Response<TItem>(source.Error);
        public static Response<TItem> Response<TItem>(this TItem item, ErrorResponse? error) => error != null ? error.Error<TItem>() : item.Success();

        public static ErrorResponse? VerifyResourceId(this string? resourceId)
        {
            if(string.IsNullOrEmpty(resourceId)) return HttpStatusCode.BadRequest.Error("ResourceId is required.");
            if (resourceId.Length < 3 || resourceId.Length > 63) 
                return HttpStatusCode.BadRequest.Error($"Invalid resourceId length {resourceId}. Minimum length: 3 characters. Maximum length: 63 characters");
            return resourceId.Any(c => !Char.IsDigit(c) && (c < 'a' || c > 'z'))
                ? HttpStatusCode.BadRequest.Error($"Invalid characters in resourceId {resourceId}. Only lowercase letters: a–z and Numbers: 0–9 are allowed")
                : null;
        }
    }
}
