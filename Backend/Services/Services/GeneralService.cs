using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Services.Interfaces;

namespace Services.Services
{
    public class GeneralService: IGeneralService
    {
        private IConfiguration _configuration;

        public GeneralService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateTime FechaHora()
        {
            return DateTime.Now;
        }
    }
}
