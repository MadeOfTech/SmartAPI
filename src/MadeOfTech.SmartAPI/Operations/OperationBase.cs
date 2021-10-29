using MadeOfTech.SmartAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    /// <summary>
    /// OperationBase is any operation that can be done on a collection or a 
    /// member (GET, PUT, POST, DELETE or PATCH).
    /// It exposes what's necesserary to identify a specific member using the
    /// given route and id.
    /// </summary>
    public abstract class OperationBase
    {
        /// <summary>
        /// This regex allow us to build a unique id for member url, even if it
        /// uses a composite key.
        /// A key is built like followinf this grammar
        /// (https://www.crockford.com/mckeeman.html style) :
        ///
        /// key
        ///		element
        ///		'(' elements ')'
        /// 
        /// elements
        ///		element ',' element
        ///		element ',' elements
        ///		
        /// element
        ///		string
        ///		value
        /// 
        /// string
        ///		'"' characters '"'
        ///
        ///	characters
        ///		""
        ///		character characters
        ///	
        /// character
        ///		- '"'
        ///		
        /// value
        ///		- '"' - ',' - ')'
        /// 
        /// Original regex :
        /// ^\((?&lt;key>"(""|[^"])*"|[^\),"]*)(,(?&lt;key>"(""|[^"])*"|[^\),"]*))+\)$|^(?&lt;key>"(""|[^"])*"|[^,"]*)$
        /// 
        /// tested with : http://regexstorm.net/tester
        /// converted with : http://derekslager.com/blog/posts/2007/09/a-better-dotnet-regular-expression-tester.ashx
        /// </summary>
        private const string _regexPattern = @"^\((?<key>""(""""|[^""])*""|[^\),""]*)(,(?<key>""(""""|[^""])*""|[^\),""]*))+\)$|^(?<key>""(""""|[^""])*""|[^,""]*)$";
        private Regex _regex = new Regex(_regexPattern, RegexOptions.ExplicitCapture);

        /// <summary>
        /// This method provide children classes with a way to get id of the
        /// specified member. The description of the id of a ressource can be
        /// found in the documentation of this package.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string[] getKeys(HttpContext context)
        {
            var id = context.Request.RouteValues["id"].ToString();
            id = System.Web.HttpUtility.UrlDecode(id);
            if (string.IsNullOrEmpty(id)) return null;

            var match = _regex.Match(id);
            var keys = new List<string>();
            foreach (var capture in match.Groups[1].Captures)
            {
                keys.Add(capture.ToString());
            }

            return keys.ToArray();
        }

        public abstract Task Invoke(HttpContext context, Collection collection, string injectAttributeName, object injectAttributeValue);
    }
}
