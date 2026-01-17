# OAuth Certificate-Based Authentication Demo

This project demonstrates OAuth certificate-based authentication between two .NET 8 Web API applications.

## Project Structure

- **OAuthApi**: The API server that hosts protected endpoints
- **OAuthClient**: The client application that generates JWT tokens and calls OAuthApi
- **certs/**: Contains the self-signed certificate used for JWT signing and validation

## Prerequisites

- .NET 8.0 SDK or later
- OpenSSL (for certificate generation, already included)

## Certificate Information

The project includes a self-signed certificate for development purposes:
- **Location**: `certs/oauth-demo.pfx`
- **Password**: `OAuthDemo2026!`
- **Validity**: 100 years (until 2125)
- **Usage**: JWT token signing and validation

See [certs/README.md](certs/README.md) for more details.

## Getting Started

### 1. Build the Projects

```bash
# Build OAuthApi
cd OAuthApi
dotnet build

# Build OAuthClient
cd ../OAuthClient
dotnet build
```

### 2. Run the Applications

You need to run both applications simultaneously. Open two terminal windows:

**Terminal 1 - Run OAuthApi (port 7001):**
```bash
cd OAuthApi
dotnet run
```

**Terminal 2 - Run OAuthClient (port 7002):**
```bash
cd OAuthClient
dotnet run
```

### 3. Access the Applications

- **OAuthApi Swagger UI**: https://localhost:7001/swagger
- **OAuthClient Swagger UI**: https://localhost:7002/swagger

## API Endpoints

### OAuthApi Endpoints

1. **GET /WeatherForecast/Get1** - Returns 5 weather forecasts (no authentication required)
2. **GET /WeatherForecast/Get2** - Returns 7 weather forecasts (no authentication required)
3. **GET /WeatherForecast/GetSecured** - Returns 10 weather forecasts (OAuth authentication required)

### OAuthClient Endpoints

1. **GET /OAuthDemo/CallGet1** - Calls OAuthApi Get1 endpoint without authentication
2. **GET /OAuthDemo/CallGet2** - Calls OAuthApi Get2 endpoint without authentication
3. **GET /OAuthDemo/CallGetSecured** - Generates JWT token and calls OAuthApi GetSecured endpoint
4. **GET /OAuthDemo/GenerateToken** - Generates a JWT token for testing

## Testing the OAuth Flow

### Option 1: Using OAuthClient

1. Navigate to OAuthClient Swagger UI at https://localhost:7002/swagger
2. Try the following endpoints in order:
   - `CallGet1` - Should succeed without authentication
   - `CallGet2` - Should succeed without authentication
   - `CallGetSecured` - Should succeed with JWT authentication (token generated automatically)

### Option 2: Using OAuthApi Directly

1. First, generate a token from OAuthClient:
   - Navigate to https://localhost:7002/swagger
   - Execute `GET /OAuthDemo/GenerateToken`
   - Copy the token from the response

2. Use the token with OAuthApi:
   - Navigate to https://localhost:7001/swagger
   - Click the "Authorize" button at the top
   - Enter: `Bearer {your-token-here}`
   - Click "Authorize"
   - Try the `GET /WeatherForecast/GetSecured` endpoint

### Option 3: Using curl

```bash
# Call unsecured endpoints
curl https://localhost:7001/WeatherForecast/Get1 -k
curl https://localhost:7001/WeatherForecast/Get2 -k

# Generate a token
TOKEN=$(curl https://localhost:7002/OAuthDemo/GenerateToken -k -s | jq -r '.token')

# Call secured endpoint with token
curl https://localhost:7001/WeatherForecast/GetSecured -k \
  -H "Authorization: Bearer $TOKEN"
```

## How It Works

1. **Certificate Setup**: Both applications use the same X.509 certificate (`oauth-demo.pfx`)
   - OAuthClient uses it to sign JWT tokens
   - OAuthApi uses it to validate JWT token signatures

2. **Token Generation**: When OAuthClient needs to call a protected endpoint:
   - It loads the certificate
   - Creates JWT claims (subject, expiration, etc.)
   - Signs the token using the certificate's private key

3. **Token Validation**: When OAuthApi receives a request:
   - Extracts the Bearer token from the Authorization header
   - Validates the signature using the certificate's public key
   - Checks issuer, audience, and expiration
   - Grants or denies access based on validation results

## Configuration

Both applications can be configured via `appsettings.json`:

```json
{
  "Authentication": {
    "CertificatePath": "../certs/oauth-demo.pfx",
    "CertificatePassword": "OAuthDemo2026!",
    "Issuer": "OAuthDemo",
    "Audience": "OAuthApiUsers"
  }
}
```

## Security Notes

⚠️ **Important**: This is a demonstration project for development purposes only.

- The certificate is self-signed and included in the repository
- The certificate password is stored in configuration files
- In production:
  - Use certificates from a trusted CA
  - Store sensitive data in secure locations (Azure Key Vault, AWS Secrets Manager, etc.)
  - Implement proper certificate rotation
  - Use HTTPS with valid certificates
  - Implement additional security measures (rate limiting, API keys, etc.)

## Project Dependencies

### OAuthApi
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Swashbuckle.AspNetCore (6.6.2)
- System.IdentityModel.Tokens.Jwt (8.0.0)

### OAuthClient
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Swashbuckle.AspNetCore (6.6.2)
- System.IdentityModel.Tokens.Jwt (8.0.0)

## Troubleshooting

### Certificate Not Found
If you see "Certificate file not found" errors:
- Ensure the `certs/oauth-demo.pfx` file exists
- Check the `CertificatePath` in `appsettings.json`
- Verify the path is relative to the project directory

### Connection Refused
If OAuthClient cannot connect to OAuthApi:
- Ensure OAuthApi is running on port 7001
- Check the `OAuthApi:BaseUrl` setting in OAuthClient's `appsettings.json`
- Verify firewall settings

### 401 Unauthorized
If you receive 401 errors on protected endpoints:
- Ensure the token was generated successfully
- Verify both applications use the same certificate
- Check that Issuer and Audience match in both applications
- Ensure the token hasn't expired (default: 1 hour)

## License

This is a demonstration project for educational purposes.
