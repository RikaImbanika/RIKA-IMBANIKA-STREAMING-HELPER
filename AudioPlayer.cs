using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RIKA_TIMER
{
    public class AudioPlayer
    {
        private readonly MediaElement _player = new MediaElement();
        private List<string> _playlist = new List<string>();
        private int _currentTrackIndex = -1;
        private readonly Random _random = new Random();
        private string _folderPath = Environment.CurrentDirectory + "\\Audios\\";
        private DesktopTextWindow _dtw;
        public MainWindow _MW;

        public AudioPlayer()
        {
            _player.LoadedBehavior = MediaState.Manual;
            _player.UnloadedBehavior = MediaState.Manual;

            _player.MediaEnded += (s, e) => PlayNext();
            InitializePlaylist();
        }

        private void InitializePlaylist()
        {
            if (!Directory.Exists(_folderPath)) return;

            _playlist = Directory.GetFiles(_folderPath, "*.mp3").ToList();
            ShufflePlaylist();
        }

        private void ShufflePlaylist()
        {
            int n = _playlist.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (_playlist[k], _playlist[n]) = (_playlist[n], _playlist[k]);
            }
        }

        public void PlayNext()
        {
            if (_playlist.Count == 0) return;

            _currentTrackIndex++;
            if (_currentTrackIndex >= _playlist.Count)
            {
                ShufflePlaylist();
                _currentTrackIndex = 0;
            }

            _player.Stop();
            _player.Source = new Uri(_playlist[_currentTrackIndex]);
            _player.Play();

            if (_dtw != null)
            {
                if (_dtw.IsVisible)
                    _dtw.Close();
                _dtw = null;
            }

            string str = $"{Path.GetFileName(_playlist[_currentTrackIndex])}";
            str = str.Remove(str.LastIndexOf("."));
            _dtw = new DesktopTextWindow($"💿 {str}");
            _dtw.Show();

            string wtf = str;

            if (wtf.Contains("-") && wtf.Length > 3)
                wtf = wtf.Substring(wtf.LastIndexOf("-") + 1);
            if (wtf.Contains("‒") && wtf.Length > 3)
                wtf = wtf.Substring(wtf.LastIndexOf("‒") + 1);
            if (wtf.Contains("—") && wtf.Length > 3)
                wtf = wtf.Substring(wtf.LastIndexOf("—") + 1);

            _MW.ChangeAudioName($"💿 {wtf}");
        }

        public void SkipTrack() => PlayNext();
    }
}
