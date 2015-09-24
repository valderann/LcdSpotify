using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifyLcd.Services.Audio
{
    public interface IEqualizer
    {
        void Start();
        void Stop();
        List<byte> GetSpectrumData();
        void Free();
    }
}
