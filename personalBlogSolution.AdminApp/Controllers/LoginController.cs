using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using personalBlogSolution.AdminApp.Services.Users;
using personalBlogSolution.ViewModels.System.Users;

namespace personalBlogSolution.AdminApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IConfiguration _configuration;
        
        public LoginController(IUserApiClient userApiClient,
            IConfiguration configuration)
        {
            _userApiClient = userApiClient;
            _configuration = configuration;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View();

            var result = await _userApiClient.Authenticate(request);

            var userPrincipal = this.ValidateToken(result.ResultDataObject);
            var authProperties = new AuthenticationProperties()
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = false
            };
            
            //Set session để lưu giữ khi đăng nhập thành công, nhằm check xem user đã đăng nhập hay chưa, nếu chưa sẽ chỉ hiện trang đăng nhập
            HttpContext.Session.SetString("Token", result.ResultDataObject);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        //Helper Methods
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            var validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidAudience = _configuration["Tokens:Issuer"],
                ValidIssuer = _configuration["Tokens:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]))
            };
            
            var principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out _);

            return principal;
        }
    }
}