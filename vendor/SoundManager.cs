using System;
using System.IO;
using System.Windows.Media;

namespace skystride.vendor
{
    public static class SoundManager
    {
        private static MediaPlayer _mediaPlayer;

        public static void PlayMusic(string path)
        {
            StopMusic();

            string fullPath = Path.GetFullPath(path);
            Console.WriteLine($"Attempting to play music: {fullPath}");

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Error: Music file not found at {fullPath}");
                return;
            }

            try
            {
                if (_mediaPlayer == null)
                {
                    _mediaPlayer = new MediaPlayer();
                    _mediaPlayer.MediaEnded += (s, e) => 
                    {
                        _mediaPlayer.Position = TimeSpan.Zero;
                        _mediaPlayer.Play();
                    };
                }

                _mediaPlayer.Open(new Uri(fullPath));
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing music: {ex.Message}");
            }
        }

        public static void StopMusic()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close();
            }
        }

        public static void SetVolume(int volume)
        {
            if (_mediaPlayer != null)
            {
                int clampedVolume = Math.Max(0, Math.Min(100, volume));
                _mediaPlayer.Volume = clampedVolume / 100.0f;
                Console.WriteLine($"Volume set to {clampedVolume}%");
            }
        }
    }
}
