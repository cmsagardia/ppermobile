using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpForbidden : HttpException
    {
		public HttpForbidden() : base("")
        {
		}

		public HttpForbidden(string message) : base(message)
		{
        }
    }
}
