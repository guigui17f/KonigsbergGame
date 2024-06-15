
namespace GuiGui.Konigsberg.Story
{
    public class StorySignalCenter
    {
        private static StorySignalCenter instance;
        public static StorySignalCenter Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new StorySignalCenter();
                }
                return instance;
            }
        }
        private StorySignalCenter() { }
        /// <summary>
        /// 参数：新能量值
        /// </summary>
        public Signal<int> CostPowerChangedSignal = new Signal<int>();
        /// <summary>
        /// 参数：关卡ID
        /// </summary>
        public Signal<int> SectionChangedSignal = new Signal<int>();
        /// <summary>
        /// 参数：是否开启Magic模式
        /// </summary>
        public Signal<bool> MagicModeChangedSignal = new Signal<bool>();
        public Signal MagicCostingSignal = new Signal();
    }
}