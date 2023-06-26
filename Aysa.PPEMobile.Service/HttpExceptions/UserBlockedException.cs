namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class UserBlockedException : HttpException
    {
        public UserBlockedException() 
            : base("")
        { }

        public UserBlockedException(string message) 
            : base(message)
        { }
    }
}
