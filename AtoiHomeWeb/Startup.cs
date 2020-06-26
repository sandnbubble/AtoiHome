using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.OAuth;
using AtoiHomeWeb.Providers;
using System.Web.Http;

// access_token을 발급하기 위해 Install-Package Microsoft.Owin.Security.OAuth -Version 2.1.0 을 설치하면
// 이 파일이 생성된다. 이 파일의 Configuration메서드가  Startup.Auth.cs의 ConfigureAuth를 대체하기 때문에
// 반드시 ConfigureAuth를 여기에서 호출해줘야한다.
[assembly: OwinStartupAttribute(typeof(AtoiHomeWeb.Startup))]
namespace AtoiHomeWeb
{
    public partial class Startup
    {
        /// <summary>
        /// access-token발급을 할 수 있는 oauth provider를 만들기 위해 추가된 소스
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            WebApiConfig.Register(config);

            // 아래의 한줄이 뭘 하는지 알 수가 없음. cors(Cross orgine resource sharing)라고 하는데 찾아봐야됨.
            // SimpleAuthorizationServerProvider 의 아래 소스와 관련있음
            // context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
            //Starup.Auth.cs의 메서드를 추가해야 기존 mvc5 시작절차가 정상적으로 됨.
            ConfigureAuth(app);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = System.TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}
