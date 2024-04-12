using System;
using System.Text;
using System.Text.Json;
using Megaverse.Models;

namespace Megaverse.Service
{
    public class ComethService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly string _baseUrl = "https://challenge.crossmint.io/api";
        private readonly string _candidateId;
        private readonly ILogger<ComethService> _logger;

        public ComethService(IHttpClientFactory httpClient, ILogger<ComethService> logger, string candidateId)
        {
            _httpClient = httpClient;
            _logger = logger;
            _candidateId = candidateId;
        }

        public async Task<AstralObjectResponse> CreateComethAsync(ComethObjectRequest request)
        {
            using var httpClient = _httpClient.CreateClient();
            var requestBody = new
            {
                candidateId = _candidateId,
                row = request.Row,
                column = request.Column,
                direction = request.Direction
            };
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            int retryCount = 0;
            int maxRetries = 3;
            do
            {
                response = await httpClient.PostAsync($"{_baseUrl}/comeths", content);

                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    int delay = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff factor
                    _logger.LogWarning($"Rate limit exceeded, retrying in {delay}ms...");
                    await Task.Delay(delay);
                    retryCount++;
                }
                else
                {
                    break;
                }
            } while (retryCount <= maxRetries);

            if (response.IsSuccessStatusCode)
            {
                var astralObjectResponse = await response.Content.ReadFromJsonAsync<AstralObjectResponse>();
                return astralObjectResponse;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to create Cometh at ({request.Row}, {request.Column}): {errorResponse}");
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }


        public async Task<AstralObjectResponse> DeleteComethAsync(int row, int column)
        {
            using var httpClient = _httpClient.CreateClient();
            var requestBody = new
            {
                candidateId = _candidateId,
                row = row,
                column = column
            };
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            int retryCount = 0;
            int maxRetries = 3;
            do
            {
                response = await httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{_baseUrl}/comeths"),
                    Content = content
                });

                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    int delay = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff factor
                    _logger.LogWarning($"Rate limit exceeded, retrying in {delay}ms...");
                    await Task.Delay(delay);
                    retryCount++;
                }
                else
                {
                    break;
                }
            } while (retryCount <= maxRetries);

            if (response.IsSuccessStatusCode)
            {
                return new AstralObjectResponse { Success = true };
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to delete Cometh at row {row}, column {column}: {errorResponse}");
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }

    }
}


