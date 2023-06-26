using System;

namespace Aysa.PPEMobile.Model
{
    public class Semana
    {
        public Guid Id { get; set; }

        public DateTime Desde { get; set; }

        public DateTime Hasta { get; set; }
        
        public int Mes { get; set; }
    }
}