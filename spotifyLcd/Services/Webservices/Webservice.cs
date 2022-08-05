using System.IO;
using System.Net;
using System.Xml.Linq;

namespace spotifyLcd.Services.Webservices
{
    public class Webservice
    {
        public XElement GetXml(string url)
        {
            var objRequest = WebRequest.Create(url);
            try
            {
                var objResponse = objRequest.GetResponse();

                using (var sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    return XElement.Load(sr);
                }
            }
            catch (WebException ex)
            {
                return null;
            }

        }
    }
}
