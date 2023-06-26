using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpException : Exception
    {
		public HttpException() : base("")
        {
		}

        public HttpException(string message) : base(message)
        {
        }
    }
}
