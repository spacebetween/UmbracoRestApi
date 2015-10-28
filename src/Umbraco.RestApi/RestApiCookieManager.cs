using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using umbraco;
using Umbraco.Core;
using Umbraco.Web;

namespace Umbraco.RestApi
{
    internal class RestApiCookieManager : ChunkingCookieManager, ICookieManager
    {
        /// <summary>
        /// Explicitly implement this so that we filter the request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string ICookieManager.GetRequestCookie(IOwinContext context, string key)
        {
            if (UmbracoContext.Current == null)
            {
                return null;
            }

            var umbPath = GlobalSettings.Path.EnsureStartsWith('/').EnsureStartsWith('/');
            var restApiPath = umbPath + "rest/v1/";
            var shouldAuth = context.Request.PathBase.StartsWithSegments(new PathString(restApiPath));

            return shouldAuth == false
                //Don't auth request, don't return a cookie
                ? null
                //Return the default implementation
                : base.GetRequestCookie(context, key);
        }

    }
}