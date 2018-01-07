#region File Description
//-----------------------------------------------------------------------------
// A basic HTTP listener app to easily connect URL to handling functions.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;


namespace Serverito
{
    /// <summary>
    /// Request context inside 'Serverito' app.
    /// Basically this is just the 'HttpListenerContext' object wrapped with some metadata.
    /// </summary>
    public class ServeritoContext
    {
        /// <summary>
        /// Http listener context object.
        /// </summary>
        public HttpListenerContext Context;

        /// <summary>
        /// Optional user data you can attach to context.
        /// </summary>
        public object UserData;
    }

    /// <summary>
    /// A delegate to handle exceptins we catch from requests handlers or while getting a request.
    /// </summary>
    /// <param name="context">Context object that contains HTTP listener context + some metadata (or null if not yet valid).</param>
    /// <param name="exception">Exception we got.</param>
    public delegate void HandleExceptionsCallback(ServeritoContext context, Exception exception);

    /// <summary>
    /// A delegate to handle new requests or requests that don't match any URL.
    /// </summary>
    /// <param name="context">Context object that contains HTTP listener context + some metadata.</param>
    public delegate void HandleRequestsCallback(ServeritoContext context);

    /// <summary>
    /// A delegate to handle new requests or requests that don't match any URL.
    /// </summary>
    /// <param name="context">Context object that contains HTTP listener context + some metadata.</param>
    public delegate void ViewFunction(ServeritoContext context);

    /// <summary>
    /// A callback to handle reading files.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>File content as bytes array.</returns>
    public delegate byte[] StaticFilesReader(string path);

    /// <summary>
    /// The main Serverity class.
    /// This object listen to incoming HTTP requests and dispatch them based on defined URL and handlers.
    /// </summary>
    public class ServeritoListener
    {
        /// <summary>
        /// Current version.
        /// </summary>
        public static readonly string Version = "1.0.0.3";

        /// <summary>
        /// Our main listener.
        /// </summary>
        HttpListener _listener;

        /// <summary>
        /// An object to connect a URL pattern to a view function.
        /// </summary>
        internal struct View
        {
            /// <summary>
            /// Url pattern to match.
            /// </summary>
            public URL UrlPattern { get; private set; }

            /// <summary>
            /// View function to call.
            /// </summary>
            public ViewFunction ViewFunc { get; private set; }

            /// <summary>
            /// Create the view.
            /// </summary>
            /// <param name="url">URL pattern to match.</param>
            /// <param name="func">View function.</param>
            public View(URL url, ViewFunction func)
            {
                UrlPattern = url;
                ViewFunc = func;
            }
        }

        /// <summary>
        /// Views to process requests with.
        /// Will iterate by order of insertion and use the first view that match URL pattern.
        /// </summary>
        List<View> _views = new List<View>();

        /// <summary>
        /// Should we use threads with this listener or process everything on the same thread?
        /// When using threads a new thread will be created per request.
        /// </summary>
        public bool UseThreads = false;

        /// <summary>
        /// If true, the server app will close requests after handling them automatically.
        /// If false, you need to close them yourself.
        /// </summary>
        public bool CloseRequests = true;

        /// <summary>
        /// Should we send responses by chunks by default?
        /// </summary>
        public bool UseChunks = true;

        /// <summary>
        /// If true and serving static files, will set content type automatically for known mime types based on file extension.
        /// If false, all files will be served with content type of application/octet-stream.
        /// </summary>
        public bool SetMimeContentType = true;

        /// <summary>
        /// Convert encoding enum to string we need to set in response content type.
        /// </summary>
        static readonly string[] _encodingToCharsetString = new string[]
        {
            ";charset=utf-8",
            ";charset=utf-32",
            ";charset=utf-unicode",
            "",
        };

        /// <summary>
        /// What encoding type to set by default for static files we serve.
        /// Choose 'Default' if you don't want to set 'charset' in content-type at all.
        /// </summary>
        public EncodingType StaticFilesEncodingType = EncodingType.Default;

        /// <summary>
        /// Static files root url.
        /// Define this property if you want to automatically serve static files from a pre-defined URL.
        /// For example, if you set this to "/static/", whenever a user tries to GET from '/static/xxx/' URL, we'll look for the
        /// file xxx under StaticFilesPath and serve it. If file not found, will return 404.
        /// 
        /// Note: must end with a trailing slash.
        /// </summary>
        public string StaticFilesRootUrl = null;

        /// <summary>
        /// Static files path on server.
        /// This is the folder on this machine the server will look for static files to serve.
        /// If you define StaticFilesRootUrl you must also define this property.
        /// </summary>
        public string StaticFilesPath = null;

        /// <summary>
        /// Function used to read static files content.
        /// You can override this handler to change the way you read files (for example if you want to add files caching mechanism).
        /// </summary>
        public StaticFilesReader StaticFilesReader = Utils.FileToBytes;

        /// <summary>
        /// Optional callbacks to handle caught exceptions.
        /// </summary>
        public HandleExceptionsCallback OnException;

