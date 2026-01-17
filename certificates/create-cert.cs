using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

// Create a self-signed certificate
var rsa = RSA.Create(2048);
var req = new CertificateRequest("CN=OAuthDemoClientCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.2") }, false));

var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(365));

// Export with private key
var pfxBytes = cert.Export(X509ContentType.Pfx, "OAuthDemo2026!");
File.WriteAllBytes("OAuthDemo-dotnet.pfx", pfxBytes);

Console.WriteLine($"Certificate created:");
Console.WriteLine($"Subject: {cert.Subject}");
Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
Console.WriteLine($"Valid from: {cert.NotBefore} to {cert.NotAfter}");
Console.WriteLine($"Saved to: OAuthDemo-dotnet.pfx");
