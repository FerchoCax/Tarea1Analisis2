using System;
using System.Collections.Generic;

#nullable disable

namespace Dominio.Entities
{
    public partial class SgError
    {
        public decimal IdOperacion { get; set; }
        public string Error { get; set; }
        public string Stacktrace { get; set; }

        public virtual SgOperacion IdOperacionNavigation { get; set; }
    }
}
