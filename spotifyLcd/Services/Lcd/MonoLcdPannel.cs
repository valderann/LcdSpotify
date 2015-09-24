using spotifyLcd.Services.Lcd;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd
{
    public class MonoLcdPannel:ILcdPannel,IDisposable
    {
        public int Width{get {return LogitechGSDK.LOGI_LCD_MONO_WIDTH; }}
        public int Height{get {return LogitechGSDK.LOGI_LCD_MONO_HEIGHT;}}

        public MonoLcdPannel(string appName)
        {
            LogitechGSDK.LogiLcdInit(appName, LogitechGSDK.LOGI_LCD_TYPE_MONO);
        }

        public void UpdatePannel(Bitmap bmp)
        {
            var resultData = BitmapToByteArray(bmp);
            LogitechGSDK.LogiLcdMonoSetBackground(resultData);
            LogitechGSDK.LogiLcdUpdate();
        }

        public void ClosePannel()
        {
            LogitechGSDK.LogiLcdShutdown();
        }

        /// <summary>
        /// Convert bitmap to monochrome byte array that can be handled by the Logitech API
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static Byte[] BitmapToByteArray(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new Exception("Bitmap PixelFormat has to be 32bppArgb");

            if (bitmap.Width != LogitechGSDK.LOGI_LCD_MONO_WIDTH || bitmap.Height != LogitechGSDK.LOGI_LCD_MONO_HEIGHT)
                throw new Exception("Bitmap size does not match the mono LCD size");

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int srcBytes = Math.Abs(data.Stride) * bitmap.Height;
            Byte[] rgbData = new Byte[srcBytes];
            Marshal.Copy(data.Scan0, rgbData, 0, srcBytes);
            Byte[] resultData = new Byte[bitmap.Width * bitmap.Height];
            for (int y = 0, ypos = 0; y < bitmap.Height; ++y, ypos += Math.Abs(data.Stride))
            {
                for (int x = 0, pos = ypos; x < bitmap.Width; ++x, pos += 4)
                {
                    byte b = rgbData[pos + 0];
                    byte g = rgbData[pos + 1];
                    byte r = rgbData[pos + 2];
                    resultData[y * bitmap.Width + x] = (byte)((0.2125 * r) + (0.7154 * g) + (0.0721 * b));
                }
            }
            bitmap.UnlockBits(data);
            return resultData;
        }

        #region IDisposable Implementation
            bool disposed = false;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    ClosePannel();
                }
                disposed = true;
            }
        #endregion
    }

}
