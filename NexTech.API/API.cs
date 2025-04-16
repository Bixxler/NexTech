using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NexTech.API.Services;
using System.Net;

namespace nexTech.API
{
    public class GetStoriesFunction
    {
        private readonly IStoryService _storyService;
        private readonly ILogger<GetStoriesFunction> _logger;

        public GetStoriesFunction(IStoryService storyService, ILogger<GetStoriesFunction> logger)
        {
            _storyService = storyService;
            _logger = logger;
        }

        [Function("GetStories")]
        public async Task<HttpResponseData> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stories")] HttpRequestData req,
             FunctionContext context)
        {
            var logger = context.GetLogger("GetStories");
            //This creates a new logger instance specifically tied to the Azure Function's execution context, labeled "GetStories" in logs.
            //reverted back to this after further reading on how this would be useful over just using _logger

            try
            {

                var stories = await _storyService.Get();

                if (stories == null || stories.Count == 0)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    return notFoundResponse;
                }

                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                
                okResponse.Headers.Add("Cache-Control", "public, max-age=300");

                await okResponse.WriteAsJsonAsync(stories);
                return okResponse;
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, "Application error while retrieving stories.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(ex.Message);
                return errorResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while retrieving stories.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An unexpected error occurred.");
                return errorResponse;
            }
        }
    }
}
