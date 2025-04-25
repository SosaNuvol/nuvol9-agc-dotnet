namespace RQI.POC.AutoGenConsole.Templates
{
    using RQI.POC.AutoGenConsole.Templates.Models;
    using System;
    using System.Configuration;
    using System.IO;

    public class TemplateManager
    {
        private string _tableTemplateFilePath;
        public string TableTemplateFilePath
        {
            get
            {
                if (_tableTemplateFilePath == null)
                {
                    _tableTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.TableTemplateFilePath"];
                }
                return _tableTemplateFilePath;
            }
        }

        private string _templateDataBound;
        public string TemplateDataBound
        {
            get
            {
                if (_templateDataBound == null)
                {
                    _templateDataBound = GetTableTemplatePrimer();
                }
                return _templateDataBound;
            }
        }

        public TemplateManager() { }

        public string GetTableTemplatePrimer()
        {
            var tablePrimer = File.ReadAllText(TableTemplateFilePath);
            if (tablePrimer == null) return null;

            tablePrimer = BindHeaderInformation(tablePrimer);

            return tablePrimer;
        }

        private string BindHeaderInformation(string templateData)
        {
            var result = templateData.Trim();
            var headerData = new HeaderModel();

            result = result
                .Replace("<COMPANY NAME>", headerData.CompanyName)
                .Replace("<YEAR>", DateTimeOffset.Now.Year.ToString())
                .Replace("<WEBSITEURL>", headerData.CompanyWebSiteUrl)
                .Replace("<DATE FULL>", DateTimeOffset.Now.ToString())
                .Replace("<AUTHOR FULLNAME>", headerData.AuthorFullName)
                .Replace("<AUTHOR EMAIL>", headerData.AuthorEmail)
                .Replace("<AUTHOR EMAIL>", headerData.AuthorEmail);

            return result;
        }
    }
}
