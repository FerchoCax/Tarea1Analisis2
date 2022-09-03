using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using Services.Services;
using Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Services.Services
{
    public class Auth: IJtAuth
    {
        private string key = "";
        

        public Auth(string key)
        {
            this.key = key;
            
        }
        public string Autenticacion(string username, string password)
        {
            AuthControl autenticacion = AuthControl.Instance;
            //MemebershipModel member = autenticacion.ValidateUser(login.user, login.pass);
            if (!autenticacion.ValidateUser(username, password))
            {
                return null;
            }
            else
            {
                var TokenHandler = new JwtSecurityTokenHandler();
                var TokenKey = Encoding.ASCII.GetBytes(key);
                var TokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                        new Claim(ClaimTypes.Name,username)
                        }),
                    Expires = DateTime.UtcNow.AddHours(12),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(TokenKey), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = TokenHandler.CreateToken(TokenDescriptor);
                return (TokenHandler.WriteToken(token));
            }
        }
        
    }
}
