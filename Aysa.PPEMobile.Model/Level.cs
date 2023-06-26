using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Model
{
    public class Level
    {
		public string Id { get; set; }

        public string Nombre { get; set; }

		public string FechaCarga { get; set; }

		public string FechaUltimaModificacion { get; set; }

        public string FechaPublicacion { get; set; }

        public int Nivel { get; set; }

        public int Estado { get; set; }

        public int Mes { get; set; }

        public int Anio { get; set; }

        public string Favorito { get; set; }

        public List<Section> Sectores { get; set; }


        // This function is using to group the sections that correspond to current level 
        public void SetSectionsBySectionList(List<Section> sections)
        {
            this.Sectores = new List<Section>();

            // Search sections with the same nivel and add to "Sectores" property
            foreach(Section section in sections)
            {
                if(this.Nivel == section.Nivel)
                {
                    this.Sectores.Add(section);
                }
            }
        }
		
    }
}
