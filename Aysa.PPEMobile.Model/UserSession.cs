using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Model
{
	public class Permissions
	{
		private Permissions(string value) { Value = value; }

		public string Value { get; set; }

        public static Permissions VisualizarLibroAbordo { get { return new Permissions("Visualizar libro de abordo"); } }
        public static Permissions VisualizarEventos { get { return new Permissions("Visualizar eventos"); } }
		public static Permissions VisualizarEventosAutorizado { get { return new Permissions("Visualizar eventos autorizado"); } }
		public static Permissions ModificarEvento { get { return new Permissions("Modificar evento"); } }
		public static Permissions ModificarEventoAutorizado { get { return new Permissions("Modificar evento autorizado"); } }
		public static Permissions CrearEvento { get { return new Permissions("Crear evento"); } }
	}


	public class UserSession
    {

        public string UserName { get; set; }

        public string Id { get; set; }

        public string nomApel { get; set; }

        public string Access_token { get; set; }

        public string Refresh_token { get; set; }

        public DateTime? LastActivityRequest { get; set; }

        public DateTime Expires_in { get; set; }

		public List<ActionPermission> Actions { get; set; }

        public List<Section> ActiveSections { get; set; }

        public List<Section> SectionsByLevel { get; set; }

        public List<Roles> Roles { get; set; }

        public PersonGuard PersonInGuard { get; set; }

		private static readonly object SyncLock = new object();
		private static UserSession instance;

		private UserSession()
		{
		}

		public static UserSession Instance
		{
			get
			{
				lock (SyncLock)
				{
					if (instance == null)
					{
						instance = new UserSession();
					}

					return instance;
				}
			}
		}

        public void InitUserSession(LoginResponse loginResponse, string userName) 
        {
            this.Access_token = loginResponse.access_token;

            if(loginResponse.refresh_token != null){
                this.Refresh_token = loginResponse.refresh_token;
            }
            this.Expires_in = DateTime.Now.AddSeconds(loginResponse.expires_in);

            if(userName.Length > 0)
            {
				// Save username
				this.UserName = userName;
            }
        }


        public bool CheckIfUserHasPermission(Permissions permision)
        {

            foreach(ActionPermission action in Actions){

                if(permision.Value == action.Nombre)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckIfUserHasActiveSections()
        {

            if(ActiveSections == null)
            {
                return false;
            }

            if(ActiveSections.Count == 0)
            {
                return false;
            }

            return true;
        }

        public bool CheckIfUserIsGuardResponsableOfMainSection()
        {
            // Search if user is guard responsable of section 1 or 2

            if(PersonInGuard == null)
            {
                return false;
            }

            foreach(Section section in PersonInGuard.Sectores)
            {
                if(section.Nivel == 1)
                {
                    // The user is responsable of section with level 1 or 2
                    return true;
                }
            }


            return false;
        }
    }
}
