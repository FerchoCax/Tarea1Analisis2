using Dominio.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AuthControl
    {
     
        public static AuthControl instance = null;

        private Dictionary<string, string> usuarios = new Dictionary<string, string>
        {
            {"fernando","fernando" },
            {"juan","juan" },
            {"admin","admin" }
        };
        public static AuthControl Instance
        {
            get
            {
                if (instance == null)
                    instance = new AuthControl();
                return instance;
            }
        }
        public bool ValidateUser(string usuario, string password)
        {
            if (usuarios.ContainsKey(usuario))
            {
                if(usuarios.GetValueOrDefault(usuario) == password)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        public LoginReturn GetUser(string usuario, string password)
        {
            if (ValidateUser(usuario, password))
            {

               

                return new LoginReturn {  username = usuario };
            }
            else
            {
                return new LoginReturn { username = "none" };
            }
        }
    }
}
