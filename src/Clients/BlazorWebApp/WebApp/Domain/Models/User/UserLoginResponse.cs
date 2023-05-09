namespace WebApp.Domain.Models.User
{
    public class UserLoginResponse
    {

        public string UserName { get; private set; }
        public string UserToken { get; private set; }

        public UserLoginResponse(string userName, string userToken)
        {
            UserName = userName;
            UserToken = userToken;
        }
    }
}
