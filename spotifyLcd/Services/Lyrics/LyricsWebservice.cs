using spotifyLcd.Services.Webservices;
using System.Linq;

namespace spotifyLcd.Services.Lyrics
{
    public class LyricsWebservice
    {
        private const string rootUrl = "http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect";

        public string GetLyrics(string artist, string track)
        {
            var url = string.Format("{0}?artist={1}&song={2}", rootUrl, System.Uri.EscapeDataString(artist), System.Uri.EscapeDataString(track));

            var webservice = new Webservice();
            var rootNode = webservice.GetXml(url);

            if (rootNode == null) return string.Empty;

            var LyricNode = rootNode.Elements().FirstOrDefault(t => t.Name.LocalName.Equals("Lyric"));
            return LyricNode != null ? LyricNode.Value : string.Empty;



        }
    }
}
