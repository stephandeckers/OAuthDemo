# OAuth Demo Certificates

## Certificate Information

- **Certificate Files:**
  - `oauth-demo.crt` - Public certificate
  - `oauth-demo.key` - Private key
  - `oauth-demo.pfx` - PKCS#12 certificate bundle (for .NET applications)

- **Password:** `OAuthDemo2026!`

- **Validity Period:** 100 years (until December 24, 2125)

- **Subject:** `/C=US/ST=State/L=City/O=OAuthDemo/CN=localhost`

## Usage

The `oauth-demo.pfx` file is used for certificate-based OAuth authentication in the OAuthDemo project.

### In .NET Applications

```csharp
var certificate = new X509Certificate2("path/to/oauth-demo.pfx", "OAuthDemo2026!");
```

## Security Note

This is a self-signed certificate intended for development and demonstration purposes only. 
Do not use in production environments.
