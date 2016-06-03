using Microsoft.Owin.Extensions;
using Owin;


namespace SkimiaOS.Server.APIServer.OWIN
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(opt => opt.Bootstrapper = new APIServerBootstrapper());
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}
