#region File Description
//-----------------------------------------------------------------------------
// Some utilities and helper functions.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Net;
using System.Collections.Generic;


namespace Serverito
{
    /// <summary>
    /// Different encoding types.
    /// </summary>
    public enum EncodingType
    {
        /// <summary>
        /// utf-8 encoding.
        /// </summary>
        UTF8,

        /// <summary>
        /// utf-32 encoding.
        /// </summary>
        UTF32,

        /// <summary>
        /// Unicode encoding.
        /// </summary>
        Unicode,

        /// <summary>
        /// Default system encoding.
        /// </summary>
        Default,
    }

    /// <summary>
    /// Static class with a collection of misc utils.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// A dictionary that convert known file extensions to their corresponding mime content-type.
        /// </summary>
        static Dictionary<string, string> _extensionToContentType = new Dictionary<string, string>();

        /// <summary>
        /// Convert encoding enum to handling class.
        /// </summary>
        static readonly System.Text.Encoding[] _encoding = new System.Text.Encoding[]
        {
            System.Text.Encoding.UTF8,
            System.Text.Encoding.UTF32,
            System.Text.Encoding.Unicode,
            System.Text.Encoding.Default,
        };

        /// <summary>
        /// Static constructor to setup few things.
        /// </summary>
        static Utils()
        {
            // set dictionary of file extensions to mime types.
            // based on: https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Complete_list_of_MIME_types
            _extensionToContentType["aac"] = "audio/aac";
            _extensionToContentType["abw"] = "application/x-abiword";
            _extensionToContentType["arc"] = "application/octet-stream";
            _extensionToContentType["avi"] = "video/x-msvideo";
            _extensionToContentType["azw"] = "application/vnd.amazon.ebook";
            _extensionToContentType["bin"] = "application/octet-stream";
            _extensionToContentType["bz"] = "application/x-bzip";
            _extensionToContentType["bz2"] = "application/x-bzip2";
            _extensionToContentType["csh"] = "application/x-csh";
            _extensionToContentType["css"] = "text/css";
            _extensionToContentType["csv"] = "text/csv";
            _extensionToContentType["doc"] = "application/msword";
            _extensionToContentType["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            _extensionToContentType["eot"] = "application/vnd.ms-fontobject";
            _extensionToContentType["epub"] = "application/epub+zip";
            _extensionToContentType["gif"] = "image/gif";
            _extensionToContentType["htm"] = ".htm";
            _extensionToContentType["html"] = "text/html";
            _extensionToContentType["ico"] = "image/x-icon";
            _extensionToContentType["ics"] = "text/calendar";
            _extensionToContentType["jar"] = "application/java-archive";
            _extensionToContentType["jpeg"] = ".jpeg";
            _extensionToContentType["jpg"] = "image/jpeg";
            _extensionToContentType["js"] = "application/javascript";
            _extensionToContentType["json"] = "application/json";
            _extensionToContentType["mid"] = ".mid";
            _extensionToContentType["midi"] = "audio/midi";
            _extensionToContentType["mpeg"] = "video/mpeg";
            _extensionToContentType["mpkg"] = "application/vnd.apple.installer+xml";
            _extensionToContentType["odp"] = "application/vnd.oasis.opendocument.presentation";
            _extensionToContentType["ods"] = "application/vnd.oasis.opendocument.spreadsheet";
            _extensionToContentType["odt"] = "application/vnd.oasis.opendocument.text";
            _extensionToContentType["oga"] = "audio/ogg";
            _extensionToContentType["ogv"] = "video/ogg";
            _extensionToContentType["ogx"] = "application/ogg";
            _extensionToContentType["otf"] = "font/otf";
            _extensionToContentType["png"] = "image/png";
            _extensionToContentType["pdf"] = "application/pdf";
            _extensionToContentType["ppt"] = "application/vnd.ms-powerpoint";
            _extensionToContentType["pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            _extensionToContentType["rar"] = "application/x-rar-compressed";
            _extensionToContentType["rtf"] = "application/rtf";
            _extensionToContentType["sh"] = "application/x-sh";
            _extensionToContentType["svg"] = "image/svg+xml";
            _extensionToContentType["swf"] = "application/x-shockwave-flash";
            _extensionToContentType["tar"] = "application/x-tar";
            _extensionToContentType["tif"] = ".tif";
            _extensionToContentType["tiff"] = "image/tiff";
            _extensionToContentType["ts"] = "application/typescript";
            _extensionToContentType["ttf"] = "font/ttf";
            _extensionToContentType["vsd"] = "application/vnd.visio";
            _extensionToContentType["wav"] = "audio/x-wav";
            _extensionToContentType["weba"] = "audio/webm";
            _extensionToContentType["webm"] = "video/webm";
            _extensionToContentType["webp"] = "image/webp";
            _extensionToContentType["woff"] = "font/woff";
            _extensionToContentType["woff2"] = "font/woff2";
            _extensionToContentType["xhtml"] = "application/xhtml+xml";
            _extensionToContentType["xls"] = "application/vnd.ms-excel";
            _extensionToContentType["xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            _extensionToContentType["xml"] = "application/xml";
            _extensionToContentType["xul"] = "application/vnd.mozilla.xul+xml";
            _extensionToContentType["zip"] = "application/zip";
            _extensionToContentType["3gp"] = "video/3gpp";
            _extensionToContentType["udio/3gpp"] = "video";
            _extensionToContentType["3g2"] = "video/3gpp2";
            _extensionToContentType["udio/3gpp2"] = "video";
            _extensionToContentType["7z"] = "application/x-7z-compressed";

            // some extras:
            _extensionToContentType["txt"] = "text/plain";
            _extensionToContentType["text"] = "text/plain";
            _extensionToContentType["md"] = "text/plain";
        }

        /// <summary>
        /// Convert a file to bytes array.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>Bytes array of the file content.</returns>
        public static byte[] FileToBytes(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }

        /// <summary>
        /// Get ip address (as string) from context.
        /// </summary>
        /// <param name="context">Context to get IP from.</param>
        /// <returns>IP as string.</returns>
        public static string GetIp(HttpListenerContext context)
        {
            return context.Request.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// Try to close a context response, ignore if fail.
        /// </summary>
        /// <param name="context">Context response to close.</param>
        public static void TryCloseResponse(HttpListenerContext context)
        {
            try
            {
                context.Response.Close();
            }
            catch { }
        }

        /// <summary>
        /// Convert file extension to mime type (content-type string).
        /// </summary>
        /// <param name="extension">File extension, without the dot (eg "doc", "mov", "mp3"...)</param>
        /// <returns>Content type string for file type, or null if not found.</returns>
        public static string ExtensionToMimeType(string extension)
        {
            // if we got extension starting with a dot, remove the dot.
            if (extension[0] == '.')
                extension = extension.Substring(1);

            // try to get mime type and return it
            string mimeType;
            if (_extensionToContentType.TryGetValue(extension.ToLower(), out mimeType))
                return mimeType;

            // unknown extension type? return null
            return null;
        }

        /// <summary>
        /// Read input from request as string.
        /// </summary>
        /// <param name="context">Context to read request input from.</param>
        /// <param name="encoding">Encoding to use.</param>
        /// <returns>Request input as string.</returns>
        public static string ReadRequestInput(HttpListenerContext context, EncodingType encoding = EncodingType.Default)
        {
            if (encoding == EncodingType.Default)
            {
                return new System.IO.StreamReader(context.Request.InputStream, true).ReadToEnd();
            }
            else
            {
                return new System.IO.StreamReader(context.Request.InputStream, _encoding[(int)encoding]).ReadToEnd();
            }
        }

        /// <summary>
        /// Convert string to bytes array.
        /// </summary>
        /// <param name="data">String to convert.</param>
        /// <param name="encoding">Encoding to use.</param>
        /// <returns>String bytes array.</returns>
        public static byte[] StringToBytes(string data, EncodingType encoding = EncodingType.UTF8)
        {
            return _encoding[(int)encoding].GetBytes(data);
        }

        /// <summary>
        /// Force a trailing slash by redirecting if trailing slash is absent.
        /// To use this function set it as a callback for OnUrlMatching, eg:
        /// server.OnUrlMatching += Utils._ForceTrailingSlashHandler;
        /// </summary>
        private static void _ForceTrailingSlashHandler(ServeritoContext context)
        {
            if (!context.Context.Request.RawUrl.ToString().EndsWith("/"))
            {
                context.Context.Response.Redirect(context.Context.Request.RawUrl + "/");
                TryCloseResponse(context.Context);
                throw new StopProcessingRequest();
            }
        }

        /// <summary>
        /// Make a server listener force all URLs to end with trailing slashes.
        /// Whenever a user enter a URL without a training slash, it will redirect to the URL with slash.
        /// Note: uses the events mechanism internally, clearing event callbacks might undo this call.
        /// </summary>
        /// <param name="listener">Listener to set.</param>
        public static void ForceTrailingSlash(ServeritoListener listener)
        {
            listener.OnUrlMatching += _ForceTrailingSlashHandler;
        }

        /// <summary>
        /// Dump all exceptions to response.
        /// To use this function set it as a callback for OnException, eg:
        /// server.OnException += Utils._DumpExceptionsToResponseHandler;
        /// </summary>
        private static void _DumpExceptionsToResponseHandler(ServeritoContext context, Exception exc)
        {
            WriteToResponse(context.Context, exc.ToString());
        }

        /// <summary>
        /// Make a server listener dump all exceptions directly to response.
        /// Use for debugging only! Very unsafe for production..
        /// Note: uses the events mechanism internally, clearing event callbacks might undo this call.
        /// </summary>
        /// <param name="listener">Listener to set.</param>
        public static void DumpExceptionsToResponse(ServeritoListener listener)
        {
            listener.OnException += _DumpExceptionsToResponseHandler;
        }

        /// <summary>
        /// Write string to response output stream.
        /// Note: will not close response.
        /// </summary>
        /// <param name="context">Context to write string into response.</param>
        /// <param name="data">Data to write.</param>
        public static void WriteToResponse(HttpListenerContext context, string data)
        {
            var bytes = StringToBytes(data);
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
    }
}
