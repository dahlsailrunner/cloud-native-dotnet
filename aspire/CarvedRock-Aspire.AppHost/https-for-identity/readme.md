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
kubectl create secret tls cr-identity-tls --cert=cr-id-local.crt --key=cr-id-local.key -n carvedrock
```

## 3. Update the Identity Ingress to Use the secret

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

import crt into trusted root authorities

## 4. The Rabbit Hole

If we really wanted to get this entire application working, we'd need to update the
webapp and api containers -- they would both need to:

* Have the `crt` file added to the container image and trusted
* Get a dns entry created for the external endpoint

OR

We could set up a certificate on the identity server itself
and then trust it inside the container images.

Either way is beyond the scope of this course, code, and exercise.

Using a real identity server would most often be deployed into a
production or more real cluster that provides both certificate management
and dns management.

Then you would simply use configuration and / or secrets using the same
techniques we've already seen and will continue to see.
