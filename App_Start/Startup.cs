using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Reporting_application.App_Start.Startup))]

namespace Reporting_application.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            ConfigureAuth(app);
        }
    }
}
