using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using Game.Data;
using Game.Setup;
using JsonFx.Json;

namespace Game.Comm
{
    public class ApiResponse
    {
        public ApiResponse(bool success)
        {
            Success = success;
        }

        public ApiResponse(bool success, dynamic data)
        {
            Success = success;
            Data = data;
        }

        public bool Success { get; set; }

        public dynamic Data { get; set; }

        public string ErrorMessage
        {
            get
            {
                if (Success || Data == null)
                {
                    return "An error occurred";
                }

                return String.Join(", ", Data.errorMessage);
            }
        }
    }

    public static class ApiCaller
    {
        private static ApiResponse MakeCall(String model, String method, IEnumerable<KeyValuePair<string, string>> data)
        {
            var jsonReader = new JsonReader();

            StringWriter queryString = new StringWriter();
            queryString.Write(string.Format("&{0}={1}", "apiId", Config.api_id));
            queryString.Write(string.Format("&{0}={1}", "apiKey", Config.api_key));
            foreach (var kv in data)
            {
                queryString.Write(string.Format("&{0}={1}", kv.Key, Uri.EscapeDataString(kv.Value)));
            }

            HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format("http://{0}/api/{1}/{2}/?{3}",
                                                    Config.api_domain,
                                                    model,
                                                    method,
                                                    queryString));
            request.Proxy = null;
            request.Method = "GET";

            try
            {
                var webResponse = (HttpWebResponse)request.GetResponse();

                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    return new ApiResponse(false);
                }

                Stream response = webResponse.GetResponseStream();

                if (response == null)
                {
                    return new ApiResponse(false);
                }

                using (StreamReader sr = new StreamReader(response))
                {
                    var responseContent = sr.ReadToEnd();
                    dynamic responseData = jsonReader.Read(responseContent);
                    return new ApiResponse(responseData.success, responseData);
                }
            }
            catch(Exception)
            {
                return new ApiResponse(false);
            }
        }

        public static ApiResponse CheckLogin(string name, string password)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("password", password),
            };
            return MakeCall("player", "check_login", parms);
        }

        public static ApiResponse CheckLoginKey(string name, string loginKey)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("login_key", loginKey),
            };
            return MakeCall("player", "check_login", parms);
        }

        public static ApiResponse Unban(string name)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("name", name),};
            return MakeCall("player", "unban", parms);
        }

        public static ApiResponse Ban(string name)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("name", name),};
            return MakeCall("player", "ban", parms);
        }

        public static ApiResponse PlayerInfo(string nameOrEmail)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("nameOrEmail", nameOrEmail),};
            return MakeCall("player", "info", parms);
        }

        public static ApiResponse PlayerSearch(string nameOrEmail)
        {
            var parms = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("nameOrEmail", nameOrEmail),};
            return MakeCall("player", "search", parms);
        }

        public static ApiResponse RenamePlayer(string name, string newName)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("new_name", newName),
            };
            return MakeCall("player", "rename", parms);
        }

        public static ApiResponse SetPlayerRights(string name, PlayerRights rights)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("rights", ((int)rights).ToString(CultureInfo.InvariantCulture)),
            };
            return MakeCall("player", "set_rights", parms);
        }

        public static ApiResponse SetPassword(string name, string password)
        {
            var parms = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("name", name),
                    new KeyValuePair<string, string>("password", password),
            };
            return MakeCall("player", "set_password", parms);
        }
    }
}