using System;
namespace Aysa.PPEMobile.Model
{
    public class EventType
    {
		public string Id { get; set; }

		public string Nombre { get; set; }

        public override string ToString()
        {
            return this.Nombre;
        }

    }
}
