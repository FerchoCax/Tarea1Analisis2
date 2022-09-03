using DataAccess;
using Dominio.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Services
{
    public class Reportes : IReportes
    {
        Dominio.Entities.Reporte reporte = new Dominio.Entities.Reporte();
        private IConfiguration _configuration;

        public Reportes(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> GetReport(string cuerpo)
        {
            try
            {
                JObject Json = JObject.Parse(cuerpo);
                string urlReporte = Json.SelectToken("url").Value<string>();
                string URL = _configuration.GetConnectionString("ReportingService") + urlReporte;

                if (Json.ContainsKey("parametros"))
                {
                    foreach (var dato in Json["parametros"].Value<JObject>())
                    {
                        URL = URL + "&" + dato.Key.ToString() + "=" + dato.Value.ToString();
                    }
                }

                URL = URL + "&rs:Command=render&rs:Format=pdf";

                using (HttpClientHandler  handler = new HttpClientHandler())
                {
                    CredentialCache credenciales = new CredentialCache();

                    credenciales.Add(new Uri(URL), "NTLM", new NetworkCredential(_configuration["Credentials:ReportingService:Usuario"], _configuration["Credentials:ReportingService:Clave"]));
                    handler.Credentials = credenciales;

                    using (HttpClient cliente = new HttpClient(handler))
                    {
                        byte[] documento = await cliente.GetByteArrayAsync(URL);
                        reporte.ReporteBase64 = Convert.ToBase64String(documento);

                        return new ObjectResult(reporte) { StatusCode = 200 };
                    }
                }
            }
            catch (Exception e)
            {
                return new ObjectResult("{"+e+"}") { StatusCode = 500 };
                
            }
        }
    }
}
