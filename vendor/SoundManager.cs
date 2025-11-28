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
            // Ensure we're on a thread that can use MediaPlayer (usually requires STA, but for simple playback might work or need dispatcher)
            // WinForms apps are usually STA.
            
            StopMusic(); // Stop any currently playing music

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
                        // Loop music
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
                // Clamp volume between 0 and 100
                int clampedVolume = Math.Max(0, Math.Min(100, volume));
                _mediaPlayer.Volume = clampedVolume / 100.0f;
                Console.WriteLine($"Volume set to {clampedVolume}%");
            }
        }
    }
}
