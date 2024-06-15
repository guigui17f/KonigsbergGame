
namespace GuiGui
{
    public class UserDataService
    {
        private static IDataPersistable dataPersister;
        public static IDataPersistable DataPersister
        {
            get
            {
                if (null == dataPersister)
                {
                    dataPersister = new ESDataPersistence();
                }
                return dataPersister;
            }
        }
    }
}
