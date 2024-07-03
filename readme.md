# Cloud Native Development with .NET

This repo will show different approaches to doing development that will target cloud native deployments.

## Solution Overview

```mermaid
C4Context
    System(WebApp, "Web Application", "Razor pages eCommerce and admin app.")
    System(Api, "API", "ASP.NET Core WebApi")
    System(IdSrv, "Identity", "AuthN service using Duende IdentityServer.")

    Boundary(b2, "Services") {
      System_Ext(Email, "Email", "Smtp4Dev")
      SystemDb_Ext(Db, "Database", "Postgres")
      System_Ext(Logging, "Logging", "Seq")
    }
      
    BiRel(WebApp, Api, "Uses")
    Rel(WebApp, IdSrv, "Authencation", "OIDC")
    Rel(Api, IdSrv, "Authencation", "OAuth2")
    Rel(Api, Db, "Uses")
    Rel(WebApp, Email, "Sends e-mails", "SMTP")
    Rel(WebApp, Api, "API calls for data", "HTTP/JSON")
      
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```
