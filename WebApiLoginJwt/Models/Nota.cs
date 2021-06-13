using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiLoginJwt.Models
{
    public class Nota
    {
        public int NotaId { get; set; }
        public string UserId { get; set; }
        public string nota { get; set; }
    }
}
