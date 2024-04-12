using System;
using System.Text;
using System.Text.Json;
using Megaverse.Models;

namespace Megaverse.Service
{
    public class SoloonService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly string _baseUrl = "https://challenge.crossmint.io/api";
        private readonly string _candidateId;
        private readonly ILogger<SoloonService> _logger;

        public SoloonService(IHttpClientFactory httpClient, ILogger<SoloonService> logger, string candidateId)
        {
            _httpClient = httpClient;
            _logger = logger;
            _candidateId = candidateId;
        }

        //public async Task<AstralObjectResponse> CreateSoloonAsync(SoloonObjectRequest request)
        //{
        //    using var httpClient = _httpClient.CreateClient();
        //    var requestBody = new
        //    {
        //        candidateId = _candidateId,
        //        row = request.Row,
        //        column = request.Column,
        //        color = request.Color
        //    };
        //    var jsonRequest = JsonSerializer.Serialize(requestBody);
        //    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync($"{_baseUrl}/soloons", content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var astralObjectResponse = await response.Content.ReadFromJsonAsync<AstralObjectResponse>();
        //        return astralObjectResponse;
        //    }
        //    else
        //    {
        //        var errorResponse = await response.Content.ReadAsStringAsync();
        //        if (errorResponse.Contains("Too Many Requests"))
        //        {
        //            _logger.LogError($"Failed to create Soloon at ({request.Row}, {request.Column}): {errorResponse}");
        //            await Task.Delay(25000); // Delay and retry or return error
        //        }
        //        return new AstralObjectResponse { Success = false, Error = errorResponse };
        //    }
        //}

        public async Task<AstralObjectResponse> CreateSoloonAsync(SoloonObjectRequest request)
        {
            using var httpClient = _httpClient.CreateClient();
            var requestBody = new
            {
                candidateId = _candidateId,
                row = request.Row,
                column = request.Column,
                color = request.Color
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            int retryCount = 0;
            int maxRetries = 3;
            do
            {
                response = await httpClient.PostAsync($"{_baseUrl}/soloons", content);

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
                _logger.LogError($"Failed to create Soloon at ({request.Row}, {request.Column}): {errorResponse}");
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }


        public async Task<AstralObjectResponse> DeleteSoloonAsync(int row, int column)
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
                    RequestUri = new Uri($"{_baseUrl}/soloons"),
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
                _logger.LogError($"Failed to delete Soloon at row {row}, column {column}: {errorResponse}");
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }

    }

}

