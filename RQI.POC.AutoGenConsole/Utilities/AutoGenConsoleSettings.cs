using System.Configuration;

namespace RQI.POC.AutoGenConsole.Utilities
{
    public static class AutoGenConsoleSettings
    {
        private static string _sqlConnectionString = string.Empty;
        public static string SqlConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_sqlConnectionString))
                {
                    _sqlConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                }
                return _sqlConnectionString;
            }
        }

        private static string _outputBaseDirectory = string.Empty;
        public static string OutputBaseDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_outputBaseDirectory))
                {
                    _outputBaseDirectory = ConfigurationManager.AppSettings["RQI.GEN.OutputBaseDir"];
                }
                return _outputBaseDirectory;
            }
        }

        private static string _baseNameSpace = string.Empty;
        public static string BaseNameSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_baseNameSpace))
                {
                    _baseNameSpace = ConfigurationManager.AppSettings["RQI.GEN.BaseNameSpace"];
                }
                return _baseNameSpace;
            }
        }

        public static string _facadeNameSpace = string.Empty;
        public static string FacadeNameSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_facadeNameSpace))
                {
                    _facadeNameSpace = ConfigurationManager.AppSettings["RQI.GEN.FacadeNameSpace"];
                }
                return _facadeNameSpace;
            }
        }
    }
}