        /// <summary>
        /// Optional callbacks to handle new requests before we start processing them.
        /// </summary>
        public HandleRequestsCallback OnNewRawRequest;

        /// <summary>
        /// Optional callbacks to handle requests that don't match any of our URLs.
        /// </summary>
        public HandleRequestsCallback OnUndefinedURL;

        /// <summary>
        /// Optional callbacks to handle requests right before we're done with them (before closing them).
        /// </summary>
        public HandleRequestsCallback OnFinishHandlingRequest;

        /// <summary>
        /// Optional callbacks to handle requests right before we start matching the URL against views to decide which view to call.
        /// Note: this won't be called for requests to get static files.
        /// </summary>
        public HandleRequestsCallback OnUrlMatching;

        /// <summary>
        /// Optional callbacks to handle requests right before we call the view to handle them (called only if a view is found).
        /// </summary>
        public HandleRequestsCallback OnPassingRequestToView;

        /// <summary>
        /// Optional callback to handle files we serve right before closing the response.
        /// </summary>
        public HandleRequestsCallback OnServingFile;

        /// <summary>
        /// Optional callback to handle when trying to serve a non-existing file.
        /// </summary>
        public HandleRequestsCallback OnMissingFile;

        /// <summary>
        /// Optional callbacks to handle requests that we handled with a view (after the view function called).
        /// </summary>
        public HandleRequestsCallback OnFinishedProcessingView;

        /// <summary>
        /// Create a new Serverito app for a single host.
        /// </summary>
        /// <param name="host">Host to listen to (in format "protocol://domain:port", eg "http://somedomain.com:80").</param>
        public ServeritoListener(string host = "http://localhost:8000/") : this(new string[] { host })
        {
        }

        /// <summary>
        /// Create a sync server.
        /// </summary>
        /// <param name="hosts">List of hosts to listen to (in format "protocol://domain:port", eg "http://somedomain.com:80").</param>
        public ServeritoListener(string[] hosts)
        {
            // create listener and add hosts
            _listener = new HttpListener();
            foreach (var host in hosts)
            {
                _listener.Prefixes.Add(host);
            }
        }

        /// <summary>
        /// Stop listening.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
        }

        /// <summary>
        /// Start listening to incoming requests.
        /// </summary>
        public void Start()
        {
            // start listener
            _listener.Start();

            // do an endless loop that listen to requests
            while (_listener.IsListening)
            {
                // hold the context we are currently processing
                ServeritoContext context = null;

                // handle incoming requests
                try
                {
                    // get context (block until a connection comes in)
                    context = new ServeritoContext() { Context = _listener.GetContext() };

                    // set response default status code and if sending in chunks
                    context.Context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Context.Response.SendChunked = UseChunks;
                    
                    // call new requests callbacks
                    if (!InvokeCallbacks(OnNewRawRequest, context))
                        continue;
                    
                    // call the handler function using threads
                    if (UseThreads)
                    {
                        var thisContext = context;
                        ThreadPool.QueueUserWorkItem(o => HandleRequest(thisContext));
                    }
                    // call the handler function without using threads
                    else
                    {
                        HandleRequest(context);
                    }

                    // lose context pointer
                    context = null;
                }
                // Client disconnected or some other error
                catch (Exception e)
                {
                    HandleException(context, e);
                }
            }
        }

        /// <summary>
        /// Define a new view, eg a URL leading to a function to handle request.
        /// </summary>
        /// <param name="url">URL pattern to match.</param>
        /// <param name="func">Function to handle the request.</param>
        public void AddView(URL url, ViewFunction func)
        {
            _views.Add(new View(url, func));
        }

        /// <summary>
        /// Handle a single incoming request.
        /// Note: this function can run inside or outside a thread, depends on config.
        /// </summary>
        /// <param name="context"></param>
        public void HandleRequest(ServeritoContext context)
        {
            try
            {
                // handle static files
                if (IsStaticFileRequest(context.Context.Request))
                {
                    ServeStaticFile(context, context.Context.Request.RawUrl.Substring(StaticFilesRootUrl.Length));
                    return;
                }

                // call the before URL matching callback
                if (!InvokeCallbacks(OnUrlMatching, context))
                    return;

                // find view to handle this request
                bool missingUrl = true;
                foreach (var view in _views)
                {
                    // check if view match
                    if (view.UrlPattern.IsMatch(context.Context.Request.RawUrl, context.Context.Request.HttpMethod))
                    {
                        // do before-processing requests callbacks
                        if (!InvokeCallbacks(OnPassingRequestToView, context))
                            return;

                        // call view and set no longer missing url
                        view.ViewFunc(context);
                        missingUrl = false;
                    }
                }

                // didn't match any URL? set status code to 404 and call the missed URLs handler
                if (missingUrl)
                {
                    context.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    if (!InvokeCallbacks(OnUndefinedURL, context))
                        return;
                }
                // found and handled via view? invoke callbacks
                else
                {
                    // invoke end handling request callbacks
                    if (!InvokeCallbacks(OnFinishedProcessingView, context))
                        return;
                }

                // invoke end handling request callbacks
                if (!InvokeCallbacks(OnFinishHandlingRequest, context))
                    return;

                // if need to close requests, try to close it
                if (CloseRequests)
                {
                    Utils.TryCloseResponse(context.Context);
                }
            }
            // handle exceptions
            catch (Exception exp)
            {
                HandleException(context, exp);
            }
        }

