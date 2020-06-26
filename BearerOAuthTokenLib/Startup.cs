using BearerOAuthTokenDeserializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BearerOAuthTokenLib
{
    public class OAuthTokeAPI
    {
        public static AuthenticationTicket DeserializeToken(string strToken)
        {
            AuthenticationTicket token = null;
            try
            {
                return token = DataProtector.Create().Unprotect(strToken.Trim());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public static string FindValueByKey(AuthenticationTicket t, string Name)
        {
            if (t != null && t.Identity != null)
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                System.Security.Claims.ClaimsIdentity identity = t.Identity;
                Type identityType = identity.GetType();
                PropertyInfo[] properties = identityType.GetProperties(flags);

                foreach (PropertyInfo p in properties)
                {
                    var val = p.GetValue(identity);

                    switch (p.Name)
                    {
                        case "Claims":
                            var claims = val as IEnumerable<System.Security.Claims.Claim>;
                            foreach (var c in claims)
                            {
                                if (c.Type.Equals(Name))
                                {
                                    return c.Value.ToString();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return null;
        }
    }
}
