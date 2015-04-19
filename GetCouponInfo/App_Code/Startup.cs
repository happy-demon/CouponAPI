using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GetCouponInfo.Startup))]
namespace GetCouponInfo
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
