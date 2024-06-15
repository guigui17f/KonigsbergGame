using GuiGui.Konigsberg.Story;

namespace GuiGui.Konigsberg
{
    public class PlayerModel
    {
        private static PlayerModel instance;
        public static PlayerModel Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new PlayerModel();
                }
                return instance;
            }
        }

        public int SectionID { get; private set; }
        public int SectionCostPower { get; private set; }
        public int TotalCostPower { get; private set; }
        
        private const string SECTION_ID_SAVE_KEY = "PlayerSectionId";
        private const string SECTION_COST_POWER_SAVE_KEY = "SectionCostPower";
        private const string TOTAL_COST_POWER_SAVE_KEY = "TotalCostPower";


        private PlayerModel()
        {
            if (UserDataService.DataPersister.Exists(SECTION_ID_SAVE_KEY))
            {
                SectionID = UserDataService.DataPersister.Load<int>(SECTION_ID_SAVE_KEY);
            }
            else
            {
                SectionID = 0;
            }
            if (UserDataService.DataPersister.Exists(SECTION_COST_POWER_SAVE_KEY))
            {
                SectionCostPower = UserDataService.DataPersister.Load<int>(SECTION_COST_POWER_SAVE_KEY);
            }
            else
            {
                SectionCostPower = 0;
            }
            if (UserDataService.DataPersister.Exists(TOTAL_COST_POWER_SAVE_KEY))
            {
                TotalCostPower = UserDataService.DataPersister.Load<int>(TOTAL_COST_POWER_SAVE_KEY);
            }
            else
            {
                TotalCostPower = 0;
            }
        }

        public void SavePlayerStatus()
        {
            UserDataService.DataPersister.Save(SECTION_ID_SAVE_KEY, SectionID);
            UserDataService.DataPersister.Save(SECTION_COST_POWER_SAVE_KEY, SectionCostPower);
            UserDataService.DataPersister.Save(TOTAL_COST_POWER_SAVE_KEY, TotalCostPower);
        }

        public void SetPlayerSection(int sectionId)
        {
            SectionID = sectionId;
            StorySignalCenter.Instance.SectionChangedSignal.Dispatch(SectionID);
        }

        public void ChangePlayerCostPower(int deltaPower)
        {
            SectionCostPower += deltaPower;
            TotalCostPower += deltaPower;
            StorySignalCenter.Instance.CostPowerChangedSignal.Dispatch(TotalCostPower);
        }

        public void ResetSectionCostPower()
        {
            ChangePlayerCostPower(-SectionCostPower);
        }

        public void ClearSectionCostPower()
        {
            SectionCostPower = 0;
            UserDataService.DataPersister.Delete(SECTION_COST_POWER_SAVE_KEY);
        }

        public void ResetPlayerStatus()
        {
            ClearSectionCostPower();
            SectionID = 0;
            TotalCostPower = 0;
            UserDataService.DataPersister.Delete(SECTION_ID_SAVE_KEY);
            UserDataService.DataPersister.Delete(TOTAL_COST_POWER_SAVE_KEY);
        }
    }
}
