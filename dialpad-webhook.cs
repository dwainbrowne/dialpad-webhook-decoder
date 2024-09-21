using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Webhooks.Dialpad
{
    public class DialpadWebhook
    {
        private readonly ILogger<DialpadWebhook> _logger;

        /// <summary>
        /// Constructor to initialize the logger.
        /// </summary>
        /// <param name="logger">Logger instance to log events.</param>
        public DialpadWebhook(ILogger<DialpadWebhook> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The main function that handles the Dialpad webhook.
        /// </summary>
        /// <param name="req">The HTTP request containing the webhook payload.</param>
        /// <param name="context">The execution context of the Azure function.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [Function("dialpad")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        FunctionContext context)
        {
            _logger.LogInformation("Dialpad webhook received.");

            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Parse the JSON payload
            JsonElement body;
            try
            {
                body = JsonSerializer.Deserialize<JsonElement>(requestBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse the webhook payload: {Exception}", ex);
                return new BadRequestObjectResult("Invalid JSON payload.");
            }

            string jwt = body.GetProperty("$content").ToString();
            string secret = "YOUR_DIALPAD_SECRET_YOU_SHARED_WITH_DIALPAD"; // From Dialpad's webhook secret

            dynamic data = JwtDecoder.DecodeJwt(jwt, secret);


            //return new OkObjectResult(data);

            // Check if data is null
            if (data.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("Webhook data is null.");
                return new BadRequestResult();
            }

            // Process the event
            if (data.TryGetProperty("state", out JsonElement eventTypeElement))
            {
                string? eventType = eventTypeElement.GetString();

                // Handle different event types
                switch (eventType)
                {
                    case "call_started":
                        await HandleCallStarted(data);
                        break;
                    case "call_ended":
                    case "hungup":
                        await HandleCallEnded(data);
                        break;
                    case "message_received":
                        await HandleMessageReceived(data);
                        break;
                    default:
                        _logger.LogInformation($"Unhandled event type: {eventType}");
                        break;
                }
            }
            else
            {
                _logger.LogWarning("Event type not specified in the request.");
                return new BadRequestResult();
            }

            // Respond with a 200 OK

            return new OkObjectResult(data);

        }

        /// <summary>
        /// Handles the call started event.
        /// </summary>
        /// <param name="data">The webhook data for call started event.</param>
        private static Task HandleCallStarted(JsonElement data)
        {
            // Extract call details
            string? callId = data.GetProperty("data").GetProperty("call_id").GetString();
            string? fromNumber = data.GetProperty("data").GetProperty("from_number").GetString();
            string? toNumber = data.GetProperty("data").GetProperty("to_number").GetString();

            // Add your logic here (e.g., log to database)
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the call ended event.
        /// </summary>
        /// <param name="data">The webhook data for call ended event.</param>
        private static Task HandleCallEnded(JsonElement data)
        {
            // Extract call details
            string? callId = data.GetProperty("data").GetProperty("call_id").GetString();
            int duration = data.GetProperty("data").GetProperty("duration").GetInt32();

            // Add your logic here
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the message received event.
        /// </summary>
        /// <param name="data">The webhook data for message received event.</param>
        private static Task HandleMessageReceived(JsonElement data)
        {
            // Extract message details
            string? messageId = data.GetProperty("data").GetProperty("message_id").GetString();
            string? content = data.GetProperty("data").GetProperty("content").GetString();

            // Add your logic here
            return Task.CompletedTask;
        }
    }
}
