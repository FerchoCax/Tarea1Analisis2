using System;
using System.Collections.Generic;

#nullable disable

namespace Dominio.Entities
{
    public partial class SgEncBitacora
    {
        public SgEncBitacora()
        {
            SgDetalleBitacora = new HashSet<SgDetalleBitacora>();
        }

        public decimal IdEnc { get; set; }
        public string NombreTabla { get; set; }
        public string Operacion { get; set; }
        public string LlaveFila { get; set; }
        public string CamposPk { get; set; }
        public string Usuario { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public decimal? IdOperacion { get; set; }
        public string UsuarioBdd { get; set; }

        public virtual SgOperacion IdOperacionNavigation { get; set; }
        public virtual ICollection<SgDetalleBitacora> SgDetalleBitacora { get; set; }
    }
}
