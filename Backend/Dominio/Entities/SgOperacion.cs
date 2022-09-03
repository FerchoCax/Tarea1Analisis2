using System;
using System.Collections.Generic;

#nullable disable

namespace Dominio.Entities
{
    public partial class SgOperacion
    {
        public SgOperacion()
        {
            SgEncBitacoras = new HashSet<SgEncBitacora>();
            SgErrores = new HashSet<SgError>();
        }

        public decimal IdOperacion { get; set; }
        public string NombreOperacion { get; set; }
        public string Descripcion { get; set; }
        public string Usuario { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public string UsuarioBdd { get; set; }
        public DateTime? FechaFinOperacion { get; set; }
        public long? TiempoOperacionBdd { get; set; }
        public string Finalizada { get; set; }

        public virtual ICollection<SgEncBitacora> SgEncBitacoras { get; set; }
        public virtual ICollection<SgError> SgErrores { get; set; }
    }
}
