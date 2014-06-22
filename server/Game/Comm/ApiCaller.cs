using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Flurl;
using Game.Comm.Api;
using Game.Data;
using Game.Data.Store;
using Game.Setup;
using Game.Util;
using Newtonsoft.Json;

namespace Game.Comm
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public T Data { get; set; }

        public IEnumerable<string> ErrorMessages { get; set; }

        public string AllErrorMessages
        {
            get
            {
                return ErrorMessages == null ? string.Empty : string.Join(",", ErrorMessages);
            }
        }

        public Error AsErrorEnumerable()
        {
            if (Success)
            {
                return Error.Ok;
            }

            Error error;
            if (ErrorMessages == null || !ErrorMessages.Any() || !Error.TryParse(ErrorMessages.First().Replace("_", ""), true, out error))
            {
                return Error.Unexpected;
            }
            
            return error;
        }
    }

    public static class ApiCaller
    {
        private static ApiResponse<dynamic> MakeCall(String model, String method, IEnumerable<KeyValuePair<string, string>> data)
        {
            return MakeCall<dynamic>(model, method, data);
        }

        private static ApiResponse<T> MakeCall<T>(String model, String method, IEnumerable<KeyValuePair<string, string>> data)
        {
            var queryParameters = data.ToDictionary(k => k.Key, v => v.Value);
            queryParameters["apiId"] = Config.api_id;
            queryParameters["apiKey"] = Config.api_key;

            if (Config.xdebug_enabled)
            {
                queryParameters["XDEBUG_SESSION_START"] = "PHPSTORM";
            }

            var url = new Url("http://" + Config.api_domain)
                    .AppendPathSegment("api")
                    .AppendPathSegment(model)
                    .AppendPathSegment(method)
                    .SetQueryParams(queryParameters);

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Proxy = null;
            request.Method = "GET";

            try
            {
                var webResponse = (HttpWebResponse)request.GetResponse();

                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    return new ApiResponse<T> { Success = false, ErrorMessages = new[] { "Failed with status code:" + webResponse.StatusCode } };
                }

                Stream response = webResponse.GetResponseStream();

                if (response == null)
                {
                    return new ApiResponse<T> { Success = false, ErrorMessages = new[] { "Failed with null response" } };
                }

                using (StreamReader sr = new StreamReader(response))
                {
                    var responseContent = sr.ReadToEnd();

                    return JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent, new JsonSerializerSettings
                    {
                        ContractResolver = new SnakeCasePropertyNamesContractResolver()
                    });
                }
            }
            catch(Exception e)
            {
                return new ApiResponse<T> { Success = false, ErrorMessages = new[] { "Failed with exception: " + e.Message } };
            }
        }

        public static ApiResponse<LoginResponseData> CheckLogin(string name, string password)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("password", password),
            };

            return MakeCall<LoginResponseData>("player", "check_login", parms);
        }

        public static ApiResponse<LoginResponseData> CheckLoginKey(string name, string loginKey)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("login_key", loginKey),
            };
            return MakeCall<LoginResponseData>("player", "check_login", parms);
        }

        public static ApiResponse<dynamic> Unban(string name)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("name", name),};
            return MakeCall("player", "unban", parms);
        }

        public static ApiResponse<dynamic> Ban(string name)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("name", name),};
            return MakeCall("player", "ban", parms);
        }

        public static ApiResponse<dynamic> PlayerInfo(string nameOrEmail)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("nameOrEmail", nameOrEmail),};
            return MakeCall("player", "info", parms);
        }

        public static ApiResponse<dynamic> PlayerSearch(string nameOrEmail)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("nameOrEmail", nameOrEmail),};
            return MakeCall("player", "search", parms);
        }

        public static ApiResponse<dynamic> RenamePlayer(string name, string newName)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("new_name", newName),
            };
            return MakeCall("player", "rename", parms);
        }

        public static ApiResponse<dynamic> SetPlayerRights(string name, PlayerRights rights)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("rights", ((int)rights).ToString(CultureInfo.InvariantCulture)),
            };
            return MakeCall("player", "set_rights", parms);
        }

        public static ApiResponse<dynamic> SetPassword(string name, string password)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("password", password),
            };
            return MakeCall("player", "set_password", parms);
        }

        public static ApiResponse<dynamic> GiveAchievement(string name, AchievementTier tier, string type, string icon, string title, string description)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("tier", ((byte)tier).ToString()),
                    new KeyValuePair<string, string>("icon", icon),
                    new KeyValuePair<string, string>("type", type),
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description),
            };

            return MakeCall("player", "give_achievement", parms);
        }

        public static ApiResponse<dynamic> ResetAuthCode(string name)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name)
            };
            return MakeCall("player", "reset_auth_code", parms);
        }

        public static ApiResponse<IEnumerable<StoreItem>> StoreItemGetAll()
        {
            return MakeCall<IEnumerable<StoreItem>>("store_item", "get_all", new Dictionary<string, string>());
        }

        public static ApiResponse<object> AddCoins(string playerName, int coins)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", playerName),
                new KeyValuePair<string, string>("coins", coins.ToString(CultureInfo.InvariantCulture))
            };

            return MakeCall("player", "add_coins", parms);
        }

        public static ApiResponse<dynamic> PurchaseItem(uint playerId, string itemId)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("playerId", playerId.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("itemId", itemId)
            };

            return MakeCall("store_item", "purchase_item", parms);
        }
    }
}