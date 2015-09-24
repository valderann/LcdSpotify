using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd.Services.Spotify
{
    public class SpotifyTrackChangedEventArgs : EventArgs
    {
        #region Private Members
            private string _artist;
            private string _track;
        #endregion

        #region Constructors
            public SpotifyTrackChangedEventArgs(string artist, string track)
            {
                _artist = artist;
                _track = track;
            }
        #endregion

        #region Properties
            public string Artist
            {
                get { return _artist; }
            }

            public string Track
            {
                get { return _track; }
            }
        #endregion
    }
}
