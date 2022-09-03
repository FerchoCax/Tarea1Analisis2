using System;
using System.Collections.Generic;

#nullable disable

namespace Dominio.Entities
{
    public partial class SgDetalleBitacora
    {
        public decimal IdDetalle { get; set; }
        public decimal IdEnc { get; set; }
        public string NombreCampo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }

        public virtual SgEncBitacora IdEncNavigation { get; set; }
    }
}
