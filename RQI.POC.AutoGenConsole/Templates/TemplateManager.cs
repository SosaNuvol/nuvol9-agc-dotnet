namespace RQI.POC.AutoGenConsole.Templates
{
    using RQI.POC.AutoGenConsole.Templates.Models;
    using System;
    using System.IO;

    public class TemplateManager
    {
        public string TableTemplateFilePath => RQI.POC.AutoGen.Utilities.TableTemplateFilePath;
        public string ViewTemplateFilePath => RQI.POC.AutoGen.Utilities.ViewTemplateFilePath;
        public string SprocTemplateFilePath => RQI.POC.AutoGen.Utilities.SprocTemplateFilePath;
        public string ProxyTypeTemplateFilePath => RQI.POC.AutoGen.Utilities.ProxyTypeTemplateFilePath;
        public string UddtTemplateFilePath => RQI.POC.AutoGen.Utilities.UddtTemplateFilePath;
        public string DataModelsRootFilePath => RQI.POC.AutoGen.Utilities.DataModelsRootFilePath;
        public string DataAccessRootFilePath => RQI.POC.AutoGen.Utilities.DataAccessRootFilePath;
        public string RepositoryRootFilePath => RQI.POC.AutoGen.Utilities.RepositoryRootFilePath;
        public string DataEntityRootFilePath => RQI.POC.AutoGen.Utilities.DataEntityRootFilePath;
        public string ProxyTypeRootFilePath => RQI.POC.AutoGen.Utilities.ProxyTypeRootFilePath;
        public string DbContextTemplateFilePath => RQI.POC.AutoGen.Utilities.DbContextTemplateFilePath;
        public string RepositoryInterfaceTemplateFilePath => RQI.POC.AutoGen.Utilities.RepositoryInterfaceTemplateFilePath;
        public string RepositoryTemplateFilePath => RQI.POC.AutoGen.Utilities.RepositoryTemplateFilePath;
        public string UnitOfWorkTemplateFilePath => RQI.POC.AutoGen.Utilities.UnitOfWorkTemplateFilePath;
        public string UnitOfWorkInterfaceTemplateFilePath => RQI.POC.AutoGen.Utilities.UnitOfWorkInterfaceTemplateFilePath;
        public string UnitOfWorkFilePath => RQI.POC.AutoGen.Utilities.UnitOfWorkFilePath;
        public string UnitOfWorkInterfaceFilePath => RQI.POC.AutoGen.Utilities.UnitOfWorkInterfaceFilePath;
        public string UnitOfWorkNameSpace => RQI.POC.AutoGen.Utilities.UnitOfWorkNameSpace;
        public string DbContextNamespance => RQI.POC.AutoGen.Utilities.DbContextNamespance;
        public string DataDataNameSpace => RQI.POC.AutoGen.Utilities.DataDataNameSpace;

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

        public string GetDbContextTemplatePrimer()
        {
            if (string.IsNullOrEmpty(DbContextTemplateFilePath) || !File.Exists(DbContextTemplateFilePath))
            {
                return null;
            }
            var dbContextPrimer = File.ReadAllText(DbContextTemplateFilePath);
            if (dbContextPrimer == null) return null;
            dbContextPrimer = BindHeaderInformation(dbContextPrimer);
            return dbContextPrimer;
        }

        public string GetRepositoryInterfaceTemplatePrimer()
        {
            if (string.IsNullOrEmpty(RepositoryInterfaceTemplateFilePath) || !File.Exists(RepositoryInterfaceTemplateFilePath))
            {
                return null;
            }
            var repositoryInterfacePrimer = File.ReadAllText(RepositoryInterfaceTemplateFilePath);
            if (repositoryInterfacePrimer == null) return null;
            repositoryInterfacePrimer = BindHeaderInformation(repositoryInterfacePrimer);
            return repositoryInterfacePrimer;
        }

        public string GetRepositoryTemplatePrimer()
        {
            if (string.IsNullOrEmpty(RepositoryTemplateFilePath) || !File.Exists(RepositoryTemplateFilePath))
            {
                return null;
            }
            var repositoryPrimer = File.ReadAllText(RepositoryTemplateFilePath);
            if (repositoryPrimer == null) return null;
            repositoryPrimer = BindHeaderInformation(repositoryPrimer);
            return repositoryPrimer;
        }

        public string GetUnitOfWorkTemplatePrimer()
        {
            if (string.IsNullOrEmpty(UnitOfWorkTemplateFilePath) || !File.Exists(UnitOfWorkTemplateFilePath))
            {
                return null;
            }
            var unitOfWorkPrimer = File.ReadAllText(UnitOfWorkTemplateFilePath);
            if (unitOfWorkPrimer == null) return null;
            unitOfWorkPrimer = BindHeaderInformation(unitOfWorkPrimer);
            return unitOfWorkPrimer;
        }

        public string GetUnitOfWorkInterfaceTemplatePrimer()
        {
            if (string.IsNullOrEmpty(UnitOfWorkInterfaceTemplateFilePath) || !File.Exists(UnitOfWorkInterfaceTemplateFilePath))
            {
                return null;
            }
            var unitOfWorkInterfacePrimer = File.ReadAllText(UnitOfWorkInterfaceTemplateFilePath);
            if (unitOfWorkInterfacePrimer == null) return null;
            unitOfWorkInterfacePrimer = BindHeaderInformation(unitOfWorkInterfacePrimer);
            return unitOfWorkInterfacePrimer;
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
