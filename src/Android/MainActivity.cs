using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using JnG2.Mobile;
using Microsoft.Xna.Framework;

namespace JnG2.Android
{
    [Activity(Label = "Jets'n'Guns 2 Mobile Test", MainLauncher = true, Exported = true,
        Theme = "@android:style/Theme.Material.NoActionBar.Fullscreen",
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden |
            ConfigChanges.Keyboard | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize |
            ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.Density)]
    public sealed class MainActivity : AndroidGameActivity
    {
        private MobileGame game;
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            var message = new TextView(this) { Text = "Preparing game files for the first launch...", Gravity = GravityFlags.Center };
            message.SetTextColor(global::Android.Graphics.Color.White);
            message.SetBackgroundColor(global::Android.Graphics.Color.Black);
            SetContentView(message);
            try
            {
                string destination = Path.Combine(FilesDir.AbsolutePath, "game");
                await Task.Run(() => InstallAssets(destination));
                if (IsFinishing || IsDestroyed) return;
                Directory.SetCurrentDirectory(destination);
                MobileInputBridge.Enabled = true;
                game = new MobileGame();
                SetContentView((View)game.Services.GetService(typeof(View)));
                game.Run();
            }
            catch (Exception ex)
            {
                message.Text = "The test build could not start.\n\n" + ex.Message;
                SetContentView(message);
                File.WriteAllText(Path.Combine(FilesDir.AbsolutePath, "startup-error.txt"), ex.ToString());
            }
        }

        private void InstallAssets(string destination)
        {
            Directory.CreateDirectory(destination);
            string marker = Path.Combine(destination, ".assets-0.1-ready");
            if (File.Exists(marker)) return;
            CopyAsset("content.zip", destination);
            CopyAsset("content.txt", destination);
            foreach (string folder in new[] { "music", "shader" })
                foreach (string name in Assets.List(folder)) CopyAsset(folder + "/" + name, destination);
            File.WriteAllText(marker, "complete");
        }

        private void CopyAsset(string name, string destination)
        {
            string output = Path.Combine(destination, name);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            using (var input = Assets.Open(name))
            using (var stream = File.Create(output + ".installing")) input.CopyTo(stream);
            File.Move(output + ".installing", output, true);
        }

        protected override void OnDestroy()
        {
            MobileInputBridge.Controls.Cancel();
            game?.Dispose();
            base.OnDestroy();
        }
    }
}
