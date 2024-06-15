
namespace GuiGui.Konigsberg
{
    public class GameModel
    {
        private static GameModel instance;
        public static GameModel Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new GameModel();
                }
                return instance;
            }
        }

        private const string CURRENT_MAPDATA_SAVE_KEY = "CurrentMapData";
        private const string CURRENT_SECTIONSTATE_SAVE_KEY = "CurrentSectionState";
        private const string CURRENT_LANGUAGE_SAVE_KEY = "CurrentLanguage";

        private GameModel() { }

        public MapData GetCurrentMapData()
        {
            if (UserDataService.DataPersister.Exists(CURRENT_MAPDATA_SAVE_KEY))
            {
                return UserDataService.DataPersister.Load<MapData>(CURRENT_MAPDATA_SAVE_KEY);
            }
            else
            {
                return null;
            }
        }

        public bool[] GetCurrentSectionState()
        {
            if (UserDataService.DataPersister.Exists(CURRENT_SECTIONSTATE_SAVE_KEY))
            {
                return UserDataService.DataPersister.LoadArray<bool>(CURRENT_SECTIONSTATE_SAVE_KEY);
            }
            else
            {
                return null;
            }
        }

        public string GetCurrentLanguage()
        {
            if (UserDataService.DataPersister.Exists(CURRENT_LANGUAGE_SAVE_KEY))
            {
                return UserDataService.DataPersister.Load<string>(CURRENT_LANGUAGE_SAVE_KEY);
            }
            else
            {
                return null;
            }
        }

        public void SaveCurrentMapData(MapData mapData)
        {
            UserDataService.DataPersister.Save(CURRENT_MAPDATA_SAVE_KEY, mapData);
        }

        public void SaveCurrentSectionState(bool[] sectionState)
        {
            UserDataService.DataPersister.Save(CURRENT_SECTIONSTATE_SAVE_KEY, sectionState);
        }

        public void SaveCurrentLanguage(string language)
        {
            UserDataService.DataPersister.Save(CURRENT_LANGUAGE_SAVE_KEY, language);
        }

        public void CleanCurrentMapData()
        {
            UserDataService.DataPersister.Delete(CURRENT_MAPDATA_SAVE_KEY);
        }

        public void CleanCurrentSectionState()
        {
            UserDataService.DataPersister.Delete(CURRENT_SECTIONSTATE_SAVE_KEY);
        }
    }
}
