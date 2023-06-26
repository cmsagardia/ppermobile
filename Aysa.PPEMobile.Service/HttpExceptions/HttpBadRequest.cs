using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpBadRequest : HttpException
    {
		public HttpBadRequest() : base("")
		{
		}

		public HttpBadRequest(string message) : base(message)
		{
		}
    }
}
