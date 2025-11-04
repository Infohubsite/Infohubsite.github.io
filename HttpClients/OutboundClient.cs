using Frontend.Common;
using Frontend.Models.Koofr;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Frontend.HttpClients
{
    public class OutboundClient(HttpClient httpClient, ILogger<OutboundClient> logger, INotificationService notifs) : Client<OutboundClient>(httpClient, logger, notifs)
    {
        #region Koofr
        public async Task<Result<FileResponseDto[]>> UploadFile(string uploadUrl, IBrowserFile file)
        {
            // Acknowledging previous syntax errors. This code is now corrected.
            // The goal is to let the browser's fetch API handle the Content-Type header
            // by interfering as little as possible.

            await using var memoryStream = new MemoryStream();
            await file.OpenReadStream(maxAllowedSize: 524_288_000).CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);

            // Do not set any Content-Type headers here.
            content.Add(fileContent, "file", file.Name);

            var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
            {
                Content = content
            };

            // === START HEADER LOGGING ===
            // As requested, logging all headers on the request object before sending.
            _logger.LogInformation("--- Preparing to send request for {FileName} ---", file.Name);
            _logger.LogInformation("--- Request Headers ---");
            foreach (var header in request.Headers)
            {
                _logger.LogInformation("  {Header}: {Value}", header.Key, string.Join(", ", header.Value));
            }
            if (request.Content != null)
            {
                _logger.LogInformation("--- Content Headers ---");
                // Note: The Content-Type header with boundary is generated here when the Content property is accessed.
                foreach (var header in request.Content.Headers)
                {
                    _logger.LogInformation("  {Header}: {Value}", header.Key, string.Join(", ", header.Value));
                }
            }
            _logger.LogInformation("-------------------------------------------------");
            // === END HEADER LOGGING ===

            return await HandleAsync(
                SendResult<FileResponseDto[]>, // Corrected typo: FileResponseDto
                request,
                "Error while uploading file to storage",
                ex => _logger.LogError(ex, "Could not upload file {FileName}", file.Name)
            );
        }
        #endregion
    }
}