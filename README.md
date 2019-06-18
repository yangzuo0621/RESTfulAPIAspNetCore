# RESTfulAPIAspNetCore

This project comes from https://github.com/KevinDockx/DocumentingAspNetCoreApisWithOpenAPI and makes some modifications on it.

## Interacting with Resources Through HTTP Methods

| HTTP Method | Request Payload | Sample URI | Response Payload |
| ----------- | --------------- | ---------- | ---------------- |
| GET         | -               | /api/authors | author collection |
| POST        | single author   | /api/authors | single author  |
| PUT         | single author   | /api/authors/{authorId} | single author or empty |
| PATCH       | JsonPatchDocument on author | /api/authors/{authorId} | single author or empty |
| DELETE      | -               | /api/authors/{authorId} | - |
| HEAD        | -               | /api/authors/{authorId} | - |
| OPTIONS     | -               | /api/...                | - |

+ HTTP `PUT` method is often used to fully update resource.

+ HTTP `PATCH` method is used to partially update resrouce. [JavaScript Object Notation (JSON) Patch](https://tools.ietf.org/html/rfc6902) is a standard to represent JSON Patch. Patch requests should be sent with media type of `applcation/json-patch+json`.

## Method Safety and Method Idempotency

| HTTP Method | Safe? | Idempotent? |
| ----------- | :---: | :---------: |
| GET         |  yes  |   yes   |
| OPTIONS     |  yes  |   yes   |
| HEAD        |  yes  |   yes   |
| POST        |  no   |   no    |
| DELETE      |  no   |   yes   |
| PUT         |  no   |   yes   |
| PATCH       |  no   |   no    |

[Method Definitions](https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html)

[Architectural Styles and
the Design of Network-based Software Architectures](https://www.ics.uci.edu/~fielding/pubs/dissertation/top.htm)


## Status Codes

https://www.restapitutorial.com/httpstatuscodes.html


## Optional: Data Shape

Allow consumer to select which fields are return by API. It usually works with HATEOAS. It may break architecture design.

## HATEOAS

## Media Types

## Other Approaches and Options to RESTFul Api

+ HAL (Hypertext Application Language)
    - https://tools.ietf.org/html/draft-kelly-json-hal-08
+ SIREN (Structured Interface for Representing Entities)
    - https://github.com/kevinswiber/siren
    - https://github.com/yury-sannikov/NHateoas
+ JSON-LD (linked data format)
    - https://json-ld.org/
+ JSO-API
    - http://jsonapi.org/
+ OData
    - https://www.odata.org/


## Caching and Concurrency

### HTTP Caching
+ [RFC 2616](https://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html)
+ [RFC 7234](https://tools.ietf.org/html/rfc7234)

### Expiration Model

Expires header

+ Expires: Sat, 14 Jan 2017 15:23:40 GMT
+ clocks must be synchronised
+ Offers little control

Cache-Control header

+ Cache-Control: public, max-age=60
+ Preferred header for expiration
+ [Directives](https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html)

    Response (server side)
    - Freshness: max-age, s-maxage
    - Cache type: public, private
    - Validation: no-cache, must-revalidate, proxy-revalidate
    - Other: no-store, no-transform

    Request (client side)
    - Freshness: max-age, min-fresh, max-stale
    - Validation: no-cache
    - Other: no-store, no-transform, only-if-cached

Cache Stores
+ Private caches
    - [angular-http-etag](https://github.com/shaungrady/angular-http-etag)
    - [Marvin.HttpCache](https://www.nuget.org/packages/Marvin.HttpCache/)
    
        PCL, no .NET Core

+ Private and shared caches
    - [CacheCow.Client](https://www.nuget.org/packages/CacheCow.Client/)

+ Shared caches (.NET Core)
    - ASP.NET Core ResponseCaching middleware

        ASP.NET Web API, full .NET framework


### Validation Model

Strong validators
+ Change if the body or headers of a response change
+ ETag (Entity Tag) response header
+ ETag: "123456789"
+ Can be used in any context (equiality si guaranteed)

Weak validators
+ Don't always change when the response changes (eg: only on significant changes)
+ Last-Modified: Sat, 14 Jan 2017 15:23:40 GMT
+ ETag: "w/123456789"
+ Equivalence, but not equality

HTTP standard advice to send both ETag and Last-Modified headers if possible.

### Concurrency Strategies

+ Pessimistic concurrency

    - Resource is locked
    - while it's locked, it cannot be modified by another client
    - This is not possible in REST

+ Optimistic concurrency

    - Token is returned together with the resource
    - The update can happen as long as the token is still valid
    - ETags are used as validation tokens

## Load testing tools

+ [West Wind WebSurge](https://websurge.west-wind.com/)