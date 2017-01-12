using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd.Services.Lcd
{
    public class BackgroundLed
    {
        public BackgroundLed()
        {
           LogitechGSDK.LogiLedInit();
           LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);
        }

        public void SetLight(int r,int g,int b)
        {
             LogitechGSDK.LogiLedSetLighting(r,g,b);
           
        }

    }
}
