using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Database
{
    public class SalesforceConfig
    {
        public int SalesforceConfigId { get; set; }

        public string EnvironmentUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SecurityToken { get; set; }

        public string ApiVersion { get; set; }
    }
}
