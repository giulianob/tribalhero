using System;
using Common;
using Game.Comm.Api;
using Game.Setup;
using Game.Util;

namespace Game.Comm
{
    class MainSiteLoginHandler : ILoginHandler
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<MainSiteLoginHandler>();
        
        public Error Login(LoginHandlerMode loginMode, string playerName, string playerLoginKey, out LoginResponseData loginData)
        {
            loginData = null;
            ApiResponse<LoginResponseData> response;
            try
            {
                response = loginMode == 0
                                   ? ApiCaller.CheckLoginKey(playerName, playerLoginKey)
                                   : ApiCaller.CheckLogin(playerName, playerLoginKey);
            }
            catch(Exception e)
            {
                logger.Error("Error loading player", e);
                return Error.Unexpected;
            }

            if (!response.Success)
            {
                return Error.InvalidLogin;
            }

            loginData = response.Data;
            return Error.Ok;
        }
    }
}