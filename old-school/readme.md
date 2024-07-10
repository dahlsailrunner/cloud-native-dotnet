## Getting Started

You should set the `Api`, `WebApp`, and `Identity` projects as startup projects. In Visual
Studio this can be done by "Configure Startup Projects", then choosing "Multiple startup projects"
(set the Api, WebApp, and Identity projects to Start).

The solution uses three services that it expects to be running locally. The simplest way to
run these services is to use Docker.  The following commands will start the services.

```bash
docker pull rnwood/smtp4dev
docker run --rm -d --name fakemail -p 3000:80 -p 2525:25 rnwood/smtp4dev

docker pull datalust/seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq

docker pull postgres
docker run -d --name carvedrock-postgres -e POSTGRES_PASSWORD=carvedrock -p 5432:5432 postgres
```

### Application URLs:
* User Interface / Web App: [https://localhost:7224/](https://localhost:7224/)
* API: [https://localhost:7213/](https://localhost:7213/)
* IdentityServer / Authentication: [https://localhost:5001/](https://localhost:5001/)
* Logs / Seq: [http://localhost:5341](http://localhost:5341)
* Email browser: [http://localhost:3000](http://localhost:3000)

If you want to explore the data, the Postgres connection string is in the `appSettings.json` file of the API project.

## Features

This is a simple e-commerce application that has a few features
that we want to explore automated testing strategies for.

Here are the features:

- **API**
  - `GET` based on category (or "all") and by id allow anonymous requests
  - `POST` requires authentication and an `admin` role
  - Validation will be done with [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) and errors returned as a `400 Bad Request` with `ProblemDetails`
  - A `GET` with a category of something other than "all", "boots", "equip", or "kayak" will return a `500 internal server error` with `ProblemDetails`
  - Data is refreshed to a known state as the app starts
- Authentication provided by OIDC via the [demo Duende Identity Server](https://demo.duendesoftware.com)
- A custom claims transformer will add the `admin` role to "Bob Smith" and any authentication via Google
- **WebApp**
  - The home page and listing pages will show a subset of products
  - There is a page at `/Admin` that will show a list of products that can be added to (edit and delete not implemented)
  - If you navigate to `/Admin` without the admin role, you should see an `AccessDenied` page
  - Any validation errors from the API should be displayed on the admin section add page
  - Can add items to cart and see a summary of the cart (shows when empty too)
  - Can submit an order or cancel the order and clear the cart
  - A submitted order will send a fake email

## VS Code Setup

Running in VS Code is a totally legitimate use-case for this solution and
repo.

The same instructions above (Getting Started) apply here, but the following
extension should probably be installed (it includes some other extensions):

- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Then run the API project, the UI project, and the Identity project.

## Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command was used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The initial setup for the application uses SQLite.
The data will be stored in a file called `carvedrock-sample.sqlite` as
defined in the API project's `appsettings.json` file.

The location of the file is in the "local AppData" folder (`Environment.SpecialFolder.LocalApplicationData`):

- Windows: `C:\Users\<username>\AppData\Local\`
- Mac: `/Users/USERNAME/.local/share`


### Switching Databases

Switching between Postgres and SQL Server is a matter
of commenting out the unused dbcontext setup / initialization logic and
commenting in what you want:

- `Program.cs` of the API project

You also need to manually delete the `CarvedRock.Data.Migrations`
folder and recreate the migrations using the instructions above.

Running SQL Server for local development:

```bash
docker pull mcr.microsoft.com/mssql/server
docker run -d --name carvedrock-sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Carvedr0ck!" -p 1433:1433 mcr.microsoft.com/mssql/server
```

## Verifiying Emails

The very simple email functionality is done using a template
from [this GitHub repo](https://github.com/leemunroe/responsive-html-email-template)
and the [smtp4dev](https://github.com/rnwood/smtp4dev)
service that can easily run in a Docker container.

There is a UI that you can naviagte to in your browser for
seeing the emails that works great.  If you use the `docker run` command
that I have listed above, the UI is at
[http://localhost:3000](http://localhost:3000).

For tests, there is also an API that is part of that service and a couple of quick
API calls call give you the content of the email body that you
want to verify:

```bash
GET http://localhost:3000/api/messages

### find the ID of the message you care about

GET http://localhost:3000/api/messages/<message-guid>/html
```

