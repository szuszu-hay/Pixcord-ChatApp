namespace ChatAPI.Exceptions
{
    public class AccountLockedOutException : Exception
    {
        public AccountLockedOutException() : base("Account is locked out. Please try again after 5 mins.")
        {
            
        }
    }
}