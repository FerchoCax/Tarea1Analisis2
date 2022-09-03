using DataAccess;
using Dominio.Entities;
using Services.Services;
using Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IJtAuth jwtAuth;
        private readonly ModelContext _context;

        public LoginController(IJtAuth jwtAuthh, ModelContext context)
        {
            this.jwtAuth = jwtAuthh;
            _context = context;
        }

        //POST para crear token
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Autenticacion([FromBody] Login login)
        {
            AuthControl autenticacion = AuthControl.Instance;
            LoginReturn login2 = autenticacion.GetUser(login.username, login.password);
            if (!login2.username.Equals(null))
            {
                var token = jwtAuth.Autenticacion(login.username, login.password);
                if (token == null)
                { return Unauthorized(); }
                login2.token = token;

                return Ok(login2);
            }
            return Unauthorized();

        }
        
    }
}
