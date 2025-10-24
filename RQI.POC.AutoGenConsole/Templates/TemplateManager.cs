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

        private string _sprocTemplateFilePath;
        public string SprocTemplateFilePath
        {
            get
            {
                if (_sprocTemplateFilePath == null)
                {
                    _sprocTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.SPROCTemplateFilePath"];
                }
                return _sprocTemplateFilePath;
            }
        }

        private string _proxyTypeTemplateFilePath;
        public string ProxyTypeTemplateFilePath
        {
            get
            {
                if (_proxyTypeTemplateFilePath == null)
                {
                    _proxyTypeTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.ProxyTypeTemplateFilePath"];
                }
                return _proxyTypeTemplateFilePath;
            }
        }

        private string _uddtTemplateFilePath;
        public string UddtTemplateFilePath
        {
            get
            {
                if (_uddtTemplateFilePath == null)
                {
                    _uddtTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.UDDTTemplateFilePath"];
                }
                return _uddtTemplateFilePath;
            }
        }

        private string _dataModelsRootFilePath;
        public string DataModelsRootFilePath
        {
            get
            {
                if (_dataModelsRootFilePath == null)
                {
                    _dataModelsRootFilePath = ConfigurationManager.AppSettings["RQI.GEN.DataModelsRootFilePath"];
                }
                return _dataModelsRootFilePath;
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

        private string _proxyTypeRootFilePath;
        public string ProxyTypeRootFilePath
        {
            get
            {
                if (_proxyTypeRootFilePath == null)
                {
                    _proxyTypeRootFilePath = ConfigurationManager.AppSettings["RQI.GEN.ProxyTypeRootFilePath"];
                }
                return _proxyTypeRootFilePath;
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

        public string GetUddtTemplatePrimer()
        {
            var udttPrimer = File.ReadAllText(UddtTemplateFilePath);
            if (udttPrimer == null) return null;
            udttPrimer = BindHeaderInformation(udttPrimer);
            return udttPrimer;
        }

        public string GetSPROCTemplatePrimer()
        {
            var sprocPrimer = File.ReadAllText(SprocTemplateFilePath);
            if (sprocPrimer == null) return null;
            sprocPrimer = BindHeaderInformation(sprocPrimer);
            return sprocPrimer;
        }

        public string GetProxyTypeTemplatePrimer()
        {
            var proxyTypePrimer = File.ReadAllText(ProxyTypeTemplateFilePath);
            if (proxyTypePrimer == null) return null;
            proxyTypePrimer = BindHeaderInformation(proxyTypePrimer);
            return proxyTypePrimer;
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
