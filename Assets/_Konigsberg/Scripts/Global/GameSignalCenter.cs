
namespace GuiGui.Konigsberg
{
    public class GameSignalCenter
    {
        private static GameSignalCenter instance;
        public static GameSignalCenter Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new GameSignalCenter();
                }
                return instance;
            }
        }
        private GameSignalCenter() { }
        public Signal ResetSectionSignal = new Signal();
        /// <summary>
        /// 参数:nodeId, nodeState
        /// </summary>
        public Signal<int, bool> NodeStateChangingSignal = new Signal<int, bool>();
        public Signal NodeStateChangedSignal = new Signal();
        /// <summary>
        /// 参数:是否打开音乐
        /// </summary>
        public Signal<bool> SwitchMusicSignal = new Signal<bool>();
        /// <summary>
        /// 参数:是否打开音效
        /// </summary>
        public Signal<bool> SwitchSFXSignal = new Signal<bool>();
        /// <summary>
        /// 参数:音效类型
        /// </summary>
        public Signal<SFXType> PlaySFXSignal = new Signal<SFXType>();
        public Signal StartTutorialSignal = new Signal();
    }
}
