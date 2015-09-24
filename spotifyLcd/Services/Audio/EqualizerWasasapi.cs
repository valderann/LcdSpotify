using spotifyLcd.Services.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace spotifyLcd
{

    public class AudioAnalyzer : IEqualizer
    {
        private float[] _fft;               //buffer for fft data
        private WASAPIPROC _process;        //callback function to obtain data
        private bool _initialized;          //initialized flag
        private int devindex;               //used device index

        private int _lines = 18;            // number of spectrum lines

        public AudioAnalyzer()
        {
            _fft = new float[1024];
            _process = new WASAPIPROC(Process);
            _initialized = false;
            Init();
        }

        // flag for display enable
        public bool DisplayEnable { get; set; }


        public void Start()
        { 
                if (!_initialized)
                {
                     var intDefaultDevice = 0;
                     for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
                    {
                                    var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                                    if (device.IsEnabled && device.IsLoopback)
                                    {
                                        intDefaultDevice = i;
                                        break;
                                    }
                    }
                   devindex = intDefaultDevice;
                   bool result = BassWasapi.BASS_WASAPI_Init(devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                   if (!result)
                   {
                       var error = Bass.BASS_ErrorGetCode();
                   }
                   else
                   {
                        _initialized = true;
                   }
                }
                BassWasapi.BASS_WASAPI_Start();
        }

        public void Stop()
        {
                BassWasapi.BASS_WASAPI_Stop(true);
                System.Threading.Thread.Sleep(500);
        }

        // initialization
        private void Init()
        {
            bool result = false;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }

        public List<byte> GetSpectrumData()
        {
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048); //get channel fft data
            int x, y;
            int b0 = 0;

            var spectrumdata = new List<byte>();

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x = 0; x < _lines; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (_lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                spectrumdata.Add((byte)y);
            }
            return spectrumdata;
        }

        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }
    }
}
