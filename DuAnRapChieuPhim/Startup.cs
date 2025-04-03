using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Owin;
using System.Web.Services.Description;


[assembly: OwinStartupAttribute(typeof(DuAnRapChieuPhim.Startup))]
namespace DuAnRapChieuPhim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();     

        }

      
    }
}
