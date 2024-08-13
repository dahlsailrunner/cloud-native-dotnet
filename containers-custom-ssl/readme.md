## Getting Started

* Set `docker-compose` as the startup project
* Add a `HOSTS` file entry.
   Here's the entry for `c:\windows\system32\drivers\etc\hosts`:

```hosts
127.0.0.1	carvedrock.identity
```
* Open the `cert-creation/cr-id-local.crt` file and import it
to your **Trusted Root Certificate Authority** store.
* Run it! :)


**N O T E:** If you follow the instructions "from scratch" in the
`cert-creation/readme.md` file, you will need to import the
updated certificate instead.


### Application URLs

All can be seen as the port mappings in the `docker-compose.yml` file

* Web App: [https://localhost:32071](https://localhost:32071) (This should launch automatically when you run the project from Visual Studio)
* API: [https://localhost:32081](https://localhost:32081)
* Identity Server: [https://carvedrock.identity:8091](https://localhost:32091)
* Seq: [http://localhost:5330](http://localhost:5330)
* Smtp4Dev: [http://localhost:5010](http://localhost:5010)

Postgres is accessible from the host on port 5444 (also defined in `docker-compose.yml`).


