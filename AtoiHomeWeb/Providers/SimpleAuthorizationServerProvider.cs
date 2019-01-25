using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

/// <summary>
/// 원본 소스가 있는 블러그
/// http://bitoftech.net/2014/06/01/token-based-authentication-asp-net-web-api-2-owin-asp-net-identity/
/// </summary>
namespace AtoiHomeWeb.Providers
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext()
            : base("DefaultConnection")
        {
        }
    }


    /// <summary>
    /// AuthRepository는 oauth인증에서 사용자 검증을 위해  데이타베이스와 연결하는 저장소의 역할이다.
    /// MVC5 Individual로 프로젝트를 생성하면 유저인증에 필요한 테이블들이 자동으로 생성되는데
    /// 생성된 테이블을 아래와 같이 별도의 connection을 생성하여 사용할 수 있다. 물론 ApplicationDbContext가
    /// 이미 자동생성된 컨트롤러 소스에 정의되어 있지만 어차피 연결이 private으로 설정되어 있고 다른 클라스에서
    /// 참조하기가 애매하다.
    /// </summary>
    public class AuthRepository : IDisposable
    {
        private AuthContext _ctx;

        private UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            _ctx = new AuthContext();
            _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }

        /// <summary>
        /// RestAPIController에 정의된 SignUp 메서드에서 유저등록을 위해 사용한다.
        /// </summary>
        /// <param name="userModel"></param>
        /// {
        ///  "userName": "Taiseer",
        ///  "password": "SuperPass",
        ///  "confirmPassword": "SuperPass"
        /// }
        /// <returns></returns>
    public async Task<IdentityResult> RegisterUser(Models.WebAPISignUpModel userModel)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = userModel.Email,
                Email = userModel.Email
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);

            return result;
        }

        /// <summary>
        /// SimpleAuthorizationServerProvider에서 유저인증을 위해 사용한다.
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<IdentityUser> FindUser(string Email, string password)
        {
            IdentityUser user = await _userManager.FindAsync(Email, password);
            return user;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }
    }

    /// ConfigureOAuth에 설정된 종점(\token)에서 제공하는 custom oauth provider이다
    /// client에서 \token에 email, password 매개변수를 아래와 같은 json형태로 전송(post)전송하면
    /// 메서드가 호출되고 유저인증을 한후 access token을 발급한다.
    /// grant_type=password&username=tester@atoihome.site&password=1234567890
    /// 발급된 token은 oneclick앱에서 www.atoihome.site, atoihomeservice(uploadloadimage등) rest api를 
    /// 사용할때 html header의 Authorization key에 "bearer token value"로 설정하여 사용된다.
    /// atoihomeservice에서 이 토큰을 디코딩하여 유효성 검증을 한다고 고생 좀 했다.

    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>
        /// 뭐하는 놈인지 좀 봐야됨. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //무조건 인증성공했다고 처리하고 있음
            context.Validated();
        }

        /// <summary>
        /// 유저인증하고 access token을 발급한다.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            using (AuthRepository _repo = new AuthRepository())
            {
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
                }
            }

            // Startup.cs 소스의 ConfigureOAuth에 설정된 옵션 참고
            // atoihomeservice에서 sub, role에 저장된 문자열을 추출해서 인증처리함
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));
            context.Validated(identity);
        }
    }
}