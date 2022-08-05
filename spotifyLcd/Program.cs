using spotifyLcd.Services.Audio;
using spotifyLcd.Services.Lcd;
using spotifyLcd.Services.Lyrics;
using spotifyLcd.Services.Spotify;
using System;
using System.Runtime.InteropServices;
using System.Threading;

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
        private static LyricsWebservice lyricsWebService = new LyricsWebservice();

        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            while (!exitSystem)
            {
                if (SpotifyNotifier.SpotifyTalker.IsSpotifyRunning())
                {
                    var analyzer = new EqualizerNaudio();
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
        // https://www.reddit.com/r/learnprogramming/comments/2m48n1/how_do_you_figure_out_the_wm_appcommands_used_by/
        // https://searchcode.com/codesearch/view/24948059/

        static void SpotifyHandle_OnSpotifyTrackChanged(object sender, SpotifyTrackChangedEventArgs e)
        {
            Console.Clear();
            CurrentNumberName = String.IsNullOrEmpty(e.Artist) ? "" : e.Artist + " - " + e.Track;

            if (!string.IsNullOrEmpty(e.Artist) && !string.IsNullOrEmpty(e.Track))
            {
                var lyrics = lyricsWebService.GetLyrics(e.Artist, e.Track);
                if (!string.IsNullOrEmpty(lyrics))
                {
                    Console.Write(lyrics);
                }
            }
        }
    }

}

