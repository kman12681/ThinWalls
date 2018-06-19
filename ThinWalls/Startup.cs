using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ThinWalls.Startup))]
namespace ThinWalls
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
