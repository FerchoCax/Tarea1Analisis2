using Dominio.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Interfaces
{
    public interface IReportes
    {
        public Task<IActionResult> GetReport(string cuerpo);
    }
}
