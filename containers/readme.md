## Getting Started

Set the `docker-compose` project as the startup project.

Run it! :)

URLs (all can be seen as the port mappings in the `docker-compose.yml` file):
* Web App: [https://localhost:32071](https://localhost:32071) (This should launch automatically when you run the project from Visual Studio)
* API: [https://localhost:32081](https://localhost:32081)
* Identity Server: [https://localhost:32091](https://localhost:32091)
* Seq: [http://localhost:5330](http://localhost:5330)
* Smtp4Dev: [http://localhost:5010](http://localhost:5010)

Postgres is accessible from the host on port 5444 (also defined in `docker-compose.yml`).

## Notes

**The app won't fully work.**  You can see the main home page - 
and you can navigate to the different product pages (Footwear, Equipment,
Kayaks). Anonymous calls to the API will work, which means the
database is fine too.  And logs will get to Seq, which you can explore.

But **logging in won't work**.  The Identity server is running,
but the certificate ONLY applies to `localhost`. So when the app
(running inside a container, not on localhost) tries to communicate
securely with the IdentityServer, it will fail.

* It won't be able to resolve `https://localhost:32091` since on the container
   localhost is the container itself, not the host machine.
* Referring to `https://host.docker.internal:32091` won't work either, since
   the certificate is only valid for `localhost`.
