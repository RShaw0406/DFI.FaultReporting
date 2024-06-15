using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Common.SessionStorage
{
    public static class SessionStorage
    {
        public static void SetInSession(this ISession currentSession, string sessionKey, object sessionObject)
        {
            currentSession.SetString(sessionKey, JsonConvert.SerializeObject(sessionObject));
        }

        public static T GetFromSession<T>(this ISession currentSession, string sessionKey)
        {
            var sessionObject = currentSession.GetString(sessionKey);
            return sessionObject == null ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject);
        }
    }
}
