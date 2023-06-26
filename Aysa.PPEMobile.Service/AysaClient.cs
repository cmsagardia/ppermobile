using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;
using System;
using System.Linq;

using Newtonsoft.Json;

namespace Aysa.PPEMobile.Service
{
    public class AysaClient
    {
        //AySA TEST
        public static string URL = "https://test-api-ppe.aysa.com.ar/api/";

        //AySA PROD
        //public static string URL = "https://prod-api-ppe.aysa.com.ar/api/";

        //Dirmod Dev
        //public static string URL = "https://dev.api.ppe.aysa.dirmod.com/api/";

        #region Old URLs
        //public static string URL = "http://10.10.20.71:8080/api/";
        //public static string URL = "http://aysappe:8080/api/";
        //public static string URL = "https://prod-api-ppe.aysa.com.ar/api/";
        //PROD
        //public static string URL = "http://10.10.20.71:8080/api/";
        //public static string URL = "https://prod-api-ppe.aysa.com.ar/api/";
        //public static string URL = "http://10.10.20.71:8080/api/";
        #endregion

        private static readonly object SyncLock = new object();
        private static AysaClient instance;

        // Define general format for Dates
        public static readonly string FormatDateToServer = "yyyy/MM/dd";

        private AysaClient()
        {
        }

        public static AysaClient Instance
        {
            get
            {
                lock (SyncLock)
                {
                    if (instance == null)
                    {
                        instance = new AysaClient();
                    }

                    return instance;
                }
            }
        }

        #region LOGIN

		private static string LOGIN_URL = "Auth/Login";

		public async Task<LoginResponse> Login(string username, string password)
		{

			Dictionary<string, string> values = new Dictionary<string, string>
		    {
			    { "grant_type", "password" },
                { "userName", username },
                { "password", password }
		    };

            LoginResponse loginResponse = await AysaClientServices.Instance.Post<LoginResponse>(LOGIN_URL, values);

			// Init user session, this session will be using throughtout the app lifecycle by differents WS
			UserSession.Instance.InitUserSession(loginResponse, username);
            UserSession.Instance.LastActivityRequest = DateTime.Now;
			// Get permissions for user logged
			await GetActionsForUser();

			return loginResponse;
		}

        private static string REFRESH_TOKEN_URL = "Auth/token?grant_type=refresh_token&refresh_token={0}";


        public async Task RefreshTokenIfNeeded()
        {

            

            if (UserSession.Instance.Expires_in < DateTime.Now)
            {
                TimeSpan? difference = DateTime.Now - UserSession.Instance.LastActivityRequest;
                //TODO Verificar el tiempo de inactividad de la sesión
                if (difference.Value.TotalMinutes >= 30 && difference.HasValue)
                {
                    throw new HttpExceptions.HttpUnauthorized();
                }

                LoginResponse loginResponse = await AysaClientServices.Instance.Post<LoginResponse>(string.Format(REFRESH_TOKEN_URL, UserSession.Instance.Refresh_token));

                // Save user session, this session will be using throughtout the app lifecycle by differents WS
                UserSession.Instance.InitUserSession(loginResponse, "");
            }

            UserSession.Instance.LastActivityRequest = DateTime.Now;
        }

        // Get user permissions 
        private static string GET_ACTIONS_BY_USER_URL = "Acciones/GetAccionesByUser";

        public async Task GetActionsForUser()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Get actions
            List<ActionPermission> actions = await AysaClientServices.Instance.Get<List<ActionPermission>>(GET_ACTIONS_BY_USER_URL);

            // Save user permissions
            UserSession.Instance.Actions = actions;
        }

        // Get user info 
        private static string GET_USER_PROFILE_URL = "Usuarios/GetUserInformation";

        public async Task<User> GetUserInfo()
		{
			// Refresh token if it needed
			await RefreshTokenIfNeeded();

			// Get user info
            User user = await AysaClientServices.Instance.Get<User>(GET_USER_PROFILE_URL);

            return user;
		}

        #endregion

        #region SECTORES

        private static string GET_SECTIONS_BY_LEVEL_URL = "Sectores/GetSectorsWithPublishGuard";

