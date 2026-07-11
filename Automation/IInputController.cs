namespace WinFormsApp1
{
    public interface IInputController
    {
        Rectangle WindowBounds { get; set; }

        void MoveMouse(int x, int y, bool absPoint = false);

        void LeftClick(int downUpDelay = 150);

        void LeftDown();

        void LeftUp();

        void MouseWheel(int x, int y, int notches);

        void KeyPress(Keys key, int delay = 10);

        void Delay(int milliseconds);

        bool ForceActivateWindow(IntPtr hWnd);
    }
}
