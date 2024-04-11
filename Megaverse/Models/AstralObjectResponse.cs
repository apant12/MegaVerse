using System;
namespace Megaverse.Models
{
    public class AstralObjectResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ObjectId { get; set; } // Assuming the API returns an ID for the created object. Nullable in case of failure.
        public string Error { get; set; } // Optional, in case there's an error message
    }

}

