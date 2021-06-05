using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    }
}