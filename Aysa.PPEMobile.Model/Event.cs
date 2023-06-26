using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Model
{

    public class Event
    {

		public enum Status
		{
			Open = 1,
			Close = 2,
		}

		public string Id { get; set; }

		public string Titulo { get; set; }

        public DateTime Fecha { get; set; }

        public string Lugar { get; set; }

        public User Usuario { get; set; }

        public int Estado { get; set; }

        public int NroEvento { get; set; }

        public EventType Tipo { get; set; }

        public string Detalle { get; set; }

        //public string Tag { get; set; }

        public List<Observation> Observaciones { get; set; }

        public Section Sector { get; set; }

        public Section SectorOrigen { get; set; }

        public int Referencia { get; set; }

        public Boolean Confidencial { get; set; }

        public List<AttachmentFile> Archivos { get; set; }

        public Boolean CanEdit { get; set; }
        public int? MayorNivelInterviniente { get; set; }
        public string MayorTipoNivelInterviniente { get; set; }

    }
}
