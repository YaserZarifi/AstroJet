using System;
using JnG2.Mobile;

namespace JnG2.Verification
{
    public static class TouchControlChecks
    {
        static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        static TouchPoint At(int id, ControlCircle c) => new TouchPoint(id, c.X, c.Y);
        public static string Run()
        {
            var controls = new TouchControls();
            controls.Resize(2400, 1080, 40, 0, 40, 24);
            controls.Update(new[] { At(1, controls.Stick), At(2, controls.Guns), At(3, controls.Bombs) });
            Check(controls.MoveX == 0 && controls.MoveY == 0, "Stick center must be stationary.");
            Check(controls.Held == (TouchAction.Guns | TouchAction.Bombs), "Simultaneous firing failed.");
            Check(controls.Pressed == controls.Held, "Initial press edges were lost.");
            controls.Update(new[] { new TouchPoint(1, controls.Stick.X + controls.Stick.Radius * 3,
                controls.Stick.Y - controls.Stick.Radius * 3), At(2, controls.Guns) });
            Check(Math.Abs(controls.MoveX * controls.MoveX + controls.MoveY * controls.MoveY - 1) < .0001,
                "Diagonal motion must remain within unit magnitude.");
            Check(controls.MoveX > 0 && controls.MoveY > 0, "Gamepad Y orientation is wrong.");
            Check(controls.Pressed == TouchAction.None && controls.Held == TouchAction.Guns, "Held buttons must not repeat press edges.");
            controls.Update(new[] { At(2, controls.Guns) });
            Check(controls.MoveX == 0 && controls.MoveY == 0, "Releasing the stick must stop movement.");
            controls.Cancel();
            Check(controls.Held == TouchAction.None && controls.Pressed == TouchAction.None, "Pause/cancel must release input.");
            controls.Update(new[] { new TouchPoint(4, controls.Stick.X + controls.Stick.Radius * .05f, controls.Stick.Y) });
            Check(controls.MoveX == 0, "Dead zone did not suppress small drift.");
            controls.Resize(1920, 1080);
            controls.Update(new[] { At(5, controls.Pause) });
            Check(controls.Pressed == TouchAction.Pause, "Pause control failed after resizing.");
            controls.Update(Array.Empty<TouchPoint>());
            Check(controls.Held == TouchAction.None, "Released actions must not remain held.");
            return "Passed touch checks: multitouch, press edges, movement normalization, upward motion, release, cancel, dead zone, resize and pause.";
        }
    }
}
