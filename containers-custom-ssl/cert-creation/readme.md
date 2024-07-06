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
commonName                  = Carved Rock Local IdentityServer

[req_ext]
subjectAltName = @alt_names

[v3_ca]
subjectAltName = @alt_names

[alt_names]
DNS.1   = carvedrock.identity
```

## 1. Use `openssl` to create `crt` and `key` files

The command below references a `cr-id.conf` file that should already exist - with the contents from step 0 above.

```bash
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout cr-id-local.key -out cr-id-local.crt -config cr-id-local.conf
```

## 2. Export a `pfx` that you can use in an ASP.NET Core project

```bash
sudo openssl pkcs12 -export -out cr-id-local.pfx -inkey cr-id-local.key -in cr-id-local.crt
```

You will be prompted for an "export" password and will need to confirm the entry. This password
is needed when opening / using the `pfx` file.

I used `Learning1sGreat!` for the password in this example.

## 3. Trust the Certificate

hosts file entry
import crt into trusted root authorities
update dockerfiles
