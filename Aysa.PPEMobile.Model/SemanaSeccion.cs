using System;

namespace Aysa.PPEMobile.Model
{
	public class SemanaSeccion
	{
		public DateTime Desde { get; set; }

		public DateTime Hasta { get; set; }

		public int DiasActivos { get; set; }

		public bool GuardiaActiva { get; set; }
	}
}