        public async Task<List<Level>> GetLevels()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Get sections from server
            List<Section> sectionsByLevels = await AysaClientServices.Instance.Get<List<Section>>(GET_SECTIONS_BY_LEVEL_URL);


            List<int> levels = sectionsByLevels.Select(section => section.Nivel).Distinct().ToList();

            levels.Sort();

            // Gorup sections in thier respective levels
            List<Level> levelsWithSections = new List<Level>();

			//Favoritos
			Level levelFavorites = new Level { Nombre = "Favoritos", Sectores = new List<Section>() };

			foreach (Section section in sectionsByLevels)
			{
				if (section.Favorito) levelFavorites.Sectores.Add(section);
			}

			levelsWithSections.Add(levelFavorites);


			foreach (int level in levels)
            {
                // Crate Level object and assign its level property
                Level levelObj = new Level();

                if (level == 99)
                {
                    levelObj.Nombre = "Nivel Apoyo";
                }
                else if (level == 98)
                {
                    levelObj.Nombre = "Nivel Operario";
                }
                else
                {
                    levelObj.Nombre = "Nivel " + level.ToString();
                }

                levelObj.Nivel = level;
                // Find and set the sections in level
                levelObj.SetSectionsBySectionList(sectionsByLevels);

                levelsWithSections.Add(levelObj);

            }



            //var indexOperario = levelsWithSections.FindIndex(x=> x.Nombre == "Nivel Operario");
            //var indexApoyo = levelsWithSections.FindIndex(x => x.Nombre == "Nivel Apoyo");

            //Level lvlTemp = levelsWithSections[indexOperario];
            //levelsWithSections[indexOperario] = levelsWithSections[indexApoyo];
            //levelsWithSections[indexApoyo] = lvlTemp;

