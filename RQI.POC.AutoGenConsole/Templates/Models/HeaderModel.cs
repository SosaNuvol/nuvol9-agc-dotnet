namespace RQI.POC.AutoGenConsole.Templates.Models
{
    using Newtonsoft.Json;
    using System;

    [Serializable]
    public class HeaderModel
    {
        [JsonProperty("companyName")]
        public string CompanyName { get; set; } = "Núvol 9 Corp";

        [JsonProperty("companyWebSiteUrl")]
        public string CompanyWebSiteUrl { get; set; } = "http://www.nuvol9.com";

        [JsonProperty("todayDate")]
        public string TodayDate { get; set; } = DateTimeOffset.Now.ToString();

        [JsonProperty("authorFullName")]
        public string AuthorFullName { get; set; } = "Andrés Sosa";

        [JsonProperty("authorEmail")]
        public string AuthorEmail { get; set; } = "andres@nuvol9.com";
    }
}
