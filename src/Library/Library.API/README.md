# ASP.NET Core Api

### ApiController attribute

The [ApiController] attribute can be applied to a controller class to enable API-specific behaviors

+ Attribute routing requirement
+ Automatic HTTP 400 responses
+ Binding source parameter inference
+ Multipart/form-data request inference
+ Problem details for error status codes

To configure the behavior of the ApiController attribute, refer to [link](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.2#problem-details-for-error-status-codes).

[FluentValidation](https://github.com/JeremySkinner/FluentValidation) Tool can be used to build custom validation rules.


### `IUrlHelper` and `LinkGenerator`

`LinkGenerator` is a new service that handles generating links. This is different from the `UrlHelper` that has been used in ASP.NET MVC for a long time in that it is just an injectable service, not depending on being called in a controller or other reference to the request.

`ControllerBase.Url` is an instance of `IUrlHelper`.

## Tests

### How to mock an `HttpContext`

ASP.NET Core provide a "fake" HttpContext named `DefaultHttpContext`.

    var controller = new HomeController();
    controller.ControllerContext = new ControllerContext();
    controller.ControllerContext.HttpContext = new DefaultHttpContext();
