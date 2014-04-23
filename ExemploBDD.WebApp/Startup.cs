using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExemploBDD.WebApp.Startup))]
namespace ExemploBDD.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
