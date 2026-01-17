#!/bin/bash
# Generate Self-Signed Certificate for OAuth Demo - UNAUTHORIZED VERSION
# This certificate is intentionally different to prove OAuth validation works

CERT_PASSWORD="Unauthorized2026!"
CERT_NAME="UnauthorizedClientCert"
VALIDITY_DAYS=825
OUTPUT_PATH="$(cd "$(dirname "$0")" && pwd)"

echo "Generating unauthorized self-signed certificate..."
echo "Certificate Name: $CERT_NAME"
echo "Validity: $VALIDITY_DAYS days"
echo "Password: $CERT_PASSWORD"
echo "NOTE: This certificate will NOT be accepted by OAuthApi"

# Generate private key and certificate
openssl req -x509 -newkey rsa:2048 -keyout "$OUTPUT_PATH/Unauthorized.key" \
    -out "$OUTPUT_PATH/Unauthorized.crt" \
    -days $VALIDITY_DAYS \
    -passout pass:$CERT_PASSWORD \
    -subj "/CN=$CERT_NAME"

echo "Certificate created successfully!"

# Get certificate thumbprint (SHA1 fingerprint)
THUMBPRINT=$(openssl x509 -in "$OUTPUT_PATH/Unauthorized.crt" -noout -fingerprint -sha1 | cut -d'=' -f2 | tr -d ':')
echo "Thumbprint: $THUMBPRINT"

# Export to PFX (with private key) - combines cert and key
openssl pkcs12 -export -out "$OUTPUT_PATH/Unauthorized.pfx" \
    -inkey "$OUTPUT_PATH/Unauthorized.key" \
    -in "$OUTPUT_PATH/Unauthorized.crt" \
    -passin pass:$CERT_PASSWORD \
    -passout pass:$CERT_PASSWORD

echo "Exported PFX: $OUTPUT_PATH/Unauthorized.pfx"

# Export to CER (public key only) - convert to DER format
openssl x509 -in "$OUTPUT_PATH/Unauthorized.crt" -outform DER -out "$OUTPUT_PATH/Unauthorized.cer"
echo "Exported CER: $OUTPUT_PATH/Unauthorized.cer"

# Get certificate validity dates
VALID_FROM=$(openssl x509 -in "$OUTPUT_PATH/Unauthorized.crt" -noout -startdate | cut -d'=' -f2)
VALID_TO=$(openssl x509 -in "$OUTPUT_PATH/Unauthorized.crt" -noout -enddate | cut -d'=' -f2)

# Create certificate details file
cat > "$OUTPUT_PATH/unauthorized-certificate-details.txt" <<EOF
OAuth Demo Unauthorized Certificate Details
============================================
Generated: $(date '+%Y-%m-%d %H:%M:%S')

Certificate Information:
- Subject: CN=$CERT_NAME
- Thumbprint: $THUMBPRINT
- Valid From: $VALID_FROM
- Valid To: $VALID_TO
- Validity Period: $VALIDITY_DAYS days

Files Generated:
- Unauthorized.pfx (Private key + Certificate) - Password: $CERT_PASSWORD
- Unauthorized.cer (Public key only)
- Unauthorized.crt (Certificate in PEM format)
- Unauthorized.key (Private key in PEM format)

Usage:
This certificate is intentionally NOT configured in OAuthApi's appsettings.json.
It is used to demonstrate that OAuth validation is working correctly by:
1. Attempting to authenticate with this certificate
2. Receiving an "Unauthorized" response from OAuthApi
3. Proving that only authorized certificates can obtain JWT tokens

IMPORTANT: This proves OAuth security is working!
EOF

echo "Certificate details saved: $OUTPUT_PATH/unauthorized-certificate-details.txt"
echo ""
echo "Unauthorized certificate generation complete!"
echo "Password: $CERT_PASSWORD"
echo ""
echo "This certificate will be REJECTED by OAuthApi - proving OAuth works!"
