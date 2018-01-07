#region File Description
//-----------------------------------------------------------------------------
// An interface to handle incoming messages.
// We use this object with the Listener classes.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System.Net;

namespace Diggers.Server
{
    /// <summary>
    /// Handle incoming requests.
    /// We send this object to one of the server types to process requests and other events.
    /// </summary>
    public interface RequestsHandler
    {
        /// <summary>
        /// Handle incoming request.
        /// </summary>
        /// <param name="context">Http listener instance for this request.</param>
        void HandleRequest(HttpListenerContext context);

        /// <summary>
        /// Handle exceptions.
        /// </summary>
        /// <param name="context">Current context or null if the exception was in getting it.</param>
        /// <param name="exp">Exception we got.</param>
        void HandleException(HttpListenerContext context, System.Exception exp);
    }
}
