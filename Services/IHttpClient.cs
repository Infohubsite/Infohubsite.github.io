using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Frontend.Services
{
    public interface IHttpClient
    {
        public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
        public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
        public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption);
        public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);
    }

    /*public interface IHttpClientJson
    {
        public Task<object?> GetFromJsonAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, Type type, JsonSerializerOptions? options, CancellationToken cancellationToken = default);
        public Task<object?> GetFromJsonAsync(this HttpClient client, Uri? requestUri, Type type, JsonSerializerOptions? options, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, JsonSerializerOptions? options, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, Uri? requestUri, JsonSerializerOptions? options, CancellationToken cancellationToken = default);
        public Task<object?> GetFromJsonAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, Type type, JsonSerializerContext context, CancellationToken cancellationToken = default);
        public Task<object?> GetFromJsonAsync(this HttpClient client, Uri? requestUri, Type type, JsonSerializerContext context, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, Uri? requestUri, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
        public Task<object?> GetFromJsonAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, Type type, CancellationToken cancellationToken = default);
        public Task<object?> GetFromJsonAsync(this HttpClient client, Uri? requestUri, Type type, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken = default);
        public Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, Uri? requestUri, CancellationToken cancellationToken = default);
    }*/
}
