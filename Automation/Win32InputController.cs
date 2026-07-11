namespace WinFormsApp1
{
    public class Win32InputController : IInputController
    {
        public Rectangle WindowBounds
        {
            get => InputSimulator.windowRec;
            set => InputSimulator.windowRec = value;
        }

        public void MoveMouse(int x, int y, bool absPoint = false)
        {
            InputSimulator.MouseMove(x, y, absPoint);
        }

        public void LeftClick(int downUpDelay = 150)
        {
            InputSimulator.LeftClick(downUpDelay);
        }

        public void LeftDown()
        {
            InputSimulator.LeftDown();
        }

        public void LeftUp()
        {
            InputSimulator.LeftUp();
        }

        public void MouseWheel(int x, int y, int notches)
        {
            InputSimulator.MouseWheel(x, y, notches);
        }

        public void KeyPress(Keys key, int delay = 10)
        {
            InputSimulator.KeyPress(key, delay);
        }

        public void Delay(int milliseconds)
        {
            InputSimulator.Delay(milliseconds);
        }

        public bool ForceActivateWindow(IntPtr hWnd)
        {
            return InputSimulator.ForceActivateWindow(hWnd);
        }
    }
}
