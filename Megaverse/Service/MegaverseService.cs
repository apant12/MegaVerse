using System;
using System.Text;
using System.Text.Json;
using Megaverse.Models;

namespace Megaverse.Service
{
    public class MegaverseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://challenge.crossmint.io/api";
        private readonly string _candidateId;

        public MegaverseService(HttpClient httpClient, string candidateId)
        {
            _httpClient = httpClient;
            _candidateId = candidateId;
        }

        public async Task<AstralObjectResponse> CreatePolyanetAsync(AstralObjectRequest request)
        {
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/polyanets?candidateId={_candidateId}", content);

            if (response.IsSuccessStatusCode)
            {
                var astralObjectResponse = await response.Content.ReadFromJsonAsync<AstralObjectResponse>();
                return astralObjectResponse;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return new AstralObjectResponse { Success = false, Error = errorResponse };
            }
        }



        // Implement other methods similarly
    }


}

