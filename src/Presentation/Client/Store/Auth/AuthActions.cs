namespace PathfinderCampaignManager.Presentation.Client.Store.Auth;

public abstract class AuthAction { }

// Login Actions
public class LoginAction : AuthAction
{
    public string EmailOrUsername { get; }
    public string Password { get; }

    public LoginAction(string emailOrUsername, string password)
    {
        EmailOrUsername = emailOrUsername;
        Password = password;
    }
}

public class LoginSuccessAction : AuthAction
{
    public string Token { get; }
    public UserInfo User { get; }

    public LoginSuccessAction(string token, UserInfo user)
    {
        Token = token;
        User = user;
    }
}

public class LoginFailureAction : AuthAction
{
    public string ErrorMessage { get; }

    public LoginFailureAction(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

// Discord Login Actions
public class DiscordLoginAction : AuthAction { }

public class DiscordCallbackAction : AuthAction
{
    public string Code { get; }

    public DiscordCallbackAction(string code)
    {
        Code = code;
    }
}

// Register Actions
public class RegisterAction : AuthAction
{
    public string Email { get; }
    public string Username { get; }
    public string Password { get; }
    public string DisplayName { get; }

    public RegisterAction(string email, string username, string password, string displayName)
    {
        Email = email;
        Username = username;
        Password = password;
        DisplayName = displayName;
    }
}

public class RegisterSuccessAction : AuthAction
{
    public string Token { get; }
    public UserInfo User { get; }

    public RegisterSuccessAction(string token, UserInfo user)
    {
        Token = token;
        User = user;
    }
}

public class RegisterFailureAction : AuthAction
{
    public string ErrorMessage { get; }

    public RegisterFailureAction(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

// General Auth Actions
public class LogoutAction : AuthAction { }

public class CheckAuthAction : AuthAction { }

public class SetLoadingAction : AuthAction
{
    public bool IsLoading { get; }

    public SetLoadingAction(bool isLoading)
    {
        IsLoading = isLoading;
    }
}