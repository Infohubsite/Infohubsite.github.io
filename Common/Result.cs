using Shared.DTO.Server;
using Shared.Extensions;
using Shared.Interface;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.Common
{
    public record ResultConverter
    {
        public static Result<TTo> ConvertFrom<TTo, TFrom>(Result<TFrom> result) where TTo : IConverterFrom<TTo, TFrom> => Convert(result, TTo.Convert);
        public static Result<TTo> ConvertTo<TTo, TFrom>(Result<TFrom> result) where TFrom : IConverterTo<TFrom, TTo> => Convert(result, TFrom.Convert);
        public static Result<List<TTo>> ConvertListFrom<TTo, TFrom>(Result<List<TFrom>> result) where TTo : IConverterFrom<TTo, TFrom> => Convert(result, ListConverter.ConvertFrom<TTo, TFrom>);
        public static Result<List<TTo>> ConvertListTo<TTo, TFrom>(Result<List<TFrom>> result) where TFrom : IConverterTo<TFrom, TTo> => Convert(result, ListConverter.ConvertTo<TTo, TFrom>);

        public static Result<TTo> Convert<TTo, TFrom>(Result<TFrom> result, Func<TFrom, TTo> func) => Result<TTo>.Convert(result, func);
    }

    public record Result<T> : Result
    {
        public T? Value { get; }

        [MemberNotNullWhen(true, nameof(Value))]
        public new bool IsSuccess => base.IsSuccess;

        private Result(Result result, T? value = default) : base(result)
        {
            Value = value;
        }

        public static Result<T> Success(T value, HttpStatusCode? statusCode = null) => new(Success(statusCode), value);
        public static new async Task<Result<T>> Fail(HttpResponseMessage response) => new(await Result.Fail(response));
        public static new Result<T> Fail(HttpStatusCode? statusCode = null, Exception? exception = null) => new(Result.Fail(statusCode, exception));
        public static new Result<T> Fail(Exception exception) => Fail(null, exception);
        public static new async Task<Result<T>> From(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return await Fail(response);

            T? result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null) return Fail(response.StatusCode, new JsonException($"Failed to read and convert response to {typeof(T).Name}"));

            return Success(result, response.StatusCode);
        }

        public static async Task<Result<T>> HandleAsync(Func<Task<Result<T>>> func, Func<Exception, Task> custom) => await HandlingUtils.HandleAsync(func, async (ex) =>
        {
            await custom(ex);
            return Fail(null, ex);
        });
        public static async Task<Result<T>> HandleAsync(Func<Task<Result<T>>> func, Action<Exception> custom) => await HandlingUtils.HandleAsync(func, (ex) =>
        {
            custom(ex);
            return Fail(ex);
        });

        public static Result<T> Convert<TTo>(Result<TTo> result, Func<TTo, T> func) => new(result, result.IsSuccess ? func(result.Value) : default);
    }

    public record Result
    {
        public bool IsSuccess { get; }
        public HttpStatusCode? StatusCode { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, HttpStatusCode? statusCode = null, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Exception = exception;
        }

        public static Result Success(HttpStatusCode? statusCode = null) => new(true, statusCode);
        public static async Task<Result> Fail(HttpResponseMessage response) => Fail(response.StatusCode, new(await ParseErrorResponse(response)));
        public static Result Fail(HttpStatusCode? statusCode = null, Exception? exception = null) => new(false, statusCode, exception);
        public static Result Fail(Exception exception) => Fail(null, exception);
        public static async Task<Result> From(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return Success(response.StatusCode);
            else return await Fail(response);
        }

        public static async Task<Result> HandleAsync(Func<Task<Result>> func, Func<Exception, Task> custom) => await HandlingUtils.HandleAsync(func, async (ex) =>
        {
            await custom(ex);
            return Fail(ex);
        });
        public static async Task<Result> HandleAsync(Func<Task<Result>> func, Action<Exception> custom) => await HandlingUtils.HandleAsync(func, (ex) =>
        {
            custom(ex);
            return Fail(ex);
        });


        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        protected static async Task<string> ParseErrorResponse(HttpResponseMessage response)
        {
            string raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(raw)) return "The server returned an empty error response.";

            try
            {
                ErrorResponseDto? error = JsonSerializer.Deserialize<ErrorResponseDto>(raw, Options);
                return error?.Message ?? raw;
            }
            catch (JsonException)
            {
                return raw;
            }
        }
    }
}
