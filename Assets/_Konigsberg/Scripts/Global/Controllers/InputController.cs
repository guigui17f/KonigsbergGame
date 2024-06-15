using HedgehogTeam.EasyTouch;

namespace GuiGui.Konigsberg
{
    public class InputController
    {
        private static InputController instance;
        public static InputController Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new InputController();
                }
                return instance;
            }
        }

        public bool Enable
        {
            get { return EasyTouch.GetEnabled(); }
        }

        private InputController() { }

        public void SetEnable(bool enable)
        {
            EasyTouch.SetEnabled(enable);
        }
    }
}
