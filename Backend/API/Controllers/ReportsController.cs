using Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private IReportes _report;

        public ReportsController(IReportes rep)
        {
            _report = rep;
        }
        [HttpPost]
        public async Task<IActionResult> GetReportrAsync()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string cuerpo = await reader.ReadToEndAsync();
                return  await _report.GetReport(cuerpo);
            }
        }
    }
}
