using System.Text.Json.Serialization;

namespace NextTech.Server.Models
{
    public class Story
    {

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
