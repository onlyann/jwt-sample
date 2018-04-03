# Sample JSON Web Tokens (JWT) authentication in ASP.NET Core

## Introduction

Sample API that is protected by JWT authentication (actually a subset of JWT) and policy authorization in ASP.NET Core 2.0

`/login` accepts POST requests as either `application/json` or `application/x-www-form-urlencoded` with username/password. Valid credentials result in an HTTP 200 with the JWT token in the body of the response.

Any other path requires a valid JWT Bearer token and will either return 401 or a welcome message.

It has no dependency on ASP.NET Core MVC or ASP.NET Core Identity.

## What is JWT and should I use it?

Start here https://jwt.io/introduction/.
The RFC standards can be found at https://tools.ietf.org/html/rfc7519.

## Getting Started

```
git clone https://github.com/onlyann/jwt-sample
cd jwt-sample/src
dotnet restore
dotnet run
```

The application is running on port 5000.

Verify that you get HTTP 401 without token:
```
curl http://localhost:5000/foo -v
```

Obtain a JWT token:
```
curl -d "username=admin&password=one-jwt-token-please" http://localhost:5000/login
```

Access the protected resource:
```
curl -H "Authorization: Bearer REPLACE_WITH_TOKEN" http://localhost:5000/foo
```

If using VS Code, you can also install the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension and run requests from the [./tests/requests.rest](./tests/requests.rest) file.

## Configuration

The application accepts configuration values from either the 
*appSettings.json* file or from environment variables:

- **Issuer** : The issuer of the JWT token.
- **Audience**: The audience of the JWT token.
- **Algorithm**: The algorithm used for signing JWT tokens. Default is `HS256`. Supported algorithms are `HS256`, `HS384`, `HS512`, `RS256`, `RS384` and `RS512`.
- **TokenLifetimeInMinutes**: Number of minutes the JWT is valid for. Default is 30.
- **ClockSkew**: Clock skew adjustment when validating token. Default is 5 seconds.
- **SecretBase64**: Base64 encoded key for HMAC-SHA algorithms. If not present, a random key is generated at startup.
- **RsaKeysXml**: XML representation of the RSA key pair. If not present, a random key is generated at startup.

## Additional notes on security and JWT

HTTPS is mandatory for any production system and should be enforced in the code (not done here).

The `LoginMiddleware` is a naive implementation with hard-coded credentials for demo purposes.

When the JWT authentication is stateless (like in this demo), the tokens can't be invalidated. For security reasons, their lifetime tends to be short.

When a token expires, the user needs to reauthenticate again.  
To prevent users from authenticating too often, some applications introduce *refresh tokens*.  
*refresh tokens* are returned with the JWT access token and have a longer lifespan.  
The idea is that JWT tokens get sent on every request while the refresh tokens only get sent once in a while to obtain a new access token (hence the term refresh token).
The refresh tokens are sometimes JWT tokens or can be opaque tokens.

When the revocation of specific tokens is necessary, some applications introduce blacklisting/whitelisting of tokens or some security stamp associated with the token claims (at the cost of additional complexity).

It is recommended to have automatic rotation of keys. This is supported by the underlying
ASP.NET Core API but is not exposed in this sample app.  
If the application is hosted on Azure, [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-whatis) can help managing the keys.

A common use case for JWT is to have an *Auth* API that issues access tokens. This *Auth* API would accept credentials, third-party social logins, api keys, ...

The user can then access protected API resources that are configured to authenticate users from the JWT access token in a stateless manner.

In such a scenario, the asymmetrical RSA-SHA algorithm is useful as protected resources only need to know the public key.  
The private key remains on the *Auth* API.
