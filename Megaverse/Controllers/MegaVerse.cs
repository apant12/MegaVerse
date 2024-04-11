using Megaverse.Models;
using Megaverse.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Megaverse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MegaVerseController : ControllerBase
    {
        private readonly MegaverseService _megaService;
        private readonly ILogger<MegaverseService> _logger;

        // Assuming MegaverseService requires HttpClient and candidateId for initialization
        public MegaVerseController(MegaverseService megaService, ILogger<MegaverseService> logger)
        {
            _megaService = megaService;
            _logger = logger;
        }


        [HttpPost("create-x-pattern")]
        public async Task<IActionResult> CreateXPattern()
        {
            const int gridSize = 11; // Assuming an 11x11 grid
            List<AstralObjectResponse> results = new List<AstralObjectResponse>();

            for (int row = 2; row < gridSize-2; row++)
            {
                for (int column = 2; column < gridSize-2; column++)
                {
                    // Check if the position is part of the X pattern
                    if (row == column || row + column == gridSize - 1)
                    {
                        var request = new AstralObjectRequest
                        {
                            Row = row,
                            Column = column
                        };
                        // Use the existing CreatePolyanetAsync method in your service
                        var result = await _megaService.CreatePolyanetAsync(request);
                        results.Add(result);

                        // Add error handling if result is not successful
                        if (!result.Success)
                        {
                            // Log the error, break, or handle it as necessary
                            _logger.LogError($"Error creating Polyanet at ({row}, {column}): {result.Error}");
                            // Optionally introduce a delay if you're encountering rate limiting
                            await Task.Delay(25000); // 1 second delay
                        }
                    }
                }
            }


            // Optional: Check if any creations failed
            if (results.Any(r => !r.Success))
            {
                // Handle any errors, e.g., by returning a specific error response
                return StatusCode(500, "An error occurred while creating the X pattern.");
            }

            return Ok(results);
        }





        // Example POST method for creating a Polyanet
        [HttpPost("polyanets")]
        public async Task<IActionResult> CreatePolyanet([FromBody] AstralObjectRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _megaService.CreatePolyanetAsync(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Implement other endpoints similarly

        // DELETE example for deleting a Polyanet
        [HttpDelete("polyanets/{row}/{column}")]
        public async Task<IActionResult> DeleteOnePolyanet(int row, int column)
        {
            try
            {
                await _megaService.DeletePolyanetAsync(row, column);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        [HttpDelete("delete-all-polyanets")]
        public async Task<IActionResult> DeleteAllPolyanets()
        {
            try
            {
                // Assuming the gridSize is known, for example, 11 for an 11x11 grid.
                await _megaService.DeleteXShapedPatternAsync();
                return Ok("All Polyanets have been deleted.");
            }
            catch (Exception ex)
            {
                // Log the exception and return an appropriate error response
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }




    }
}
