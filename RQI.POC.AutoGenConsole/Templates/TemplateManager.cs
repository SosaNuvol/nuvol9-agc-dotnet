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

        private string _repositoryRootFilePath;
        public string RepositoryRootFilePath
        {
            get
            {
                if (_repositoryRootFilePath == null)
                {
                    _repositoryRootFilePath = ConfigurationManager.AppSettings["RQI.GEN.RepositoryRootFilePath"];
                }
                return _repositoryRootFilePath;
            }
        }

        private string _dataEntityRootFilePath;
        public string DataEntityRootFilePath
        {
            get
            {
                if (_dataEntityRootFilePath == null)
                {
                    _dataEntityRootFilePath = ConfigurationManager.AppSettings["RQI.GEN.Data.EntityRootFilePath"];
                }
                return _dataEntityRootFilePath;
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

        private string _dbContextTemplateFilePath;
        public string DbContextTemplateFilePath
        {
            get
            {
                if (_dbContextTemplateFilePath == null)
                {
                    _dbContextTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.DbContextTemplateFilePath"];
                }
                return _dbContextTemplateFilePath;
            }
        }

        private string _repositoryInterfaceTemplateFilePath;
        public string RepositoryInterfaceTemplateFilePath
        {
            get
            {
                if (_repositoryInterfaceTemplateFilePath == null)
                {
                    _repositoryInterfaceTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.RepositoryInterfaceTemplateFilePath"];
                }
                return _repositoryInterfaceTemplateFilePath;
            }
        }

        private string _repositoryTemplateFilePath;
        public string RepositoryTemplateFilePath
        {
            get
            {
                if (_repositoryTemplateFilePath == null)
                {
                    _repositoryTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.RepositoryTemplateFilePath"];
                }
                return _repositoryTemplateFilePath;
            }
        }

        private string _unitOfWorkTemplateFilePath;
        public string UnitOfWorkTemplateFilePath
        {
            get
            {
                if (_unitOfWorkTemplateFilePath == null)
                {
                    _unitOfWorkTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.UnitOfWorkTemplateFilePath"];
                }
                return _unitOfWorkTemplateFilePath;
            }
        }

        private string _unitOfWorkInterfaceTemplateFilePath;
        public string UnitOfWorkInterfaceTemplateFilePath
        {
            get
            {
                if (_unitOfWorkInterfaceTemplateFilePath == null)
                {
                    _unitOfWorkInterfaceTemplateFilePath = ConfigurationManager.AppSettings["RQI.GEN.UnitOfWorkInterfaceTemplateFilePath"];
                }
                return _unitOfWorkInterfaceTemplateFilePath;
            }
        }

        private string _unitOfWorkFilePath;
        public string UnitOfWorkFilePath
        {
            get
            {
                if (_unitOfWorkFilePath == null)
                {
                    _unitOfWorkFilePath = ConfigurationManager.AppSettings["RQI.GEN.UnitOfWorkFilePath"];
                }
                return _unitOfWorkFilePath;
            }
        }

        private string _unitOfWorkInterfaceFilePath;
        public string UnitOfWorkInterfaceFilePath
        {
            get
            {
                if (_unitOfWorkInterfaceFilePath == null)
                {
                    _unitOfWorkInterfaceFilePath = ConfigurationManager.AppSettings["RQI.GEN.UnitOfWorkInterfaceFilePath"];
                }
                return _unitOfWorkInterfaceFilePath;
            }
        }

        private string _unitOfWorkNameSpace;
        public string UnitOfWorkNameSpace
        {
            get
            {
                if (_unitOfWorkNameSpace == null)
                {
                    _unitOfWorkNameSpace = ConfigurationManager.AppSettings["RQI.GEN.UnitOfWorkNameSpace"];
                }
                return _unitOfWorkNameSpace;
            }
        }

        private string _dbContextNamespance;
        public string DbContextNamespance
        {
            get
            {
                if (_dbContextNamespance == null)
                {
                    _dbContextNamespance = ConfigurationManager.AppSettings["RQI.GEN.DbContextNamespance"];
                }
                return _dbContextNamespance;
            }
        }

        private string _dataDataNameSpace;
        public string DataDataNameSpace
        {
            get
            {
                if (_dataDataNameSpace == null)
                {
                    _dataDataNameSpace = ConfigurationManager.AppSettings["RQI.GEN.DataDataNameSpace"];
                }
                return _dataDataNameSpace;
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
