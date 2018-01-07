#region File Description
//-----------------------------------------------------------------------------
// Implement a URL mapper object.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using System.Text.RegularExpressions;


namespace Serverito
{
    /// <summary>
    /// Different ways to match URLs.
    /// </summary>
    public enum UrlMatchingType
    {
        /// <summary>
        /// The request URL must match exactly the url pattern.
        /// </summary>
        Exact,

        /// <summary>
        /// The request URL should match the begining of the pattern.
        /// </summary>
        StartsWith,

        /// <summary>
        /// The request URL should match the ending of the pattern.
        /// </summary>
        EndsWith,

        /// <summary>
        /// The request URL would be matched against pattern as a regex.
        /// </summary>
        RegEx,
    }

    /// <summary>
    /// An object to test URL matching.
    /// We use these objects to map URLs to views.
    /// </summary>
    public class URL
    {
        // a string to match exactly or starting with.
        string _pattern;

        // how we try to match url to pattern.
        UrlMatchingType _matchingType;

        // regex to use when matching using regex
        Regex _regex;

        // if set, will only match requests with this HTTP method.
        string _methodFilter = null;

        /// <summary>
        /// Create the URL pattern.
        /// </summary>
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="method">Optional filter by HTTP method (see HttpMethods for options).</param>
        /// <param name="matchType">How to match pattern.</param>
        public URL(string pattern, string method = null, UrlMatchingType matchType = UrlMatchingType.Exact)
        {
            // set pattern and matching type
            _pattern = pattern;
            _matchingType = matchType;
            _methodFilter = method;

            // if its regex matching, create regex object
            if (_matchingType == UrlMatchingType.RegEx)
            {
                _regex = new Regex(_pattern);
            }
        }

        /// <summary>
        /// Create the URL pattern from regex.
        /// </summary>
        /// <param name="regex">Regex to match.</param>
        /// <param name="method">Optional filter by HTTP method (see HttpMethods for options).</param>
        public URL(Regex regex, string method = null)
        {
            _pattern = regex.ToString();
            _matchingType = UrlMatchingType.RegEx;
            _methodFilter = method;
            _regex = regex;
        }

        /// <summary>
        /// Check if a given URL match the pattern.
        /// </summary>
        /// <param name="url">URL to check.</param>
        /// <param name="httpMethod">HTTP method.</param>
        /// <returns>If url match.</returns>
        public bool IsMatch(string url, string httpMethod)
        {
            // first check method
            if (_methodFilter != null && _methodFilter != httpMethod)
                return false;

            // now check match based on matching type
            switch (_matchingType)
            {
                case UrlMatchingType.StartsWith:
                    return url.StartsWith(_pattern);

                case UrlMatchingType.EndsWith:
                    return url.EndsWith(_pattern);

                case UrlMatchingType.Exact:
                    return url == _pattern;

                case UrlMatchingType.RegEx:
                    return _regex.IsMatch(url);
            }

            // shouldn't get here
            return false;
        }
    }
}
