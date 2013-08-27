//sqlmetal /server:. /database:SEMObjects /dbml:SEMObjects.dbml /namespace:HostingService.Test.Databases.SEMObjects

using System.Configuration;

namespace HostingService.Test.Databases.SEMObjects
{
    partial class SEMObjects
    {
        public SEMObjects()
            : this(ConfigurationManager.AppSettings["SEMObjectDBConnectionString"])
        { }
    }
}
