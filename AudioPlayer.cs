using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RIKA_IMBANIKA_LIFE_HELPER
{
    public class AudioPlayer
    {
        private readonly MediaElement _player = new MediaElement();
        private List<string> _playlist = new List<string>();
        private List<int> _probs = new List<int>();
        private int _currentTrackIndex = -1;
        private readonly Random _random = new Random();
        private string _folderPath;
        private DesktopTextWindow _dtw;
        public MainWindow _MW;

        public AudioPlayer()
        {
            _folderPath = $"{S.PF}Audios";
            _player.LoadedBehavior = MediaState.Manual;
            _player.UnloadedBehavior = MediaState.Manual;

            _player.MediaEnded += (s, e) => PlayNext();
            InitializePlaylist();
        }

        private void InitializePlaylist()
        {
            if (!Directory.Exists(_folderPath)) return;

            _playlist = Directory.GetFiles(_folderPath, "*.mp3").ToList();
            Improove();
            ShufflePlaylist();
        }

        private void Improove()
        {
            _probs = new List<int>();
            for (int i = 0; i < _playlist.Count; i++)
                _probs.Add(GetLastNumberInBrackets(_playlist[i]));

            for (int i = 0; i < _probs.Count; i++)
            {
                for (int j = 0; j < _probs[i] - 1; j++)
                {
                    _playlist.Add(_playlist[i]);
                }
            }
        }

        public int GetLastNumberInBrackets(string input)
        {
            if (string.IsNullOrEmpty(input)) return 10;

            int lastOpenBracket = input.LastIndexOf('[');
            int lastCloseBracket = input.LastIndexOf(']');

            if (lastOpenBracket == -1 || lastCloseBracket == -1 || lastCloseBracket <= lastOpenBracket)
                return 10;

            string numberStr = input.Substring(lastOpenBracket + 1, lastCloseBracket - lastOpenBracket - 1);

            if (int.TryParse(numberStr, out int result))
                return result;

            return 10;
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

            //Next 10
            for (int i = 0; i < _playlist.Count; i++)
            {
                for (int j = i + 1; j < Math.Min(i + 11, _playlist.Count); j++)
                {
                    if (Equals(_playlist[i], _playlist[j]))
                    {
                        _playlist.RemoveAt(j);
                        j--;                           
                    }
                }
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
            str = str.Remove(str.LastIndexOf(" ["));
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
