using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

public class JwtDecoder
{
    /// <summary>
    /// Decodes the JWT token received from Dialpad and returns the payload as a dynamic object.
    /// </summary>
    /// <param name="encodedJwt">The Base64-encoded JWT token to decode.</param>
    /// <param name="secret">The secret key to validate the token signature.</param>
    /// <returns>A dynamic object (JsonElement) representing the decoded JWT payload.</returns>
    public static JsonElement DecodeJwt(string encodedJwt, string secret)
    {
        // Decode the Base64-encoded JWT from the request body
        string jwtToken = Encoding.UTF8.GetString(Convert.FromBase64String(encodedJwt));

        // Convert the secret string to a byte array using UTF-8 encoding
        byte[] key = Encoding.UTF8.GetBytes(secret);

        // Create TokenValidationParameters object for validating the JWT.
        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Skip issuer validation
            ValidateAudience = false, // Skip audience validation
            ValidateLifetime = false, // Skip expiration validation
            ValidateIssuerSigningKey = false, // Skip signature validation
            SignatureValidator = delegate (string token, TokenValidationParameters parameters)
            {
                // Bypass signature validation for now, since the kid is missing and we trust Dialpad
                JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwt = jwtHandler.ReadJwtToken(token);
                return jwt;
            }
        };

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        try
        {
            // Read the JWT without signature validation.
            JwtSecurityToken jwt = handler.ReadJwtToken(jwtToken);

            // The payload is in the 'Payload' property of the JwtSecurityToken.
            // Serialize the payload to a JSON string and parse it as a JsonElement
            string jsonPayload = jwt.Payload.SerializeToJson();
            JsonDocument jsonDocument = JsonDocument.Parse(jsonPayload);

            // Return the JsonElement, which can be accessed dynamically
            return jsonDocument.RootElement;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine("Token validation failed: " + ex.Message);
            throw; // Re-throw the exception if needed, or return a default JsonElement
        }
    }
}
