# RESTfulAPIAspNetCore

This project comes from https://github.com/KevinDockx/DocumentingAspNetCoreApisWithOpenAPI and makes some modifications on it.

## Interacting with Resources Through HTTP Methods

| HTTP Method | Request Payload | Sample URI | Response Payload |
| ----------- | --------------- | ---------- | ---------------- |
| GET         | -               | /api/authors | author collection |
| POST        | single author   | /api/authors | single author  |
| PUT         | single author   | api/authors/{authorId} | single author or empty |
| PATCH       | JsonPatchDocument on author | /api/authors/{authorId} | single author or empty |
| DELETE      | -               | api/authors/{authorId} | - |
| HEAD        | -               | api/authors/{authorId} | - |
| OPTIONS     | -               | api/...                | - |

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
