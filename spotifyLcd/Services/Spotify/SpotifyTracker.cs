using spotifyLcd.Domain;
using spotifyLcd.Services.Spotify;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace spotifyLcd
{
    public class SpotifyNotifier
    {
        enum SpotifyKeys
        {
            PlayPause,
            Next,
            Previous,
            VolumeUp,
            VolumeDown,
            Mute
        }

        public class SpotifyTalker
        {
            #region Events
                public delegate void SpotifyTrackChangedHandler(object sender, SpotifyTrackChangedEventArgs e);
                public event SpotifyTrackChangedHandler OnSpotifyTrackChanged;
            #endregion

            #region Private members
                private int _hWindow;
                private string _strWindowTitle;
                private Thread _thread;
                private Dictionary<SpotifyKeys, string> _keyMap;
            #endregion

            #region Constructors
                public SpotifyTalker()
                {
                    _keyMap = new Dictionary<SpotifyKeys, string>();
                    _keyMap[SpotifyKeys.PlayPause] = " ";
                    _keyMap[SpotifyKeys.Next] = "^{RIGHT}";
                    _keyMap[SpotifyKeys.Previous] = "^{LEFT}";
                    _keyMap[SpotifyKeys.VolumeUp] = "^{UP}";
                    _keyMap[SpotifyKeys.VolumeDown] = "^{DOWN}";
                    _keyMap[SpotifyKeys.Mute] = "^+{DOWN}";
                }
            #endregion

            #region Properties
                public string CurrentWindowTitle
                {
                    get { return _strWindowTitle; }
                }
            #endregion

            #region Public Methods
                public void Initialize()
                {
                    EnsureSpotifyIsRunning();

                    _hWindow = GetSpotifyWindowHandle();
                    if (_hWindow == -1)
                    {
                       //throw new Exception("Spotify window handle not found");
                    }
                    _strWindowTitle = ""; 

                    try
                    {
 
                        _thread = new Thread(new ThreadStart(this.ListenSpotifyChanges));
                        _thread.Start();

                        // Wait until thread is alive..
                        while (!_thread.IsAlive) ;
                        ExecuteListener();
                    }
                    catch (Exception ex)
                    {
                        Uninitialize();
                       // throw ex;
                    }
                }

                public void Uninitialize()
                {
                    try
                    {
                        if (_thread != null)
                        {
                            // Request abort..
                            _thread.Abort();
                            // Wait for thread to finish
                            _thread.Join();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                public void TogglePause()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.Send(_keyMap[SpotifyKeys.PlayPause]);
                }

                public void NextTrack()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.SendWait(_keyMap[SpotifyKeys.Next]);
                }

                public void PreviousTrack()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.SendWait(_keyMap[SpotifyKeys.Previous]);
                }

                public void VolumeUp()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.Send(_keyMap[SpotifyKeys.VolumeUp]);
                }

                public void VolumeDown()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.Send(_keyMap[SpotifyKeys.VolumeDown]);
                }

                public void ToggleMute()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SendKeys.Send(_keyMap[SpotifyKeys.Mute]);
                }

                public void ShowSpotify()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.ShowWindow(_hWindow, 1);
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SpotifyNative.SetFocus(_hWindow);
                }

                public void HideSpotify()
                {
                    if (!HasWindowHandle())
                    {
                        return;
                    }
                    SpotifyNative.ShowWindow(_hWindow, 0);
                    SpotifyNative.SetForegroundWindow(_hWindow);
                    SpotifyNative.SetFocus(_hWindow);
                }
           #endregion

            #region Helper methods
                private void ParseWindowTitle(string title, ref string artist, ref string track)
                {
                    if (string.IsNullOrEmpty(title))
                    {
                        return;
                    }

                    string ss = title.Replace("Spotify", "");
                    ss = ss.TrimStart('-', ' ');
                    string[] parts = ss.Split(new string[] { " - " }, StringSplitOptions.None); 
                    if (parts.Length == 0)
                    {
                        return;
                    }
                    if (parts.Length >= 2)
                    {
                        artist = parts[0];
                        track = parts[1];
                    }
                }

                public static bool IsSpotifyRunning()
                {
                    Process[] pArr = Process.GetProcessesByName("spotify");
                    if (pArr.Length < 1)
                    {
                        return false;
                    }
                    return true;
                }

                private void EnsureSpotifyIsRunning()
                {
                    if (!IsSpotifyRunning())
                    {
                        throw new Exception("Spotify is not running");
                    }
                }

                public SpotifyTrack GetTrack()
                {
                    if (HasWindowHandle())
                    {
                        string strTitle = GetWindowTitle();
                        string strArtist = string.Empty;
                        string strTrack = string.Empty;
                        ParseWindowTitle(strTitle, ref strArtist, ref strTrack);
                        return new SpotifyTrack() { Artist = strArtist, Track = strTrack };
                    }
                    return null;
                }

                private void ExecuteListener()
                {
                    string strTitle = GetWindowTitle();
                    if (strTitle.CompareTo(_strWindowTitle) != 0)
                    {
                        _strWindowTitle = strTitle;

                        string strArtist = string.Empty;
                        string strTrack = string.Empty;
                        ParseWindowTitle(strTitle, ref strArtist, ref strTrack);
                        if (this.OnSpotifyTrackChanged != null)
                        {
                            this.OnSpotifyTrackChanged(this, new SpotifyTrackChangedEventArgs(strArtist, strTrack));
                        }
                    }
            
                }

                private void ListenSpotifyChanges()
                {
                    while (true)
                    {
                        if (HasWindowHandle())
                        {
                            ExecuteListener();
                        }
                        Thread.Sleep(500);
                    }
                }

                private bool HasWindowHandle()
                {
                    return (_hWindow != 0);
                }

                private string GetWindowTitle()
                {
                    _hWindow = GetSpotifyWindowHandle();
                    if (!HasWindowHandle())
                    {
                        return "";
                    }

                    StringBuilder sb = new StringBuilder(1024);
                    SpotifyNative.GetWindowText(_hWindow, sb, sb.Capacity);

                    return sb.ToString();
                }

                private int GetSpotifyWindowHandle()
                {
                    return SpotifyNative.FindWindow("SpotifyMainWindow", null);
                }
            #endregion
        }

    }
}
