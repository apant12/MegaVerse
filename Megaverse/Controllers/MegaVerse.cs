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
        private readonly SoloonService _soloonService;
        private readonly ComethService _comethService;

        // Assuming MegaverseService requires HttpClient and candidateId for initialization
        public MegaVerseController(MegaverseService megaService, ILogger<MegaverseService> logger, ComethService comethService, SoloonService soloonService)
        {
            _megaService = megaService;
            _logger = logger;
            _soloonService = soloonService;
            _comethService = comethService;
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

        [HttpPost("soloons")]
        public async Task<IActionResult> CreateSoloon([FromBody] SoloonObjectRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _soloonService.CreateSoloonAsync(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("soloons/{row}/{column}")]
        public async Task<IActionResult> DeleteSoloon(int row, int column)
        {
            try
            {
                await _soloonService.DeleteSoloonAsync(row, column);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("comeths")]
        public async Task<IActionResult> CreateCometh([FromBody] ComethObjectRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _comethService.CreateComethAsync(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("comeths/{row}/{column}")]
        public async Task<IActionResult> DeleteCometh(int row, int column)
        {
            try
            {
                await _comethService.DeleteComethAsync(row, column);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpDelete("delete-all-astral-objects")]
        public async Task<IActionResult> DeleteAllAstralObjects()
        {
            try
            {
                const int gridSize = 30; // Adjust this according to your grid size
                var tasks = new List<Task<AstralObjectResponse>>();

                for (int row = 0; row < gridSize; row++)
                {
                    for (int col = 0; col < gridSize; col++)
                    {
                        // Add delete tasks for each service
                        tasks.Add(DeleteWithRetryAsync(row, col, _megaService.DeletePolyanetAsync));
                        tasks.Add(DeleteWithRetryAsync(row, col, _soloonService.DeleteSoloonAsync));
                        tasks.Add(DeleteWithRetryAsync(row, col, _comethService.DeleteComethAsync));
                    }
                }

                // Wait for all the delete tasks to complete
                var responses = await Task.WhenAll(tasks);

                // Check for any failures and log them
                var failedDeletes = responses.Where(r => !r.Success).ToList();
                if (failedDeletes.Any())
                {
                    _logger.LogError($"Failed to delete some astral objects: {string.Join(", ", failedDeletes.Select(r => r.Error))}");
                    return StatusCode(500, "Failed to delete some astral objects.");
                }

                return Ok("All astral objects have been deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting all astral objects: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async Task<AstralObjectResponse> DeleteWithRetryAsync(int row, int col, Func<int, int, Task<AstralObjectResponse>> deleteFunc)
        {
            int maxRetries = 3;
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                var response = await deleteFunc(row, col);
                if (response.Success) return response;
                if (response.Error.Contains("Too Many Requests"))
                {
                    await Task.Delay((int)Math.Pow(2, retryCount) * 1000); // Exponential backoff
                    retryCount++;
                }
                else
                {
                    return response; // Return if the error is not related to rate limiting
                }
            }
            return new AstralObjectResponse { Success = false, Error = "Max retry attempts exceeded." };
        }



        private async Task<GoalMap> GetGoalMap(string candidateId)
        {
            try
            {
                // Assuming you have a method in MegaverseService to fetch the goal map
                var goalMap = await _megaService.GetGoalMapAsync(candidateId);
                return goalMap;
            }
            catch (Exception ex)
            {
                // Log the exception and return null or handle the error as needed
                _logger.LogError($"Error fetching goal map for candidate ID {candidateId}: {ex.Message}");
                return null;
            }
        }



        [HttpPost("generate-map")]
        public async Task<IActionResult> GenerateMap()
        {
            // Fetch the goal map using the specified candidate ID
            var candidateId = "3ade151f-3c7d-4dd3-8588-2d197a3c0565"; // Candidate ID provided
            var goalMap = await GetGoalMap(candidateId);

            if (goalMap == null || goalMap.Goal == null)
            {
                return BadRequest("Invalid goal map.");
            }

            // Parse the goal map to generate astral objects
            var astralObjects = ParseGoalMap(goalMap.Goal);

            // Create astral objects based on the parsed goal map
            var results = await CreateAstralObjects(astralObjects);

            // Return the generated map along with the results
            return Ok(new { GoalMap = goalMap, Results = results });
        }






        private List<AstralObject> ParseGoalMap(List<List<string>> goal)
        {
            var astralObjects = new List<AstralObject>();

            for (int row = 0; row < goal.Count; row++)
            {
                for (int column = 0; column < goal[row].Count; column++)
                {
                    var cellValue = goal[row][column];

                    if (cellValue == "POLYANET")
                    {
                        astralObjects.Add(new AstralObject
                        {
                            Type = AstralObjectType.Polyanet,
                            Row = row,
                            Column = column
                        });
                    }
                    else if (cellValue.EndsWith("_SOLOON"))
                    {
                        var color = cellValue.Split('_')[0];
                        astralObjects.Add(new AstralObject
                        {
                            Type = AstralObjectType.Soloons,
                            Row = row,
                            Column = column,
                            Color = color.ToLower() // Normalize color to lowercase
                        });
                    }
                    else if (cellValue.EndsWith("_COMETH"))
                    {
                        var direction = cellValue.Split('_')[0];
                        astralObjects.Add(new AstralObject
                        {
                            Type = AstralObjectType.Cometh,
                            Row = row,
                            Column = column,
                            Direction = direction.ToLower() // Normalize direction to lowercase
                        });
                    }
                }
            }

            return astralObjects;
        }

        [HttpPost("create-astral-objects")]
        public async Task<IActionResult> CreateAstralObjects(List<AstralObject> astralObjects)
        {
            if (astralObjects == null || !astralObjects.Any())
            {
                return BadRequest("Astral objects list is empty.");
            }

            var results = new List<AstralObjectResponse>();

            foreach (var astralObject in astralObjects)
            {
                switch (astralObject.Type)
                {
                    case AstralObjectType.Polyanet:
                        var polyanetRequest = new AstralObjectRequest
                        {
                            Row = astralObject.Row,
                            Column = astralObject.Column,
                            CandidateId = "3ade151f-3c7d-4dd3-8588-2d197a3c0565"
                        };
                        var polyanetResult = await _megaService.CreatePolyanetAsync(polyanetRequest);
                        results.Add(polyanetResult);
                        break;
                    case AstralObjectType.Cometh:
                        var comethRequest = new ComethObjectRequest
                        {
                            Row = astralObject.Row,
                            Column = astralObject.Column,
                            Direction = astralObject.Direction,
                            CandidateId = "3ade151f-3c7d-4dd3-8588-2d197a3c0565"
                        };
                        var comethResult = await _comethService.CreateComethAsync(comethRequest);
                        results.Add(comethResult);
                        break;
                    case AstralObjectType.Soloons:
                        var soloonRequest = new SoloonObjectRequest
                        {
                            Row = astralObject.Row,
                            Column = astralObject.Column,
                            Color = astralObject.Color,
                            CandidateId = "3ade151f-3c7d-4dd3-8588-2d197a3c0565"
                        };
                        var soloonResult = await _soloonService.CreateSoloonAsync(soloonRequest);
                        results.Add(soloonResult);
                        break;
                    default:
                        _logger.LogWarning($"Unknown astral object type: {astralObject.Type}");
                        break;
                }
            }

            return Ok(results);
        }
    }
}

