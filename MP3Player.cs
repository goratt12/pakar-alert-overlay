using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pakar_alert_overlay
{
    public class MP3Player
    {
        public static async Task PlayMP3FromResourceAsync(byte[] mp3Data)
        {
            if (mp3Data != null && mp3Data.Length > 0)
            {
                using (var memoryStream = new MemoryStream(mp3Data))
                using (var reader = new Mp3FileReader(memoryStream))
                using (var waveOut = new WaveOutEvent())
                {
                    // Set the playback volume (optional)
                    waveOut.Volume = 1.0f; // You can adjust the volume as needed (0.0 to 1.0).

                    // Set the output device and start asynchronous playback
                    waveOut.Init(reader);
                    waveOut.Play();

                    // Wait for playback to complete asynchronously
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            else
            {
                Console.WriteLine("Resource data is empty.");
            }
        }

        public static async Task SoundAlarm()
        {
            byte[] mp3Data = Properties.Resources.alarm;

            if (mp3Data != null && mp3Data.Length > 0)
            {
                await PlayMP3FromResourceAsync(mp3Data);
            }
            else
            {
                Console.WriteLine("MP3 resource not found or is empty.");
            }
        }
    }
}
