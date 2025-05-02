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

        private string _viewTemplateFilePath;
        public string ViewTemplateFilePath
        {
            get
            {
                if (_viewTemplateFilePath == null)
                {
                    _viewTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.ViewTemplateFilePath"];
                }
                return _viewTemplateFilePath;
            }
        }

        private string _dataAccessRootFilePath;
        public string DataAccessRootFilePath
        {
            get
            {
                if (_dataAccessRootFilePath == null)
                {
                    _dataAccessRootFilePath = ConfigurationManager.AppSettings["RQI.GEN.DataAccessRootFilePath"];
                }
                return _dataAccessRootFilePath;
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

        public string GetViewTemplatePrimer()
        {
            var viewPrimer = File.ReadAllText(ViewTemplateFilePath);
            if (viewPrimer == null) return null;
            
            viewPrimer = BindHeaderInformation(viewPrimer);
            return viewPrimer;
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
