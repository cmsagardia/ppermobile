using System;
using System.Collections.Generic;


namespace Aysa.PPEMobile.Model
{
    public class Section
    {
		public string Id { get; set; }

		public string Nombre { get; set; }

        public int Nivel { get; set; }

        public bool Favorito { get; set; }

        public List<PersonGuard> ResponsablesGuardia { get; set; }

        public string TipoNivel { get; set; }

        public override string ToString()
        {
            return this.Nombre;
        }
    }
}
