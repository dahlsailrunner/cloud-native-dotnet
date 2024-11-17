# Creating an SSL Certificate Using `openssl`

## 0. Create or update the `conf` file to be used as input

The file you create or update should look something like this:

```conf
[req]
prompt             = no
default_bits       = 2048
distinguished_name = req_distinguished_name
req_extensions     = req_ext
x509_extensions    = v3_ca

[req_distinguished_name]
countryName                 = US
stateOrProvinceName         = Minnesota
localityName                = St. Paul
organizationName            = Carved Rock
organizationalUnitName      = Development
commonName                  = auth.carvedrock.local

[req_ext]
subjectAltName = @alt_names

[v3_ca]
subjectAltName = @alt_names

[alt_names]
DNS.1   = auth.carvedrock.local
```

## 1. Use `openssl` to create `crt` and `key` files

The command below references a `cr-id.conf` file that should already exist - with the contents from step 0 above.

```bash
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout cr-id-local.key -out cr-id-local.crt -config cr-id-local.conf
```

## 2. Create a Kubernetes TLS Secret

```bash
kubectl create secret tls cr-identity-tls --cert=cr-id-local.crt --key=cr-id-local.key -n kyt-app
```

## Update the Identity Ingress to Use the secret

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ingress-cr-identity
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  ingressClassName: nginx
  tls: 
  - hosts:
    - auth.carvedrock.local
    secretName: cr-identity-tls 
  rules:
  - host: auth.carvedrock.local        
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: carvedrock-identity
            port: 
              number: 8080
```

Make sure to apply the changes!  `aspirate apply`

## 3. Trust the Certificate

hosts file entry
import crt into trusted root authorities
