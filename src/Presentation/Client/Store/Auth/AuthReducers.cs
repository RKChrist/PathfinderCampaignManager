using Fluxor;

namespace PathfinderCampaignManager.Presentation.Client.Store.Auth;

public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState ReduceLoginAction(AuthState state, LoginAction action) =>
        new(state.IsAuthenticated, isLoading: true, state.Token, state.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceLoginSuccessAction(AuthState state, LoginSuccessAction action) =>
        new(isAuthenticated: true, isLoading: false, action.Token, action.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceLoginFailureAction(AuthState state, LoginFailureAction action) =>
        new(isAuthenticated: false, isLoading: false, token: null, user: null, action.ErrorMessage);

    [ReducerMethod]
    public static AuthState ReduceDiscordLoginAction(AuthState state, DiscordLoginAction action) =>
        new(state.IsAuthenticated, isLoading: true, state.Token, state.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceDiscordCallbackAction(AuthState state, DiscordCallbackAction action) =>
        new(state.IsAuthenticated, isLoading: true, state.Token, state.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceRegisterAction(AuthState state, RegisterAction action) =>
        new(state.IsAuthenticated, isLoading: true, state.Token, state.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceRegisterSuccessAction(AuthState state, RegisterSuccessAction action) =>
        new(isAuthenticated: true, isLoading: false, action.Token, action.User, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceRegisterFailureAction(AuthState state, RegisterFailureAction action) =>
        new(isAuthenticated: false, isLoading: false, token: null, user: null, action.ErrorMessage);

    [ReducerMethod]
    public static AuthState ReduceLogoutAction(AuthState state, LogoutAction action) =>
        new(isAuthenticated: false, isLoading: false, token: null, user: null, errorMessage: null);

    [ReducerMethod]
    public static AuthState ReduceSetLoadingAction(AuthState state, SetLoadingAction action) =>
        new(state.IsAuthenticated, action.IsLoading, state.Token, state.User, state.ErrorMessage);
}