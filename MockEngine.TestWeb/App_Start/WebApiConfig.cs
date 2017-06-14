using MockEngine.Http;
using MockEngine.Http.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MockEngine.TestWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API formatters

            config.Formatters.Insert(0,new YamlMediaTypeFormatter());

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = "ready", controller = "MockTest" }
            );
            config.Routes.MapHttpRoute(
                name: "DefaultApi2",
                routeTemplate: "api/*",
                defaults: new { action = "ready", controller = "MockTest" }
            );

            config.MessageHandlers.Insert(0,new MockMessageHandler());
        }
    }
}
