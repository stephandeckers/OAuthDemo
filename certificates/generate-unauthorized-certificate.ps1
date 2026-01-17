# Generate Self-Signed Certificate for OAuth Demo - UNAUTHORIZED VERSION
# This certificate is intentionally different to prove OAuth validation works

$certPassword = "Unauthorized2026!"
$certName = "UnauthorizedClientCert"
$validityDays = 825  # Maximum recommended validity for modern security standards
$outputPath = $PSScriptRoot

Write-Host "Generating unauthorized self-signed certificate..." -ForegroundColor Green
Write-Host "Certificate Name: $certName" -ForegroundColor Cyan
Write-Host "Validity: $validityDays days" -ForegroundColor Cyan
Write-Host "Password: $certPassword" -ForegroundColor Yellow
Write-Host "NOTE: This certificate will NOT be accepted by OAuthApi" -ForegroundColor Red

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
$pfxPath = Join-Path $outputPath "Unauthorized.pfx"
$securePassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null
Write-Host "Exported PFX: $pfxPath" -ForegroundColor Green

# Export to CER (public key only)
$cerPath = Join-Path $outputPath "Unauthorized.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Exported CER: $cerPath" -ForegroundColor Green

# Create certificate details file
$detailsPath = Join-Path $outputPath "unauthorized-certificate-details.txt"
$details = @"
OAuth Demo Unauthorized Certificate Details
============================================
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Certificate Information:
- Subject: CN=$certName
- Thumbprint: $($cert.Thumbprint)
- Valid From: $($cert.NotBefore)
- Valid To: $($cert.NotAfter)
- Validity Period: $validityDays days

Files Generated:
- Unauthorized.pfx (Private key + Certificate) - Password: $certPassword
- Unauthorized.cer (Public key only)

Usage:
This certificate is intentionally NOT configured in OAuthApi's appsettings.json.
It is used to demonstrate that OAuth validation is working correctly by:
1. Attempting to authenticate with this certificate
2. Receiving an "Unauthorized" response from OAuthApi
3. Proving that only authorized certificates can obtain JWT tokens

IMPORTANT: This proves OAuth security is working!
"@

$details | Out-File -FilePath $detailsPath -Encoding UTF8
Write-Host "Certificate details saved: $detailsPath" -ForegroundColor Green

# Remove from certificate store (optional - keep it for testing)
# Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force

Write-Host "`nUnauthorized certificate generation complete!" -ForegroundColor Green
Write-Host "Password: $certPassword" -ForegroundColor Yellow
Write-Host "`nThis certificate will be REJECTED by OAuthApi - proving OAuth works!" -ForegroundColor Red
