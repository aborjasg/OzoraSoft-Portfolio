namespace OzoraSoft.Library.Security
{
    public interface IJwtSettings
    {
        /// <summary>
        /// 
        /// </summary>
        string Issuer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string Audience { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string SecretKey { get; set; }        
    }
    /// <summary>
    /// 
    /// </summary>
    public class JwtSettings : IJwtSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string Issuer { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string Audience { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string SecretKey { get; set; } = "";
    }
}
