namespace OzoraSoft.Library.Security
{ 
    public class TokenResponse
    {
        /// <summary>
        /// access token
        /// </summary>
        public string access_token { get; set; } = "";

        /// <summary>
        /// token type
        /// </summary>
        public string token_type { get; set; } = "";

        /// <summary>
        /// expires in
        /// </summary>
        public int expires_in { get; set; } = 0;

        /// <summary>
        /// scope
        /// </summary>
        public string scope { get; set; } = "";

        /// <summary>
        /// refresh token
        /// </summary>
        public string refresh_token { get; set; } = "";
    }
}
