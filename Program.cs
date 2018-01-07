using Serverito;
using System;

namespace ServeritoTest
{
    class Program
    {
        /// <summary>
        /// 'Main' to quickly test the lib by building it as console app.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            {
                // create serverito
                var host = "http://localhost:8000/";
                ServeritoListener server = new ServeritoListener(host);

                // hold everything we print to console
                System.Text.StringBuilder consoleString = new System.Text.StringBuilder();

                // function to dump line to console + store it in consoleString so we can retrieve it view the webpage.
                System.Action<string> PrintConsoleLine = (string line) =>
                {
                // special case: ignore requests to fetch console
                if (line.EndsWith("/console/"))
                        return;

                // add to console string and write to console
                consoleString.Append(line);
                    consoleString.Append('\n');
                    Console.WriteLine(line);
                };

                // add test print for all callbacks
                server.OnException += (ServeritoContext context, Exception exception) => { PrintConsoleLine("Got Exception! " + exception.ToString()); };
                server.OnFinishedProcessingView += (ServeritoContext context) => { PrintConsoleLine("Finished processing view: " + context.Context.Request.RawUrl); };
                server.OnFinishHandlingRequest += (ServeritoContext context) => { PrintConsoleLine("Finished handling request: " + context.Context.Request.RawUrl); };
                server.OnNewRawRequest += (ServeritoContext context) => { PrintConsoleLine("\nNEW RAW REQUEST: " + context.Context.Request.RawUrl); };
                server.OnPassingRequestToView += (ServeritoContext context) => { PrintConsoleLine("Before passing to view: " + context.Context.Request.RawUrl); };
                server.OnServingFile += (ServeritoContext context) => { PrintConsoleLine("Serving file: " + context.Context.Request.RawUrl); };
                server.OnUndefinedURL += (ServeritoContext context) => { PrintConsoleLine("Undefined URL: " + context.Context.Request.RawUrl); };
                server.OnMissingFile += (ServeritoContext context) => { PrintConsoleLine("Missing file: " + context.Context.Request.RawUrl); };
                server.OnUrlMatching += (ServeritoContext context) => { PrintConsoleLine("URL matching: " + context.Context.Request.RawUrl); };

                // add static files
                server.StaticFilesRootUrl = "/static/";
                server.StaticFilesPath = "../../test_static_files";

                // dump all errors to response
                Utils.DumpExceptionsToResponse(server);

                // force trailing slash
                Utils.ForceTrailingSlash(server);

                // add some test views

                // root (eg '/') serve a testing html file that shows interesting links
                server.AddView(new URL("/"), (ServeritoContext context) =>
                {
                    server.ServeHtmlPage(context, "index.html");
                });

                // /hello/ just write "hello world"
                server.AddView(new URL("/hello/"), (ServeritoContext context) =>
                {
                    Utils.WriteToResponse(context.Context, "Hello World!");
                });

                // /main/ serve a testing html file
                server.AddView(new URL("/index/"), (ServeritoContext context) =>
                {
                    server.ServeHtmlPage(context, "test.html");
                });

                // /exp/ will throw exception
                server.AddView(new URL("/exp/"), (ServeritoContext context) =>
                {
                    throw new Exception("This is a test.");
                });

                // /anynumber/(+d)/ just check regex urls
                server.AddView(new URL(@"/anynumber/\d+/", matchType: UrlMatchingType.RegEx), (ServeritoContext context) =>
                {
                    Utils.WriteToResponse(context.Context, "It Works!");
                });

                // /console/ return the content of the console string
                server.AddView(new URL("/console/"), (ServeritoContext context) =>
                {
                    Utils.WriteToResponse(context.Context, consoleString.ToString());
                });

                // open test page in default browser
                System.Diagnostics.Process.Start(host);

                // start listening
                server.Start();
            }
        }
    }
}