        /// <summary>
        /// Get if a given context is a request to get a static file.
        /// Note: only works if you define 'StaticFilesRootUrl'.
        /// </summary>
        /// <param name="request">Request data to check.</param>
        /// <returns>If the request is for a static file.</returns>
        public bool IsStaticFileRequest(HttpListenerRequest request)
        {
            return (StaticFilesRootUrl != null &&
                request.HttpMethod == "GET" &&
                request.RawUrl.StartsWith(StaticFilesRootUrl));
        }

        /// <summary>
        /// Serve a static file.
        /// Note: will close request even if 'CloseRequests' is set to false.
        /// </summary>
        /// <param name="context">Context containing the request to serve file for.</param>
        /// <param name="path">Path of file to serve, under 'StaticFilesPath'.</param>
        /// <param name="serveHtml">If true, instead of serving file as a file it will render it as an HTML page.</param>
        public void ServeStaticFile(ServeritoContext context, string path, bool serveHtml = false)
        {
            // static files path not defined? error
            if (StaticFilesPath == null)
            {
                throw new NullReferenceException("To serve static files you must set the 'StaticFilesPath' property.");
            }

            // get file full path
            path = System.IO.Path.Combine(StaticFilesPath, path);

            // file not found? return 404
            if (!System.IO.File.Exists(path))
            {
                // call missing file callback
                if (!InvokeCallbacks(OnMissingFile, context))
                    return;

                // set status code to not found and try closing request
                context.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                Utils.TryCloseResponse(context.Context);
                return;
            }

            // set default content type
            context.Context.Response.ContentType = serveHtml ? "text/html" : "application/octet-stream";

            // get filename
            var filename = System.IO.Path.GetFileName(path);

            // set and content disposition for serving html
            if (serveHtml)
            {
                context.Context.Response.Headers.Add("Content-Disposition", "inline;filename=\"" + filename + "\"");
            }
            // set and content disposition for file
            else
            {
                context.Context.Response.Headers.Add("Content-Disposition", "attachment;filename=\"" + filename + "\"");
            }

            // set content type based on known mime types
            if (!serveHtml && SetMimeContentType)
            {
                context.Context.Response.ContentType = Utils.ExtensionToMimeType(System.IO.Path.GetExtension(path)) ?? 
                    context.Context.Response.ContentType;
            }

            // set encoding type
            if (StaticFilesEncodingType != EncodingType.Default)
            {
                context.Context.Response.ContentType += _encodingToCharsetString[(int)StaticFilesEncodingType];
            }

            // read file into response
            var fileContent = StaticFilesReader(path);
            context.Context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);

            // call serving files callback
            if (!InvokeCallbacks(OnServingFile, context))
                return;

            // close response
            Utils.TryCloseResponse(context.Context);
        }

        /// <summary>
        /// Serve an HTML file as a webpage.
        /// Note: this works using the static files mechanism, eg you must set 'StaticFilesPath' path for it to work.
        /// </summary>
        /// <param name="context">Context containing the request to serve file for.</param>
        /// <param name="path">Path of file to serve, under 'StaticFilesPath'.</param>
        public void ServeHtmlPage(ServeritoContext context, string path)
        {
            ServeStaticFile(context, path, true);
        }

        /// <summary>
        /// Handle an exception while handling a request.
        /// </summary>
        /// <param name="context">Request context, or null if the error happened while trying to get context.</param>
        /// <param name="exp">Exception object.</param>
        private void HandleException(ServeritoContext context, Exception exp)
        {
            // set status code to error by default.
            // note: the try-catch is in case the context was disposed.
            try
            {
                context.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            } catch { }

            // call the errors handler callback
            try
            {
                OnException?.Invoke(context, exp);
            }
            catch (BreakCallbacks) { }

            // if need to close requests, try to close it
            if (CloseRequests)
            {
                Utils.TryCloseResponse(context.Context);
            }
        }

        /// <summary>
        /// Invoke event handlers.
        /// </summary>
        /// <param name="callbacks">Callbacks to invoke.</param>
        /// <param name="context">Http listener context.</param>
        /// <returns>True if we should continue handling this request, false if we need to stop handling it.</returns>
        private bool InvokeCallbacks(HandleRequestsCallback callbacks, ServeritoContext context)
        {
            // invoke callback
            try
            {
                callbacks?.Invoke(context);
            }
            // on break callbacks do nothing
            catch (BreakCallbacks)
            {
            }
            // on abort request call abort and return false.
            catch (AbortRequest)
            {
                context.Context.Response.Abort();
                return false;
            }
            // on stop processing just return false.
            catch (StopProcessingRequest)
            {
                return false;
            }

            // if we got here we return true to continue handling request.
            return true;
        }
    }
}
