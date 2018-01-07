# Serverito

Http framework for C# web apps.

## Why

Sometimes you want to build simple web apps in C# without using the extensive ASP.net or IIS frameworks. 
For that purpose, .Net provide a very useful class, [HttpListener](https://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx).

While its quite easy to use, the problem with ```HttpListener``` is that you need to implement a lot of basic things on your own: serving static files, mapping URLs, writing output to responses, etc.

To solve that, *Serverito* implement a wrapping layer around ```HttpListener``` that provide a very easy API, and do most of the tedious work for you.

**It takes minutes to build web apps with *Serverito*!**

## Install

Install Serverito via NuGet:

```
Install-Package Serverito
```

Or check it out on [nuget.org](https://www.nuget.org/packages/Serverito/).

## Using Serverito

Lets create a *Serverito* server on localhost, add a test URL, and start listening:

```cs
// don't forget to add 'using Serverito;'

// create server
ServeritoListener server = new ServeritoListener("http://localhost:8000/");

// add a test url
server.AddView(new URL("/"), (ServeritoContext context) =>
{
	Utils.WriteToResponse(context.Context, "Hello World!");
});

// start listening
server.Start();
```

In the example above we created a server, added a 'view' to it (mapping between URL and a function to handle it), and started listening to incoming requests.

If you run your app you'll see that its blocking on the ```server.Start()``` line. This means your listener is working and ready to handle incoming requests. 

If you open a browser and go to URL ```http://localhost:8000/```, you should now see ```Hello World!``` printed on the screen.

### Urls and Views

As you saw in previous example, to use *Serverito* you need to map different URLs to handling functions (aka 'Views') which are responsible to render the response.

When *Serverito* receive an incoming request, the listener will iterate over all the views you previously defined and use the first one that the request URL matches its pattern (this means that the order you put them is important). 

In the example above the URL pattern was quite simple, only accepting '/', but URLs can be more sophisticated than that. For example, the following view:

```cs
server.AddView(new URL("/post/", HttpMethods.POST, UrlMatchingType.StartsWith), (ServeritoContext context) =>
{
	Utils.WriteToResponse(context.Context, "Hello World!");
});
```

Will run only for POST requests if URL starts with "/post/". 
You can also use Regex in URLs:

```cs
server.AddView(new URL(@"/number/\d+/", matchType: UrlMatchingType.RegEx), (ServeritoContext context) =>
{
	Utils.WriteToResponse(context.Context, "Hello World!");
});
```

The example above will match any http request type, for all URLs that match the pattern of '/number/<number>/' (<number> can be any length number).

### Serving Static Files

Usually in production you want to serve static files from your web server layer (eg nginx, apache, etc.) and not from your app. However, if you want to serve static files from *Serverito*, its easy to do (useful for development process).

To serve static files automatically, you only need to set two properties in your server:

```
server.StaticFilesRootUrl = "/static/";
server.StaticFilesPath = "../../static_files";
```

```StaticFilesRootUrl``` is the root URL used to serve static files. In the example above, whenever someone enters a URL like '/static/something/', the server will understand he's looking for a file and will try to serve him the content of 'something'.

```StaticFilesPath``` is the path on your machine of the static files. When the server need to serve a static file, it will search for it under this path.

So if we look at the example above, if a user goes to URL ```/static/hello.txt```, the server will look for file ```../../static_files/hello.txt``` (relative to current working directory) and try to serve it.

If file not found, 404 error code will be returned.

#### Mime Types

*Serverito* handle mime-types automatically by setting the content-type header based on file extension. To disable this behavior, set:

```
server.SetMimeContentType = false;
```

#### Encoding

To choose what encoding type to use with static files, you can set the `StaticFilesEncodingType` property:

```cs
server.StaticFilesEncodingType = EncodingType.UTF8;
```

This will set the 'charset' property in the content-type header.

#### Smarter File Handling

By default whenever *Serverito* need to serve a file, it just reads the file bytes and write them to response. If you want to use caching mechanisms or have a more sophisticated logic, you can override the function that reads a file:

```cs
server.StaticFilesReader = SmartReadingFunc;
```


### Rendering Html

To serve an HTML page use ```server.ServeHtmlPage```:

```cs
server.AddView(new URL("/"), (ServeritoContext context) =>
{
	server.ServeHtmlPage(context, "test.html");
});
```

Note that this uses the static files mechanism, which means that you have to set ```StaticFilesPath``` for it to work (the server will look for `test.html` under the path you defined as StaticFilesPath).

### Useful Config

The *Serverito* server comes with some useful config properties you should know:

##### server.UseThreads [default: false]

When true, the server will open a new thread for every incoming request.

##### CloseRequests [default: true]

When true, the server will close responses automatically whenever its most fitting. Closing the response is what actually fires it back to client.

If you want to control when to close the responses, set this to false.

##### UseChunks [default: true]

If true, will send data in chunks.

##### SetMimeContentType [default: true]

If true, will set content-type automatically for known file types whenever serving static files.

##### StaticFilesEncodingType [default: EncodingType.UTF8]

What encoding type to use for files we serve.

### Utils

```Utils``` is a static class with useful utilities to help you handle requests and setup the server. It has lots of useful stuff, but the following are the most important functions you should know:

##### Utils.FileToBytes

Read a file into bytes buffer.

##### Utils.GetIp

Get IP address (as string) from request.

##### Utils.ReadRequestInput

Read request input stream and return it as string. If you're using JSON with your APIs you need a JSON lib to convert to objects.

##### Utils.WriteToResponse

Write string directly to response.

##### Utils.ForceTrailingSlash

Make your server force users to use URLs with trailing slashes (except for static files).

##### Utils.DumpExceptionsToResponse

Make your server dump all exceptions to response.

### Events

To make the server more flexible, it features a set of events you can register and use to process requests while they go through the pipes.

You can listen to the following events:

- OnException: called whenever an exception occurs.
- OnFinishedProcessingView: called right after a view successfully runs.
- OnFinishHandlingRequest: called after a request is fully handled and ready to be closed.
- OnNewRawRequest: called when we get a new request, before we start processing it.
- OnPassingRequestToView: called before we pass the request to the matching view.
- OnServingFile: called whenever a static file is served.
- OnUndefinedURL: called whenever we can't find a matching view for a request URL.
- OnMissingFile: called whenever we can't find a static file we need to serve.
- OnUrlMatching: called before we start matching URLs for a new request (eg before we decide which view to use).

All the events above get the ```ServeritoContext``` as their param. You can use ```ServeritoContext.UserData``` to pass data between them.

#### Controlling Flow

The callbacks you register can control the flow of the request by throwing some special exceptions:

- BreakCallbacks: throwing this exception will skip the following event handlers for this specific event.
- AbortRequest: will abort the request and stop processing it.
- StopProcessingRequest: will stop processing request, but won't abort it. if you close request manually, it will be a valid response.

## Example

If you clone this repository and build the project as a console application instead of a class library, you will get a simple example app that renders a test page and defines some test views.

## Changes

#### 1.0.0.1

Initial release.

#### 1.0.0.2

- Fixed bug in setting mime-type automatically.
- Added support in changing static files encoding type.
- Added charset header to static files we serve.

## Contact

For bug report, questions or feature requests, please use the [GitHub Issues](https://github.com/RonenNess/Serverito/issues/) section.

For anything else, feel free to contact me directly at [ronenness@gmail.com](mailto:ronenness@gmail.com).


## License

*Serverito* is distributed under the permissive MIT license.