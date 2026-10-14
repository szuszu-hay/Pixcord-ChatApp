namespace ChatAPI.Exceptions
{
    public class EmailNotConfirmedException : Exception
    {
        public EmailNotConfirmedException() : base("Please confirm your email."){
        }
    }
}