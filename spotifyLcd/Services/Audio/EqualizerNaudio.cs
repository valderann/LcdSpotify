using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace spotifyLcd.Services.Audio
{
    public class EqualizerNaudio : IEqualizer
    {
        private IWaveIn waveIn;
        private static int fftLength = 8192; // NAudio fft wants powers of two!
        private SampleAggregator aggregator;
        private List<byte> spectrumBuffer = new List<byte>();

        //Todo: implement equalizer with Naudio
        public void Start()
        {
            aggregator = new SampleAggregator();
            aggregator.NotificationCount = 882;
            aggregator.PerformFFT = true;
            aggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);

            waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveIn.Dispose();
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            int bufferIncrement = waveIn.WaveFormat.BlockAlign;

            for (int index = 0; index < bytesRecorded; index += bufferIncrement)
            {
                float sample32 = BitConverter.ToSingle(buffer, index);
                aggregator.Add(sample32);
            }


        }
        void FftCalculated(object sender, FftEventArgs e)
        {
            var tempBuffer = new List<Byte>();

            var lines = 18;
            var b0 = 0;
            var y = 0;
            for (var x = 0; x < lines; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < e.Result[1 + b0].X) peak = e.Result[1 + b0].X;
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                tempBuffer.Add((byte)y);
            }

            spectrumBuffer = tempBuffer;
        }

        public List<byte> GetSpectrumData()
        {
            return spectrumBuffer;
        }

        public void Free()
        {
            waveIn.Dispose();
        }
    }
}
