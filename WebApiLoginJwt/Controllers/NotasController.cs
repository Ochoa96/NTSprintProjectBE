using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiLoginJwt.Models;

namespace WebApiLoginJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotasController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        public NotasController(UserManager<ApplicationUser> um, AppDbContext context)
        {
            this._userManager = um;
            this._context = context;
        }
        
        public async Task<ApplicationUser> GetUser()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);
            return user;            
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var user = await GetUser();
                var notas = _context.Notas.Where(nota => nota.UserId == user.Id).Select(nota => new { nota.NotaId, nota.nota });
                return Ok(notas);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Post(NotaModel N)
        {
            try
            {
                var user = await GetUser();
                Nota nota = new Nota
                {
                    nota = N.nota,
                    UserId = user.Id
                };
                _context.Add(nota);
                _context.SaveChanges();
                return Ok(nota);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var user = await GetUser();
                var toDelete = _context.Notas.Where(nota => nota.UserId == user.Id).ToArray();
                _context.Notas.RemoveRange(toDelete);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
