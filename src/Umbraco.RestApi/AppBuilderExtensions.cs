using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Logging;
using Owin;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.RestApi.Controllers;
using Umbraco.Web;
using Umbraco.Web.Security.Identity;

namespace Umbraco.RestApi
{
    //public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
    //{
    //    public override void Configuration(IAppBuilder app)
    //    {
    //        //ensure the default options are configured
    //        base.Configuration(app);

    //        app.ConfigureUmbracoRestApi(new UmbracoRestApiOptions()
    //        {
    //            //Modify the CorsPolicy as required
    //            CorsPolicy = new CorsPolicy()
    //            {
    //                AllowAnyHeader = true,
    //                AllowAnyMethod = true,
    //                AllowAnyOrigin = true
    //            }
    //        });

    //        app.UseUmbracoCookieAuthenticationForRestApi(ApplicationContext.Current);

    //    }
    //}

    public static class AppBuilderExtensions
    {
        public static void ConfigureUmbracoRestApi(this IAppBuilder app, UmbracoRestApiOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            UmbracoRestApiOptionsInstance.Options = options;
        }

        public static IAppBuilder UseUmbracoCookieAuthenticationForRestApi(this IAppBuilder app, ApplicationContext appContext)
        {
            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return app;

            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                UmbracoConfig.For.UmbracoSettings().Security,
                GlobalSettings.TimeOutInMinutes,
                GlobalSettings.UseSSL)
            {
                Provider = new BackOfficeCookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user 
                    // logs in. This is a security feature which is used when you 
                    // change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                            TimeSpan.FromMinutes(30),
                            (manager, user) => user.GenerateUserIdentityAsync(manager),
                            identity => identity.GetUserId<int>()),
                }
            };

            //This is what will ensure that the rest api calls are auth'd
            authOptions.CookieManager = new RestApiCookieManager();

            app.UseCookieAuthentication(authOptions);

            return app;
        }
    }
}
