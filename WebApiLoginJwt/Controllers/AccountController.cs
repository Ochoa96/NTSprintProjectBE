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
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.user
                    };
                    var result = await _userManager.CreateAsync(user, model.password);
                    if (result.Succeeded)
                    {
                        var res = await _signInManager.PasswordSignInAsync(model.user, model.password, false, false);
                        if (res.Succeeded)
                            return BuildToken(model);
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid Login attempt.");
                            return NotFound(ModelState);
                        }
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
                else
                    return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserInfo model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.user, model.password, false, false);
                    if (result.Succeeded)
                        return BuildToken(model);
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Login attempt.");
                        return NotFound(ModelState);
                    }
                }
                else
                    return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IActionResult BuildToken(UserInfo model)
        {
            try
            {
                var Claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, model.user),
                    new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["secret_key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddHours(1);
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "http://localhost:13105",
                    audience: "http://localhost:13105",
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