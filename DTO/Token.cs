
namespace Docusign_Connect.DTO
{
    public class Token
    {
        public string access_token { get; set; }
        public string created_at { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }
    public class JwtGrant
    {
        public string grant_type { get; set; }
        public string assertion { get; set; }

    }

}