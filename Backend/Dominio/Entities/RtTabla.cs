using System;
using System.Collections.Generic;

#nullable disable

namespace Dominio.Entities
{
    public partial class RtTabla
    {
        public long Tid { get; set; }
        public string Tabla { get; set; }
        public string Descripcion { get; set; }
        public string Sinonimo { get; set; }
        public long Sistema { get; set; }
        public string MeClase { get; set; }
        public string MeOperacion { get; set; }
        public string MeEstado { get; set; }
        public string Reportes { get; set; }
        public string Bitacorizar { get; set; }
    }
}
