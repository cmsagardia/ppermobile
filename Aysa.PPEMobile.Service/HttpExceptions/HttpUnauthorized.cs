using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpUnauthorized : HttpException
    {
		public HttpUnauthorized() : base("")
        {
		}

		public HttpUnauthorized(string message) : base(message)
		{
		}
    }
}
