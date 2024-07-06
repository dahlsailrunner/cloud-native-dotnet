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
commonName                  = id-local.carvedrock.com

[req_ext]
subjectAltName = @alt_names

[v3_ca]
subjectAltName = @alt_names

[alt_names]
DNS.1   = id-local.carvedrock.com
```

## 1. Use `openssl` to create `cer` and `key` files

The command below references a `cr-id.conf` file that should already exist - with the contents from step 0 above.

```bash
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout cr-id-local.key -out cr-id-local.cer -config cr-id-local.conf -passin pass:Learning1sGreat!
```

## 2. Export a `pfx` that you can use in an ASP.NET Core project

```bash
sudo openssl pkcs12 -export -out cr-id-local.pfx -inkey cr-id-local.key -in cr-id-local.cer
```

## 3. Trust the Certificate

hosts file entry
import cer into trusted root authorities
