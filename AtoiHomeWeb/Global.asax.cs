using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

/// <summary>
/// WEB API 프로젝트의 경우 이 소스가 필요하지 않지만 ASP.net Web application 템플릿을
/// 사용해서 프로젝트를 생성한 후 WEBAPI를 적용하려면 이 파일이 필요하고 주석처리된 부분이
/// 추가되어야 한다. 이후 owin oauth를 추가하는 과정에서 이 파일의 주석처리된 소스를
/// Startup.cs에서 대체하게됐다. 하지만 이 파일은 프로그램의 starting point로 여전히
/// 필요하다. 이 파일을 프로젝트에서 제거하면 런타임 에러가 발생한다.
/// </summary>
namespace AtoiHomeWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // OneClick android app에서 AtoiHomeWeb으로 로그인 요청을 하기 위해
            // WebAPI를 추가함
            // Manually installed WebAPI 2.2 after making an MVC project.
            //GlobalConfiguration.Configure(WebApiConfig.Register); // NEW way
            /////////////////////////////////////////////////////////////////

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
