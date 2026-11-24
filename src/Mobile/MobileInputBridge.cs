using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace JnG2.Mobile
{
    public static class MobileInputBridge
    {
        private static readonly List<TouchPoint> points = new List<TouchPoint>(10);
        private static int width, height;
        public static bool Enabled { get; set; }
        public static bool GameplayEnabled { get; private set; }
        public static TouchControls Controls { get; } = new TouchControls();

        public static MouseState Update(int screenWidth, int screenHeight, bool gameplay, MouseState mouse)
        {
            if (!Enabled) return mouse;
            if (width != screenWidth || height != screenHeight)
            {
                Controls.Resize(screenWidth, screenHeight);
                width = screenWidth;
                height = screenHeight;
            }
            GameplayEnabled = gameplay;
            points.Clear();
            foreach (TouchLocation touch in TouchPanel.GetState())
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                    points.Add(new TouchPoint(touch.Id, touch.Position.X, touch.Position.Y));
            if (gameplay)
            {
                Controls.Update(points);
                return new MouseState(mouse.X, mouse.Y, mouse.ScrollWheelValue, ButtonState.Released,
                    mouse.MiddleButton, mouse.RightButton, mouse.XButton1, mouse.XButton2);
            }
            Controls.Cancel();
            if (points.Count == 0)
                return new MouseState(mouse.X, mouse.Y, mouse.ScrollWheelValue, ButtonState.Released,
                    mouse.MiddleButton, mouse.RightButton, mouse.XButton1, mouse.XButton2);
            return new MouseState((int)points[0].X, (int)points[0].Y, mouse.ScrollWheelValue,
                ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        public static Vector2 Movement => Enabled && GameplayEnabled
            ? new Vector2(Controls.MoveX, Controls.MoveY) : Vector2.Zero;

        public static bool IsDown(int action, int player) => Enabled && GameplayEnabled && player == 0
            && (Controls.Held & MapAction(action)) != TouchAction.None;
        public static bool IsPressed(int action, int player) => Enabled && GameplayEnabled && player == 0
            && (Controls.Pressed & MapAction(action)) != TouchAction.None;
        private static TouchAction MapAction(int action) => action switch
        {
            0 => TouchAction.Guns,
            1 => TouchAction.Bombs,
            2 => TouchAction.Primary,
            3 => TouchAction.Secondary,
            _ => TouchAction.None
        };
    }
}
