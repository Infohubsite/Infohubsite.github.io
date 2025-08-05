using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.Common
{
    public record Result<T>
    {
        private Result _result { get; }

        public bool IsSuccess => _result.IsSuccess;
        public T? Value { get; }
        public HttpStatusCode? StatusCode => _result.StatusCode;
        public string? ErrorMessage => _result.ErrorMessage;

        internal Result(T? value, Result result)
        {
            Value = value;
            _result = result;
        }

        public static implicit operator Result<T>(Result result) => new(default, result);
        public static implicit operator Result(Result<T> result) => result._result;
    }

    public record Result
    {
        private static JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public bool IsSuccess { get; }
        public HttpStatusCode? StatusCode { get; }
        public string? ErrorMessage { get; }

        internal Result(bool isSuccess, HttpStatusCode? statusCode = null, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success<T>(T value) => new(value, new(true));
        public static Result<T> Success<T>(T value, HttpStatusCode statusCode) => new(value, new(true, statusCode));
        public static Result Success(HttpStatusCode statusCode) => new(true, statusCode);
        public static Result Fail(HttpStatusCode statusCode, string? errorMessage = null) => new(false, statusCode, errorMessage);
        public static Result Fail(string errorMessage) => new(false, null, errorMessage);
        public static async Task<Result> From(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return Success(response.StatusCode);
            else return Fail(response.StatusCode, await ParseErrorResponse(response));
        }
        public static async Task<Result<T?>> From<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return Fail(response.StatusCode, await ParseErrorResponse(response));

            T? result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null) return Fail(response.StatusCode, $"Failed to read and convert response to {typeof(T).Name}");

            return Success<T?>(result, response.StatusCode);
        }

        private static async Task<string> ParseErrorResponse(HttpResponseMessage response)
        {
            string raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(raw)) return "The server returned an empty error response";

            try
            {
                ApiError? error = JsonSerializer.Deserialize<ApiError>(raw, Options);
                return error?.Message ?? error?.Title ?? raw;
            }
            catch (JsonException)
            {
                return raw;
            }
        }

        private record ApiError
        {
            [JsonPropertyName("title")] public string? Title { get; init; }
            [JsonPropertyName("message")] public string? Message { get; init; }
            [JsonPropertyName("status")] public int? Status { get; init; }
            [JsonPropertyName("error")] public string? Error { get; init; }
            [JsonPropertyName("errors")] public List<string>? Errors { get; init; }
        }
    }
}
