# Generate Self-Signed Certificate for OAuth Demo
# This script creates a certificate with maximum recommended validity (825 days)

$certPassword = "OAuthDemo2026!"
$certName = "OAuthDemoClientCert"
$validityDays = 825  # Maximum recommended validity for modern security standards
$outputPath = $PSScriptRoot

Write-Host "Generating self-signed certificate..." -ForegroundColor Green
Write-Host "Certificate Name: $certName" -ForegroundColor Cyan
Write-Host "Validity: $validityDays days" -ForegroundColor Cyan
Write-Host "Password: $certPassword" -ForegroundColor Yellow

# Create the certificate
$cert = New-SelfSignedCertificate `
    -Subject "CN=$certName" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -KeySpec Signature `
    -KeyLength 2048 `
    -KeyAlgorithm RSA `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddDays($validityDays) `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") # Client Authentication

Write-Host "Certificate created successfully!" -ForegroundColor Green
Write-Host "Thumbprint: $($cert.Thumbprint)" -ForegroundColor Cyan

# Export to PFX (with private key)
$pfxPath = Join-Path $outputPath "OAuthDemo.pfx"
$securePassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null
Write-Host "Exported PFX: $pfxPath" -ForegroundColor Green

# Export to CER (public key only)
$cerPath = Join-Path $outputPath "OAuthDemo.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Exported CER: $cerPath" -ForegroundColor Green

# Create certificate details file
$detailsPath = Join-Path $outputPath "certificate-details.txt"
$details = @"
OAuth Demo Certificate Details
==============================
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Certificate Information:
- Subject: CN=$certName
- Thumbprint: $($cert.Thumbprint)
- Valid From: $($cert.NotBefore)
- Valid To: $($cert.NotAfter)
- Validity Period: $validityDays days

Files Generated:
- OAuthDemo.pfx (Private key + Certificate) - Password: $certPassword
- OAuthDemo.cer (Public key only)

Usage:
- OAuthApi: Uses the public key (CER) to validate client certificates
- OAuthClient: Uses the private key (PFX) to authenticate

IMPORTANT: Keep the PFX file and password secure!
"@

$details | Out-File -FilePath $detailsPath -Encoding UTF8
Write-Host "Certificate details saved: $detailsPath" -ForegroundColor Green

# Remove from certificate store (optional - keep it for testing)
# Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force

Write-Host "`nCertificate generation complete!" -ForegroundColor Green
Write-Host "Password: $certPassword" -ForegroundColor Yellow
Write-Host "`nIMPORTANT: Save this password - you'll need it for the OAuthClient configuration" -ForegroundColor Red
