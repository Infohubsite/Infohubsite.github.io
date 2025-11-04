using Frontend.Common;
using Frontend.HttpClients;
using Frontend.Models.Koofr;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Shared.DTO.Server;
using System.IO;

namespace Frontend.Services
{
    public interface IFileService
    {
        int Cache(IBrowserFile file);
        void ClearCache();
        Task Download(string fileName);
        Task<IEnumerable<string?>> Upload();
    }

    public class FileService(DefaultClient client, IJSRuntime jsRuntime, ILogger<FileService> logger) : IFileService
    {
        private readonly DefaultClient _client = client;
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly ILogger<FileService> _logger = logger;

        private readonly List<IBrowserFile> _cache = [];

        public int Cache(IBrowserFile file)
        {
            _cache.Add(file);
            return _cache.Count - 1;
        }
        public void ClearCache() => _cache.Clear();

        public async Task Download(string fileName)
        {
            // somehow download and then maybe display the file in the website
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string?>> Upload()
        {
            if (_cache.Count == 0) return [];

            _logger.LogInformation("Requesting upload URL from backend for {Count} files.", _cache.Count);
            Result<KoofrUploadDto> result = await _client.GetUpload();

            if (!result.IsSuccess || string.IsNullOrEmpty(result.Value.Url))
            {
                _logger.LogError("Backend did not return a valid upload URL.");
                return Enumerable.Repeat<string?>(null, _cache.Count);
            }

            (int index, string? fileName)[] results = await Task.WhenAll(_cache.Select((file, index) => Upload(file, index, result.Value.Url)));

            ClearCache();
            return results
                .OrderBy(r => r.index)
                .Select(r => r.fileName);
        }

        private async Task<(int index, string? fileName)> Upload(IBrowserFile file, int index, string uploadUrl)
        {
            try
            {
                _logger.LogInformation("Uploading file #{Index}: {FileName} to Koofr via JS Interop.", index, file.Name);

                using Stream stream = file.OpenReadStream(file.Size);
                using DotNetStreamReference streamRef = new(stream);

                FileResponseDto[] result = await _jsRuntime.InvokeAsync<FileResponseDto[]>("uploadFileToKoofr", uploadUrl, streamRef, file.Name, file.ContentType);

                if (result == null || result.Length == 0)
                {
                    _logger.LogError("Koofr upload for file #{Index} ({FileName}) did not return a valid response.", index, file.Name);
                    return (index, null);
                }

                _logger.LogInformation("Successfully uploaded file #{Index}: {FileName}", index, file.Name);
                return (index, result.FirstOrDefault()?.Name);
            }
            catch (JSException ex)
            {
                _logger.LogError(ex, "JS Interop error during upload for file #{Index} ({FileName}).", index, file.Name);
                return (index, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while uploading file #{Index}: {FileName}", index, file.Name);
                return (index, null);
            }
        }
    }
}