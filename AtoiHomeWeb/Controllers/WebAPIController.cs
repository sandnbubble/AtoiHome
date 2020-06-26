using AtoiHomeWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AtoiHomeWeb.Controllers
{
    [RoutePrefix("api/RestAccount")]
    public class RestAccountController : ApiController
    {
        private Providers.AuthRepository _repo = null;

        public RestAccountController()
        {
            _repo = new Providers.AuthRepository();
        }

        /// <summary>
        /// POST api/Account/Register 
        /// </summary>
        /// <param name="userModel"></param>
        /// (grant_type=password&username=”Taiseer”&password=”SuperPass”
        /// <returns></returns>
        [AllowAnonymous]
        [Route("SignUp")]
        public async Task<IHttpActionResult> SignUp(Models.WebAPISignUpModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        /// <summary>
        /// GET api/Account/Get
        /// GET 요청 테스트용 메서드
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("Get")]
        public IHttpActionResult Get()
        {
            return Ok("hello");
        }

        /// <summary>
        /// GET api/Account/Get
        /// GET 요청 테스트용 메서드
        /// </summary>
        /// <returns></returns>
        
        [Authorize]
        [HttpGet]
        [Route("ValidateToken")]
        public IHttpActionResult ValidateToken()
        {
            return Ok("true");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }
            return null;
        }
    }
}