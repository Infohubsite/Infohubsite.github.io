using Frontend.Common;
using Frontend.HttpClients;
using Frontend.Models.Koofr;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Shared.DTO.Server;

namespace Frontend.Services
{
    public interface IFileService
    {
        public record UploadProgress(string Status, float Progess, string? Name = null, bool? Success = null);
        int Cache(IBrowserFile file);
        void ClearCache();
        Task Download(string fileName);
        Task<IEnumerable<string?>> Upload(Action<UploadProgress> progress);
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
            // TODO
            // somehow download and then maybe display the file in the website
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string?>> Upload(Action<IFileService.UploadProgress> progress)
        {
            if (_cache.Count == 0) return [];

            progress(new("Getting ready to uplaod files", 0f));
            Result<KoofrUploadDto> result = await _client.GetUpload();

            if (!result.IsSuccess || string.IsNullOrEmpty(result.Value.Url))
            {
                _logger.LogError("Backend did not return a valid upload URL.");
                progress(new("File upload failed", 1f));
                return Enumerable.Repeat<string?>(null, _cache.Count);
            }

            int total = _cache.Count;
            int completedCount = 0;

            progress(new($"Uploaded 0 of {total} files", 0f));
            (int index, string? fileName)[] results = await Task.WhenAll(_cache.Select((file, index) =>
                Upload(file, index, result.Value.Url)
                    .ContinueWith(task =>
                    {
                        int currentCount = Interlocked.Increment(ref completedCount);
                        progress(new($"Uploaded {currentCount} of {total} files", (float)currentCount / total, task.IsCompletedSuccessfully ? task.Result.fileName : file.Name, task.IsCompletedSuccessfully));
                        return task.Result;
                    })
            ));

            progress(new("Upload complete", 1f));
            ClearCache();
            return results
                .OrderBy(r => r.index)
                .Select(r => r.fileName);
        }

        private async Task<(int index, string? fileName)> Upload(IBrowserFile file, int index, string uploadUrl)
        {
            try
            {
                using Stream stream = file.OpenReadStream(file.Size);
                using DotNetStreamReference streamRef = new(stream);

                FileResponseDto[] result = await _jsRuntime.InvokeAsync<FileResponseDto[]>("uploadFileToKoofr", uploadUrl, streamRef, file.Name, file.ContentType);

                if (result == null || result.Length == 0)
                {
                    _logger.LogError("Koofr upload for file #{Index} ({FileName}) did not return a valid response.", index, file.Name);
                    return (index, null);
                }

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