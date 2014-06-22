using Game.Comm.Api;
using Game.Setup;

namespace Game.Comm
{
    enum LoginHandlerMode
    {
        SessionIdLogin = 0,
        PasswordLogin = 1
    }

    interface ILoginHandler
    {
        Error Login(LoginHandlerMode loginMode, string playerName, string playerLoginKey, out LoginResponseData loginData);
    }
}
