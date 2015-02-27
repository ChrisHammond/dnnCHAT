using Christoc.Modules.DnnChat.Components;
using Microsoft.Owin;
using Owin;



//[assembly: OwinStartup(typeof(Christoc.Modules.DnnChat.Components.Startup))]
[assembly: OwinStartup(typeof(Startup))]

namespace Christoc.Modules.DnnChat.Components
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            // something

            var i = 1;
        } 
    }
}