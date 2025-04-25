using System.Configuration;

namespace RQI.POC.AutoGenConsole.Utilities
{
    public class GeneratorBase
    {
        private string _dataAccessNameSpace = null;
        public string DataAccessNameSpace
        {
            get
            {
                if (this._dataAccessNameSpace == null)
                {
                    _dataAccessNameSpace = ConfigurationManager.AppSettings["RQI.GEN.DataAccessNameSpace"];
                }
                return this._dataAccessNameSpace;
            }
        }
    }
}
