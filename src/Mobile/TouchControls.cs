using System;
using System.Collections.Generic;

namespace JnG2.Mobile
{
    [Flags]
    public enum TouchAction { None = 0, Guns = 1, Bombs = 2, Primary = 4, Secondary = 8, Pause = 16 }

    public readonly struct TouchPoint
    {
        public readonly int Id;
        public readonly float X, Y;
        public TouchPoint(int id, float x, float y) { Id = id; X = x; Y = y; }
    }

    public readonly struct ControlCircle
    {
        public readonly float X, Y, Radius;
        public ControlCircle(float x, float y, float radius) { X = x; Y = y; Radius = radius; }
        public bool Contains(float x, float y) => (x - X) * (x - X) + (y - Y) * (y - Y) <= Radius * Radius;
    }

    // Coordinates are screen pixels. Movement Y is positive upward, matching the original gamepad input.
    public sealed class TouchControls
    {
        private int? stickFinger;
        public float MoveX { get; private set; }
        public float MoveY { get; private set; }
        public TouchAction Held { get; private set; }
        public TouchAction Pressed { get; private set; }
        public ControlCircle Stick { get; private set; }
        public ControlCircle Guns { get; private set; }
        public ControlCircle Bombs { get; private set; }
        public ControlCircle Primary { get; private set; }
        public ControlCircle Secondary { get; private set; }
        public ControlCircle Pause { get; private set; }
        public const float DeadZone = 0.12f;

        public void Resize(float width, float height, float leftInset = 0, float topInset = 0,
            float rightInset = 0, float bottomInset = 0)
        {
            float w = width - leftInset - rightInset;
            float h = height - topInset - bottomInset;
            if (w <= 0 || h <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            float unit = Math.Min(h, w / 1.6f);
            Stick = new ControlCircle(leftInset + unit * .19f, topInset + h - unit * .21f, unit * .13f);
            Guns = new ControlCircle(leftInset + w - unit * .16f, topInset + h - unit * .19f, unit * .085f);
            Bombs = new ControlCircle(leftInset + w - unit * .36f, topInset + h - unit * .16f, unit * .065f);
            Primary = new ControlCircle(leftInset + w - unit * .19f, topInset + h - unit * .40f, unit * .06f);
            Secondary = new ControlCircle(leftInset + w - unit * .37f, topInset + h - unit * .35f, unit * .06f);
            Pause = new ControlCircle(leftInset + w - unit * .08f, topInset + unit * .08f, unit * .045f);
            Cancel();
        }

        public void Cancel()
        {
            stickFinger = null;
            MoveX = MoveY = 0;
            Held = Pressed = TouchAction.None;
        }

        // Supply every currently active finger each frame; omit released/cancelled fingers.
        public void Update(IReadOnlyList<TouchPoint> touches)
        {
            if (Stick.Radius <= 0) throw new InvalidOperationException("Resize must run before touch input.");
            TouchAction next = TouchAction.None;
            MoveX = MoveY = 0;
            bool stickFound = false;
            if (stickFinger.HasValue)
            {
                for (int i = 0; i < touches.Count; i++) if (touches[i].Id == stickFinger.Value) stickFound = true;
                if (!stickFound) stickFinger = null;
            }
            for (int i = 0; i < touches.Count; i++)
            {
                TouchPoint touch = touches[i];
                if (!stickFinger.HasValue && Stick.Contains(touch.X, touch.Y)) stickFinger = touch.Id;
                if (touch.Id == stickFinger)
                {
                    float x = (touch.X - Stick.X) / Stick.Radius;
                    float y = (Stick.Y - touch.Y) / Stick.Radius;
                    float length = (float)Math.Sqrt(x * x + y * y);
                    if (length > DeadZone)
                    {
                        float magnitude = Math.Min(1f, (length - DeadZone) / (1f - DeadZone));
                        MoveX = x / length * magnitude;
                        MoveY = y / length * magnitude;
                    }
                    continue;
                }
                if (Guns.Contains(touch.X, touch.Y)) next |= TouchAction.Guns;
                if (Bombs.Contains(touch.X, touch.Y)) next |= TouchAction.Bombs;
                if (Primary.Contains(touch.X, touch.Y)) next |= TouchAction.Primary;
                if (Secondary.Contains(touch.X, touch.Y)) next |= TouchAction.Secondary;
                if (Pause.Contains(touch.X, touch.Y)) next |= TouchAction.Pause;
            }
            Pressed = next & ~Held;
            Held = next;
        }
    }
}
