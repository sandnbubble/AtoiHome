using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Web.Http;

namespace AtoiHomeWeb
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // TODO: Add any additional configuration code.

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // WebAPI when dealing with JSON & JavaScript!
            // Setup json serialization to serialize classes to camel (std. Json format)
            //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //Formatter.SerializerSettings.ContractResolver =
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}