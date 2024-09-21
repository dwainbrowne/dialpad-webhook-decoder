Here's a README documentation you can use based on the C# class for decoding a Dialpad JWT:

---

# Dialpad Event Subscription Webhook

This repository contains a webhook implementation that decodes a JWT received from Dialpad event subscriptions. This service is designed to consume and process Dialpad events such as call, SMS, and contact status notifications.

## Getting Started

To consume events from Dialpad, you need to build an HTTP service that listens for incoming events. Dialpad sends these events as JWTs (JSON Web Tokens) for security purposes. Each JWT is signed with a shared secret, allowing you to verify the authenticity of the event.

### Prerequisites

- .NET 6 or later
- A public HTTP service to receive event notifications (e.g., Azure Functions)
- A valid secret key shared with Dialpad

### Event Subscription

Dialpad sends various live events like call, SMS, contact status, and change logs. Events are sent in a JWT format and can be subscribed to using the Dialpad API.

To subscribe to events, follow these steps:

1. Build an HTTP service.
2. Create an event subscription in Dialpad's API, providing the URL of your HTTP service.
3. Generate a secure string to use as the shared secret with Dialpad when creating the event subscription.

For more details, refer to the [Dialpad API documentation](https://developers.dialpad.com/).

### Secret and Event Encoding

The JWT is signed with a secret you share with Dialpad to ensure that the event originated from their system. After verifying the token, you'll be able to access the event's payload, which contains details about the event.

## How It Works

This project decodes and verifies the JWT using the `JwtDecoder` class, which returns the payload of the event as a `JsonElement`. The payload contains event data in JSON format, which can be used to handle events like call started, call ended, or message received.

### `JwtDecoder` Class

The `JwtDecoder` class decodes the JWT using the secret key provided by Dialpad and returns the payload. Here’s how it works:

- The token is first Base64-decoded.
- The secret key is used to validate the signature of the JWT.
- The payload is extracted and returned as a dynamic object (`JsonElement`).

### Webhook Handler

The webhook handler (`DialpadWebhook`) processes the incoming requests from Dialpad. It verifies the JWT, decodes it, and handles different types of events (e.g., `call_started`, `call_ended`, `message_received`).

### Example Events

- **Call Started:** Extracts the call ID, from number, and to number.
- **Call Ended:** Extracts the call ID and duration of the call.
- **Message Received:** Extracts the message ID and content.

## Usage

### 1. Set Up the HTTP Service

This example uses Azure Functions as the HTTP service. Once deployed, the service will listen for incoming POST requests from Dialpad.

### 2. Configure Your Secret

Ensure you have configured your secret shared with Dialpad in the code:

```csharp
string secret = "YOUR_DIALPAD_SECRET_YOU_SHARED_WITH_DIALPAD"; // From Dialpad's webhook secret
```

### 3. Decode the JWT

To decode the JWT received in the event payload, the `JwtDecoder.DecodeJwt()` method is used:

```csharp
string jwt = body.GetProperty("$content").ToString();
dynamic data = JwtDecoder.DecodeJwt(jwt, secret);
```

### 4. Handle Events

Once the event data is decoded, different event types are handled:

```csharp
switch (eventType)
{
    case "call_started":
        await HandleCallStarted(data);
        break;
    case "call_ended":
        await HandleCallEnded(data);
        break;
    case "message_received":
        await HandleMessageReceived(data);
        break;
    default:
        _logger.LogInformation($"Unhandled event type: {eventType}");
        break;
}
```

### 5. Deploy the Webhook

Deploy the webhook using any cloud platform, such as Azure Functions, to expose the public endpoint required for event subscriptions.

## API Documentation

For details on how to create and manage event subscriptions, visit the [Dialpad API reference](https://developers.dialpad.com/reference/event-subscriptions).

---

## Contributing

Feel free to fork this repository and submit pull requests if you wish to improve the webhook functionality or add more event types.

## License

This project is licensed under the MIT License.

---