            return levelsWithSections;
        }

        private static string ADD_OR_DELETE_FAVORITE_URL = "Sectores/AddOrDeleteFavorite";

        public async Task<Section> AddOrDeleteFavorite(string id, bool currentState)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "SectorId", id },
                { "isFavorite", (!currentState).ToString()}
            };

            Section changeFavorite = await AysaClientServices.Instance.Post<Section>(ADD_OR_DELETE_FAVORITE_URL, values);

            return changeFavorite;
        }

        private static string SEARCH_SECTIONS__BY_NAME_URL = "Sectores/GetSectorByNameWithPublishGuard?nombre={0}";

        public async Task<List<Section>> SearchSectionByName(string searchText)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Get sections from server
            return await AysaClientServices.Instance.Get<List<Section>>(string.Format(SEARCH_SECTIONS__BY_NAME_URL, searchText));
        }

        private static string GET_DATE_BY_SECTION_URL = "Semanas/GetWeekBySector?sectorId={0}";

        public async Task<SemanaSeccion> SearchDateBySection(string sectionId)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Get sections from server
            return await AysaClientServices.Instance.Get<SemanaSeccion>(string.Format(GET_DATE_BY_SECTION_URL, sectionId));
        }


        // Get people responsable of guard by ID of Sectionc
        private static string GET_RESPONSABLES_GUARD_BY_SECTOR_URL = "Guardias/GetResponsableGuardiaActualBySector?sectorId={0}";

        public async Task<List<PersonGuard>> GetResponsablesGuardBySector(string id)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Get people in guard from server
            List<PersonGuard> peopleInGuard = await AysaClientServices.Instance.Get<List<PersonGuard>>(string.Format(GET_RESPONSABLES_GUARD_BY_SECTOR_URL, id));

            // Build list of Contact Types form properties of personGuard, this list it will be using to show a tableView with contact information
            foreach (PersonGuard personGuard in peopleInGuard)
            {
                personGuard.BuildContactTypes();
            }


            return peopleInGuard;
        }

        private static string LIST_SECTIONS_ACTIVE_URL = "Guardias/GetSectoresGuardiaActiva";

        public async Task<List<Section>> GetActiveSections()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<Section>>(LIST_SECTIONS_ACTIVE_URL);
        }

        private static string LIST_SECTIONS_BY_LEVEL_URL = "Sectores/FilterByUserLevel";

        public async Task<List<Section>> GetSectionsByUserLevel()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<Section>>(LIST_SECTIONS_BY_LEVEL_URL);
        }

        private static string GET_GUARD_RESPONSABLE_URL = "ResponsablesGuardia/GetByUserName?userName={0}";

        public async Task<PersonGuard> GetGuardResponsableByName(string username)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<PersonGuard>(string.Format(GET_GUARD_RESPONSABLE_URL, username));
        }


        #endregion



        #region Semana

        private static string SEMANA_GUARDIA_URL = "Semanas/GetActiveWeek";
        public async Task<Semana> GetActiveWeek()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<Semana>(SEMANA_GUARDIA_URL);
        }
        #endregion

        #region Events

        private static string LIST_EVENTS_URL = "Eventos/GetAbiertos";
        private static string LIST_EVENTS_BY_USER_URL = "Eventos/GetAbiertosByUser";

        public async Task<List<Event>> GetOpenEvents()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Check if the user has permissions to watch the all events
            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarEventos))
            {
                // Get all events
                return await AysaClientServices.Instance.Get<List<Event>>(LIST_EVENTS_URL);
            }
            else
            {

                if (UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarEventosAutorizado))
                {
                    return await AysaClientServices.Instance.Get<List<Event>>(LIST_EVENTS_BY_USER_URL);
                }
            }


            // If user doesn't have permissions return empty list
            return new List<Event>();
        }

        //Novedades Get
        private static string LIST_FEATURES_URL = "novedades";

        public async Task<List<Feature>> GetFeatures()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            // Check if the user has permissions to watch the all events
            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.VisualizarEventosAutorizado))
            {
                // Get all features
                return await AysaClientServices.Instance.Get<List<Feature>>(LIST_FEATURES_URL);
            }


            // If user doesn't have permissions return empty list
            return new List<Feature>();
        }


        private static string GET_EVENT_URL = "Eventos/{0}";

        public async Task<Event> GetEventById(string id)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<Event>(string.Format(GET_EVENT_URL, id));
        }

        private static string GET_FEATURE_URL = "novedades/{0}";

        public async Task<Feature> GetFeatureById(string id)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<Feature>(string.Format(GET_FEATURE_URL, id));
        }

        private static string CREATE_EVENT_URL = "Eventos";

        public async Task<Event> CreateEvent(Event eventObj)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Post<Event>(CREATE_EVENT_URL, eventObj);
        }

        private static string CREATE_FEATURE_URL = "Novedades";

        public async Task<Feature> CreateFeature(Feature eventObj)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Post<Feature>(CREATE_FEATURE_URL, eventObj);
        }

        private static string UPDATE_EVENT_URL = "Eventos/{0}";

        public async Task<Event> UpdateEvent(string Id, Event eventObj)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Put<Event>(string.Format(UPDATE_EVENT_URL, Id), eventObj);
        }

        private static string UPDATE_FEATURE_URL = "novedades/{0}";

        public async Task<Feature> UpdateFeature(string Id, Feature eventObj)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Put<Feature>(string.Format(UPDATE_FEATURE_URL, Id), eventObj);
        }

        // Get events by filter
        private static string GET_EVENTS_BY_FILTER_URL = "Eventos/GetByFilters?nroEvento={0}&titulo={1}&estado={2}&desde={3}&hasta={4}";

        public async Task<List<Event>> GetEventsByFilter(FilterEventData filter)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();


            string nroEvento = filter.EventNumber != 0 ? filter.EventNumber.ToString() : "";
            string title = filter.Title;
            string status = filter.Status != -1 ? filter.Status.ToString() : "";
            string fromDate = filter.FromDate.ToString(FormatDateToServer);
            string toDate = filter.ToDate.ToString(FormatDateToServer);

            // Get people in guard from server
            return await AysaClientServices.Instance.Get<List<Event>>(string.Format(GET_EVENTS_BY_FILTER_URL, nroEvento, title, status, fromDate, toDate));
        }

        // Get features by filter
        private static string GET_FEATURES_BY_FILTER_URL = "novedades/GetByFilters?nombreApellido={0}&detail={1}&desde={2}&hasta={3}&sectorid={4}";

        public async Task<List<Feature>> GetFeaturesByFilter(FilterFeatureData filter)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();


            string userName = filter.Username != "" ? filter.Username : "";
            string detail = filter.Detail;
            string fromDate = filter.FromDate.ToString(FormatDateToServer);
            string toDate = filter.ToDate.ToString(FormatDateToServer);
            string sectorId = filter.Sector.Id != "000000000" ? filter.Sector.Id : "";

            string parameters = string.Format(GET_FEATURES_BY_FILTER_URL, userName, detail, fromDate, toDate, sectorId);
            // Get people in guard from server
            return await AysaClientServices.Instance.Get<List<Feature>>(parameters);
        }


        private static string LIST_EVENTS_TYPE_URL = "EventoTipos";

        public async Task<List<EventType>> GetEventsType()
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<EventType>>(LIST_EVENTS_TYPE_URL);
        }

        private static string CREATE_OBSERVATION_URL = "EventosObservacion";

        public async Task<Observation> CreateObservation(Observation observationObj)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Post<Observation>(CREATE_OBSERVATION_URL, observationObj);
        }

        private static string SET_CONFIDENTIAL_URL = "Eventos/SetConfidencial?eventoId={0}";

        public async Task<object> SetEventConfidential(string eventID)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Post<object>(string.Format(SET_CONFIDENTIAL_URL, eventID));
        }

        #endregion


        #region Files


        private static string GET_FILE_URL = "Archivos/GetFile?id={0}";

        public async Task<byte[]> GetFile(string fileId)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.GetFile(string.Format(GET_FILE_URL, fileId));
        }


        private static string GET_FILES_OF_EVENT_URL = "Eventos/GetEventoArchivos?Id={0}";

        public async Task<List<AttachmentFile>> GetFilesOfEvent(string eventId)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<AttachmentFile>>(string.Format(GET_FILES_OF_EVENT_URL, eventId));
        }


        private static string GET_FILES_OF_FEATURE_URL = "novedades/GetNovedadArchivos?Id={0}";
        public async Task<List<AttachmentFile>> GetFilesOfFeature(string eventId)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<AttachmentFile>>(string.Format(GET_FILES_OF_FEATURE_URL, eventId));
        }

        private static string UPLOAD_FILE_URL = "Archivos/UploadPublicFile";

        public async Task<AttachmentFile> UploadFile(byte[] file, string fileName)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.UploadFile<AttachmentFile>(UPLOAD_FILE_URL, file, fileName);
            //return await AysaClientServices.Instance.Upload<AttachmentFile>(file, fileName);
        }

        private static string UPLOAD_FILE_BASE64_URL = "Archivos/UploadFileBase64";

        public async Task<AttachmentFile> UploadFileBase64(byte[] file, string fileName, bool privateFile)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.UploadFileBase64<AttachmentFile>(UPLOAD_FILE_BASE64_URL, file, fileName, privateFile);
        }

        private static string UPLOAD_FILE_URL_FEATURE = "Archivos/UploadNovedadFile?privateFile=false";

        public async Task<AttachmentFile> UploadFileFeature(byte[] file, string fileName)
        {
            // Refresh token if it needed
            await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.UploadFile<AttachmentFile>(UPLOAD_FILE_URL_FEATURE, file, fileName);
            //return await AysaClientServices.Instance.Upload<AttachmentFile>(file, fileName);
        }

        #endregion

        #region Document

        private static string GET_DOCUMENTS_URL = "Documentos";

        public async Task<List<Document>> GetDocuments()
		{
			// Refresh token if it needed
			await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Get<List<Document>>(GET_DOCUMENTS_URL);
		}

        private static string GET_DOCUMENT_FILE_URL = "Documentos/GetFile?serverRelativeUrl={0}";

        public async Task<byte[]> GetDocumentFile(string serverRelativeUrl)
		{
			// Refresh token if it needed
			await RefreshTokenIfNeeded();

			return await AysaClientServices.Instance.GetFile(string.Format(GET_DOCUMENT_FILE_URL, serverRelativeUrl));
		}

		private static string DELETE_FILE_URL = "Archivos/{0}";

        public async Task<AttachmentFile>DeleteFile(string fileId)
		{
			// Refresh token if it needed
			await RefreshTokenIfNeeded();

            return await AysaClientServices.Instance.Delete<AttachmentFile>(string.Format(DELETE_FILE_URL, fileId));
		}

        #endregion
    }
}
