using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd.Services.Lcd
{
    public class SpotifyLcdPannel:IDisposable
    {
        protected ILcdPannel LcdPannel{get;set;}

        public SpotifyLcdPannel()
        {
            LcdPannel = new MonoLcdPannel("Spotify control");
        }

        private string _text { get; set; }
        private int _textPos { get; set; }
        private int textChangeCounter = 0;
        private Bitmap CreateBitmap(string text, List<Byte> spectrum)
        {
            var bmp = new Bitmap(LcdPannel.Width, LcdPannel.Height);
            var font = new Font("Arial", 10, GraphicsUnit.Pixel);
            var brush = Brushes.White;
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var spotifyTextSize=g.MeasureString("Spotify: ",font);
                g.DrawString("Spotify: ", font, brush, 0, 0);

                //Print artist and title to the LCD display
                var toDrawText=text;
                if (!text.Equals(_text))
                {
                    _text = text;
                    _textPos = 0;
                }
                else {
                    var songTitleSize = g.MeasureString(_text, font);
                    
                    //Scroll text if the text is bigger than the screen
                    if (songTitleSize.Width + spotifyTextSize.Width> LcdPannel.Width)
                    {
                        toDrawText = toDrawText.Substring(_textPos, _text.Length - _textPos);
                        if( textChangeCounter==4){
                            _textPos += 2;
                            if (toDrawText.Length < 10)
                            {
                                _textPos = 0;
                                //Wait before scrolling
                                textChangeCounter = -10;
                            }
                            else { textChangeCounter = 0;}
                            
                        }
                        textChangeCounter += 1;
                    }
                }

                g.DrawString(toDrawText, font, brush, spotifyTextSize.Width, 0);

               // Audio visualization
                var xOffset = 0;
                var yOffset = 12;
                foreach (var ln in spectrum)
                {
                    var intBarHeight = (int)(((double)ln / (double)255) * (LcdPannel.Height - yOffset));
                    g.FillRectangle(brush, new Rectangle(xOffset, (LcdPannel.Height) - intBarHeight, 8, intBarHeight));
                    xOffset += 10;
                }
            }
            return bmp;
        }

        public void UpdatePannel(string text, List<Byte> spectrum)
        {
            LcdPannel.UpdatePannel(CreateBitmap(text, spectrum));
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
                    LcdPannel.Dispose();
                }
                disposed = true;
            }
        #endregion

    }
}
