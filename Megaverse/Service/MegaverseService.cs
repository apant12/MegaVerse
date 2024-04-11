using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Megaverse.Models;

namespace Megaverse.Service
{
    public class MegaverseService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly string _baseUrl = "https://challenge.crossmint.io/api";
        private readonly string _candidateId;
        private readonly ILogger<MegaverseService> _logger;


        public MegaverseService(IHttpClientFactory httpClient, ILogger<MegaverseService> logger, string candidateId)
        {
            _httpClient = httpClient;
            _logger = logger;
            _candidateId = candidateId;
        }

        public async Task<IEnumerable<AstralObjectResponse>> DeleteXShapedPatternAsync()
        {
            const int gridSize = 11; // The grid size
            List<AstralObjectResponse> responses = new List<AstralObjectResponse>();

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    // Skip the grid's center if it's odd-sized.
                    //if (gridSize % 2 == 1 && row == gridSize / 2 && col == gridSize / 2)
                    //{
                    //    continue;
                    //}

                    // Check if the position is on either of the two diagonals.
                    if (row == col || row + col == gridSize - 1)
                    {
                        var deleteResponse = await DeletePolyanetAsync(row, col);
                        responses.Add(deleteResponse);

                        // If the deletion was unsuccessful due to rate limiting, log and wait
                        if (!deleteResponse.Success && deleteResponse.Error.Contains("Too Many Requests"))
                        {
                            _logger.LogError($"Failed to delete Polyanet at ({row}, {col}): {deleteResponse.Error}");
                            // Wait for a certain period before trying the next request
                            await Task.Delay(30000); // Delay time in milliseconds; adjust as needed
                        }
                    }
                }
            }

            return responses;
        }



        public async Task<AstralObjectResponse> DeletePolyanetAsync(int row, int column)
        {
            // Log the attempt to delete the Polyanet at the given coordinates.
            _logger.LogInformation($"Attempting to delete Polyanet at row {row}, column {column}.");
            using var httpClient = _httpClient.CreateClient();
            var requestBody = new
            {
                candidateId = _candidateId,
                row = row,
                column = column
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_baseUrl}/polyanets"),
                Content = content
            };

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new AstralObjectResponse { Success = true };
            }
            else
            {
                // The deletion was not successful. Log the status code and response.
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to delete Polyanet at row {row}, column {column}. Response: {errorResponse}");
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }


        public async Task<bool> DeleteOnePolyanetAsync(int row, int column)
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

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_baseUrl}/polyanets"),
                Content = content
            };

            var response = await httpClient.SendAsync(request);

            // rest of the method remains the same...


            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                // Depending on your error handling, you might want to log this or throw an exception.
                return false;
            }
        }

        public async Task<AstralObjectResponse> CreatePolyanetAsync(AstralObjectRequest request)
        {
            using var httpClient = _httpClient.CreateClient();

            var requestBody = new
            {
                candidateId = _candidateId,
                column = request.Column,
                row = request.Row

            };
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_baseUrl}/polyanets", content);

            if (response.IsSuccessStatusCode)
            {
                var astralObjectResponse = await response.Content.ReadFromJsonAsync<AstralObjectResponse>();
                return astralObjectResponse;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                if (errorResponse.Contains("Too Many Requests"))
                {
                    _logger.LogError($"Failed to create Polyanet at ({request.Row}, {request.Column}): {errorResponse}");
                    await Task.Delay(15000); // Delay and retry or return error
                }
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }

            public async Task DeleteAllPolyanetsAsync(int gridSize)
            {
                var tasks = new List<Task<AstralObjectResponse>>();
                for (int row = 0; row < gridSize; row++)
                {
                    for (int col = 0; col < gridSize; col++)
                    {
                        tasks.Add(DeletePolyanetAsync(row, col));
                    }
                }

                // Wait for all the delete tasks to complete
                var responses = await Task.WhenAll(tasks);

                // Log the outcome of each delete operation
                foreach (var response in responses)
                {
                    if (response.Success)
                    {
                        _logger.LogInformation($"Deleted Polyanet: Success");
                    }
                    else
                    {
                        _logger.LogError($"Failed to delete Polyanet: {response.Error}");
                    }
                }
            }




            // Implement other methods similarly
        }




    }


