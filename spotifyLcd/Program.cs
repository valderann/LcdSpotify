using spotifyLcd.Services.Lcd;
using spotifyLcd.Services.Spotify;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace spotifyLcd
{
    class Program
    {
        static bool exitSystem = false;
        #region Trap application termination
            [DllImport("Kernel32")]
            private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

            private delegate bool EventHandler(CtrlType sig);
            static EventHandler _handler;

            enum CtrlType
            {
                CTRL_C_EVENT = 0,
                CTRL_BREAK_EVENT = 1,
                CTRL_CLOSE_EVENT = 2,
                CTRL_LOGOFF_EVENT = 5,
                CTRL_SHUTDOWN_EVENT = 6
            }

            private static bool Handler(CtrlType sig)
            {
                exitSystem = true;
                Environment.Exit(-1);
                return true;
            }
        #endregion
        
        
        private static string CurrentNumberName = "";
        static void Main(string[] args)
        {
                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);
  
                while (!exitSystem)
                {
                    if (SpotifyNotifier.SpotifyTalker.IsSpotifyRunning())
                    {
                        var analyzer = new AudioAnalyzer();
                        analyzer.Start();
                        using (var pannel = new SpotifyLcdPannel())
                        {
                            var spotifyHandle = new SpotifyNotifier.SpotifyTalker();
                            spotifyHandle.OnSpotifyTrackChanged += SpotifyHandle_OnSpotifyTrackChanged;
                            spotifyHandle.Initialize();

                            while (SpotifyNotifier.SpotifyTalker.IsSpotifyRunning())
                            {
                                //Back or forward button pressed?
                                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_MONO_BUTTON_0)) { spotifyHandle.PreviousTrack(); }
                                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_MONO_BUTTON_1)) { spotifyHandle.NextTrack(); }
                               
                                //Update LCD pannel
                                pannel.UpdatePannel(CurrentNumberName, analyzer.GetSpectrumData());

                                Thread.Sleep(100);
                            }
                            spotifyHandle.OnSpotifyTrackChanged -= SpotifyHandle_OnSpotifyTrackChanged;
                            spotifyHandle.Uninitialize();
                        }
                        analyzer.Stop();
                        analyzer.Free();
                    }
                    Thread.Sleep(500);
                }
               
        }

        static void SpotifyHandle_OnSpotifyTrackChanged(object sender, SpotifyTrackChangedEventArgs e)
        {
            CurrentNumberName = String.IsNullOrEmpty(e.Artist) ? "" : e.Artist + " - " + e.Track;
        }
    }

}
   
