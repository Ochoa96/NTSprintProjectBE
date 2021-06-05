using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiLoginJwt.Models;

namespace WebApiLoginJwt.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _conf;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._conf = configuration;
        }

        [Route("Create")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserInfo model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.user,
                    Email = model.email
                };
                var result = await _userManager.CreateAsync(user, model.password);
                if (result.Succeeded)
                    return BuildToken(model);
                else
                    return BadRequest(result.Errors);
            }
            else
                return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("Login")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Login([FromBody] UserInfo model)
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.user, model.password, false, false);
                if (result.Succeeded)
                    return BuildToken(model);
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login attempt.");
                    return BadRequest(ModelState);
                }
            }
            else
                return BadRequest(ModelState);
        }

        private IActionResult BuildToken(UserInfo model)
        {
            try
            {
                var Claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, model.user),
                    new Claim(JwtRegisteredClaimNames.Email, model.email),
                    new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["secret_key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddHours(1);
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "https://localhost:44376",
                    audience: "https://localhost:44376",
                    claims: Claims,
                    expires: expiration,
                    signingCredentials: creds
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}