using System.Text.Json.Serialization;

namespace NexTech.API.Models
{
    public class Story
    {

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
