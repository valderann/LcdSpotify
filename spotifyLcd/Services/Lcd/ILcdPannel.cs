using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd.Services.Lcd
{
    public interface ILcdPannel:IDisposable
    {
        int Width { get; }
        int Height { get; }
        void UpdatePannel(Bitmap bmp);
    }
}
