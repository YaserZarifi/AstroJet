using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D;

namespace JnG2.Mobile
{
    public sealed class MobileGame : GameCore
    {
        private SpriteBatch overlay;
        private Texture2D pixel;
        public MobileGame()
        {
            GraphicsDeviceManager.IsFullScreen = true;
            GraphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            GraphicsDeviceManager.PreferredBackBufferWidth = Math.Max(display.Width, display.Height);
            GraphicsDeviceManager.PreferredBackBufferHeight = Math.Min(display.Width, display.Height);
        }
        protected override void Draw(GameTime time)
        {
            base.Draw(time);
            if (!MobileInputBridge.Enabled || !MobileInputBridge.GameplayEnabled) return;
            if (overlay == null)
            {
                overlay = new SpriteBatch(GraphicsDevice);
                pixel = new Texture2D(GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);
            var controls = MobileInputBridge.Controls;
            float sx = GraphicsDevice.Viewport.Width / (float)Window.ClientBounds.Width;
            float sy = GraphicsDevice.Viewport.Height / (float)Window.ClientBounds.Height;
            overlay.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateScale(sx, sy, 1));
            Ring(controls.Stick, Color.White * .3f);
            var thumb = new ControlCircle(controls.Stick.X + controls.MoveX * controls.Stick.Radius,
                controls.Stick.Y - controls.MoveY * controls.Stick.Radius, controls.Stick.Radius * .35f);
            Ring(thumb, Color.Cyan * .65f);
            Ring(controls.Guns, (controls.Held & TouchAction.Guns) != 0 ? Color.Cyan : Color.White * .55f);
            Ring(controls.Bombs, Color.Orange * .65f);
            Ring(controls.Primary, Color.LimeGreen * .6f);
            Ring(controls.Secondary, Color.Violet * .6f);
            Ring(controls.Pause, Color.White * .6f);
            // Simple weapon and pause symbols remain legible without loading another font.
            Line(controls.Guns.X - 12, controls.Guns.Y, controls.Guns.X + 12, controls.Guns.Y, Color.White);
            Line(controls.Guns.X, controls.Guns.Y - 12, controls.Guns.X, controls.Guns.Y + 12, Color.White);
            Line(controls.Pause.X - 6, controls.Pause.Y - 10, controls.Pause.X - 6, controls.Pause.Y + 10, Color.White);
            Line(controls.Pause.X + 6, controls.Pause.Y - 10, controls.Pause.X + 6, controls.Pause.Y + 10, Color.White);
            overlay.End();
        }
        private void Ring(ControlCircle circle, Color color)
        {
            for (int i = 0; i < 48; i++)
            {
                float a = i * MathHelper.TwoPi / 48, b = (i + 1) * MathHelper.TwoPi / 48;
                Line(circle.X + (float)Math.Cos(a) * circle.Radius, circle.Y + (float)Math.Sin(a) * circle.Radius,
                    circle.X + (float)Math.Cos(b) * circle.Radius, circle.Y + (float)Math.Sin(b) * circle.Radius, color);
            }
        }
        private void Line(float x1, float y1, float x2, float y2, Color color)
        {
            var delta = new Vector2(x2 - x1, y2 - y1);
            overlay.Draw(pixel, new Vector2(x1, y1), null, color, (float)Math.Atan2(delta.Y, delta.X),
                Vector2.Zero, new Vector2(delta.Length(), 3), SpriteEffects.None, 0);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { overlay?.Dispose(); pixel?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
