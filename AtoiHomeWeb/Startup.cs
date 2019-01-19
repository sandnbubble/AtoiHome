using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AtoiHomeWeb.Startup))]
namespace AtoiHomeWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
