using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace VkMusic
{
    public class Track : ObservableObject
    {
        public Int64 id { get; set; }
        public Int64 id_user { get; set; }
        public String title { get; set; }
        public String autor { get; set; }
        public int seconds { get; set; }

        public Track() { }
        public Track(Int64 id, Int64 id_user, String title, String autor, int seconds, bool isPlaying) 
        {
            this.id = id;
            this.id_user = id_user;
            this.title = title;
            this.autor = autor;
            this.seconds = seconds;
            IsPlaying = isPlaying;
        }
        public Track(Track tr,bool isPlaying)
        {
            this.id = tr.id;
            this.id_user = tr.id_user;
            this.title = tr.title;
            this.autor = tr.autor;
            this.seconds = tr.seconds;
            IsPlaying = isPlaying;
        }
        bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying;
            set => isPlaying = value;
        }
        public double getDurationInt
        {
            get
            {
                return (double)seconds;
            }
        }
        public String getDuration
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                return time.ToString(@"mm\:ss");
            }
        }
        public String getTitle 
        {
            get 
            {
                return HttpUtility.HtmlDecode(title);
            }
        }
        public String getAutor
        {
            get
            {
                return HttpUtility.HtmlDecode(autor);
            }
        }

        private ICommand _playCommand;
        private ICommand _playCommandCurrent;

        public ICommand PlayCommand
        {
            get
            {
                if (_playCommand == null)
                {
                    _playCommand = new RelayCommand(
                        param => Play()
                    );
                }
                return _playCommand;
            }
        }
        public ICommand PlayCommandCurrent
        {
            get
            {
                if (_playCommandCurrent == null)
                {
                    _playCommandCurrent = new RelayCommand(
                        param => PlayCurrent()
                    );
                }
                return _playCommandCurrent;
            }
        }
        private async void Play()
        {
            var _tracks = ((MainWindow)Application.Current.MainWindow)._tracks;
            var thisTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.getTitle == getTitle);
            if (IsPlaying)
            {
                _tracks[thisTrack.index] = new Track(thisTrack.track, false);
                ((MainWindow)Application.Current.MainWindow).CurrentSelectedTrack = new Track(thisTrack.track, false);
            }
            else
            {
                var playingTrack = _tracks.Select((t, i) => new { index = i, track = t }).SingleOrDefault(t => t.track.IsPlaying);
                if (playingTrack != null)
                {
                    _tracks[playingTrack.index] = new Track(playingTrack.track, false);
                }
                _tracks[thisTrack.index] = new Track(thisTrack.track, true);
                ((MainWindow)Application.Current.MainWindow).CurrentSelectedTrack = new Track(thisTrack.track, true);
            }

            await ((MainWindow)System.Windows.Application.Current.MainWindow).getWeb().play(string.Format("{0}_{1}", id_user, id));
        }

        private async void PlayCurrent()
        {
            var _tracks = ((MainWindow)Application.Current.MainWindow)._tracks;
            var _currentTrack = ((MainWindow)Application.Current.MainWindow).CurrentSelectedTrack;
            var thisTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.getTitle == _currentTrack.getTitle);
            if (thisTrack.track.IsPlaying)
            {
                _tracks[thisTrack.index] = new Track(thisTrack.track, false);
                ((MainWindow)Application.Current.MainWindow).CurrentSelectedTrack = new Track(thisTrack.track, false);
            }
            else
            {
                var playingTrack = _tracks.Select((t, i) => new { index = i, track = t }).SingleOrDefault(t => t.track.IsPlaying);
                if (playingTrack != null)
                {
                    _tracks[playingTrack.index] = new Track(playingTrack.track, false);
                }
                _tracks[thisTrack.index] = new Track(thisTrack.track, true);
                ((MainWindow)Application.Current.MainWindow).CurrentSelectedTrack = new Track(thisTrack.track, true);
            }

            await ((MainWindow)System.Windows.Application.Current.MainWindow).getWeb().play(string.Format("{0}_{1}", _currentTrack.id_user,_currentTrack.id));
        }

        public string getStringU() 
        {
            return IsPlaying != true ? "Images/play.png" : "Images/pause.png";
        }

        public string MyImagePath
        {
            get 
            {
                return new Uri(getStringU(), UriKind.Relative).ToString(); 
            }
        }
    }
}
