// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using System.Security.Claims;
using Duende.IdentityServer.Test;

namespace CarvedRock.Identity;

public static class TestUsers
{
    public static List<TestUser> Users
    {
        get
        {
            return
            [
                new TestUser
                {
                    SubjectId = "1",
                    Username = "erik",
                    Password = "erik",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Erik Dahl"),
                        new Claim(JwtClaimTypes.GivenName, "Erik"),
                        new Claim(JwtClaimTypes.FamilyName, "Dahl"),
                        new Claim(JwtClaimTypes.Email, "ErikDahl@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://erik.com"),
                        new Claim(JwtClaimTypes.Role, "admin")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "bob",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Role, "user")
                    }
                }
            ];
        }
    }
}