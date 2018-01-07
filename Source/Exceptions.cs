#region File Description
//-----------------------------------------------------------------------------
// Exceptions used with serverito.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System;


namespace Serverito
{
    /// <summary>
    /// Throw this exception from an event handler (for example from 'OnNewRawRequest' callback) to break the callbacks chain
    /// and not continue to next callback.
    /// </summary>
    public class BreakCallbacks : Exception
    {
        /// <summary>
        /// Break callbacks chain.
        /// </summary>
        public BreakCallbacks()
        {
        }
    }

    /// <summary>
    /// Throw this exception from an event handler (for example from 'OnNewRawRequest' callback) to break the callbacks chain and abort the request.
    /// This will call the respnse Abort() and stop handling this request immediately.
    /// </summary>
    public class AbortRequest : Exception
    {
        /// <summary>
        /// Abort and stop processing current request.
        /// </summary>
        public AbortRequest()
        {
        }
    }

    /// <summary>
    /// Throw this exception from an event handler (for example from 'OnNewRawRequest' callback) to break the callbacks chain and stop processing the request.
    /// This will not call Abort(), but will stop processing the request immediately.
    /// </summary>
    public class StopProcessingRequest : Exception
    {
        /// <summary>
        /// Stop processing current request.
        /// </summary>
        public StopProcessingRequest()
        {
        }
    }
}
