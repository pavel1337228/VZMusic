using CefSharp;
using CefSharp.DevTools;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VkMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ListViewTracks.ItemsSource = _tracks;
            _tracks.CollectionChanged += Tracks_CollectionChanged;
        }


        private ChromiumWebBrowser _web;
        public ObservableCollection<Track> _tracks = new ObservableCollection<Track>();
        public Track currentSelectedTrack = new Track();
        public Track CurrentSelectedTrack
        {
            get
            {
                return currentSelectedTrack;
            }
            set 
            {
                CurrentTrackTitle.Text = HttpUtility.HtmlDecode(value.title);
                CurrentTrackAutor.Text = HttpUtility.HtmlDecode(value.autor);
                CurrentTrackMyImagePath.Source = new BitmapImage(new Uri(value.getStringU(), UriKind.Relative));
                currentSelectedTrack = value;
            }
        }
        async void Tracks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: // если добавление
                    if (e.NewItems?[0] is Track newTrack)
                        await _web.writeLine($"Добавлен новый объект: {newTrack.title}");
                    break;
                case NotifyCollectionChangedAction.Remove: // если удаление
                    if (e.OldItems?[0] is Track oldTrack)
                        await _web.writeLine($"Удален объект: {oldTrack.title}");
                    break;
                case NotifyCollectionChangedAction.Replace: // если замена
                    if ((e.NewItems?[0] is Track replacingTrack) &&
                        (e.OldItems?[0] is Track replacedTrack))
                        await _web.writeLine($"Объект {replacedTrack.title} вкл/выкл");
                    break;
            }
        }
        System.Timers.Timer t = new System.Timers.Timer();
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //return;
            CefSettings settings = new CefSettings();
            settings.RootCachePath = System.IO.Path.GetFullPath("cache");
            settings.CachePath = System.IO.Path.GetFullPath("cache\\global");
            settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";
            //settings.PersistSessionCookies = true;
            settings.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 6P Build/XXXXX; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/47.0.2526.68 Mobile Safari/537.36";
            Cef.Initialize(settings);

            _web = new ChromiumWebBrowser("https://m.vk.com/login");
            
            Grid.SetRow(_web, 0);
            vkPanel.Children.Add(_web);
            _web.AddressChanged += _web_AddressChanged;
            _web.ConsoleMessage += OnConsoleMessage;
            _web.FrameLoadEnd += (s, args) =>
            {
                //Wait for the MainFrame to finish loading
                if (args.Frame.IsMain)
                {
                    args.Frame.ExecuteJavaScriptAsync("window.vk.audioAdsConfig = null;");
                    args.Frame.ExecuteJavaScriptAsync("document.querySelector('.AudioBlock_triple_stacked_slider').remove();");
                    args.Frame.ExecuteJavaScriptAsync("audio.setPosition(2);");
                    args.Frame.ExecuteJavaScriptAsync("setInterval(() => { if ((audio.isPlaying())&&(audio.getPosition() + 1 > audio.duration())) { audio.getNextAudio().then(value => console.log('id_'+audio.getCurrent().id+'_end_'+ value.id.toString()))  } else { if (audio.getPosition() != 0) { console.log('position_'+audio.getPosition()) } } } , 350);");
                    if (args.Url == "https://m.vk.com/audio?section=my")
                    {
                        Dispatcher.Invoke(async () =>
                        {
                            //_tracks = new ObservableCollection<Track>();
                            foreach (Track track in await _web.getTracks()) 
                            {
                                _tracks.Add(track);
                            }
                            //Tracks.Add(new Track());
                            //list.Add(tracks[0]);
                            //ListViewTracks.ItemsSource = Tracks;
                            tabs.SelectedIndex = 1;
                            volume.Value = 0.5;

                            countT.Text = string.Format("Треков загружено: {0}", _tracks.Count);

                            t.Elapsed += Timer_Elapsed;
                            t.Interval = 100;
                            t.Start();
                        });
                    }
                }
            };
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(async () =>
                {
                    if (isLooping)
                    {
                        PerelevaikaRGB.Direction += 10;
                        if (PerelevaikaRGB.Direction >= 360)
                            PerelevaikaRGB.Direction = 0;
                        PerelevaikaRGB.Opacity += 5;
                        if (PerelevaikaRGB.Opacity >= 70)
                            PerelevaikaRGB.Opacity = 10;
                        //MessageBox.Show(PerelevaikaRGB.Direction.ToString());
                    }
                });
            }
            catch { }
        }

        private void OnConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (e.Message != null) 
            {
                if (e.Message.ToString().Split('_').Length == 2)
                {
                    if (e.Message.ToString().Split('_')[0] == "position")
                    {
                        double position = double.Parse(e.Message.ToString().Split('_')[1], System.Globalization.CultureInfo.InvariantCulture);
                        Dispatcher.Invoke(async () =>
                        {
                            slider.Maximum = currentSelectedTrack.getDurationInt - 1;
                            if (!dragStarted)
                            {
                                slider.Value = position;
                                TimeSpan time = TimeSpan.FromSeconds(position);
                                curTrackPos.Text = time.ToString(@"mm\:ss");
                                TimeSpan timeE = TimeSpan.FromSeconds(currentSelectedTrack.getDurationInt);
                                curTrackDur.Text = timeE.ToString(@"mm\:ss");
                            }
                            else 
                            {
                                await _web.pause();
                            }
                        });
                        //var nextTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.id.ToString() == next_id);
                        //Track.SetThis(nextTrack.track.id);
                    }
                }
                else if (e.Message.ToString().Split('_').Length == 4)
                {
                    var next_id = e.Message.ToString().Split('_')[3];
                    if (e.Message.ToString().Split('_')[2] == "end")
                    {
                        Dispatcher.Invoke(async () =>
                        {
                            if (!isLooping)
                            {
                                var nextTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.id.ToString() == next_id);
                                if (!nextTrack.track.IsPlaying)
                                {
                                    var playingTrack = _tracks.Select((t, i) => new { index = i, track = t }).SingleOrDefault(t => t.track.IsPlaying);
                                    if (playingTrack != null)
                                    {
                                        _tracks[playingTrack.index] = new Track(playingTrack.track, false);
                                    }
                                    _tracks[nextTrack.index] = new Track(nextTrack.track, true);
                                    CurrentSelectedTrack = new Track(nextTrack.track, true);
                                }
                            }
                            else 
                            {
                                await _web.setPosition(0);
                            }
                        });
                    }
                }
            }
        }

        public ChromiumWebBrowser getWeb()
        {
            return _web;
        }
        private async void HandleKeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key.ToString())
            {
                case "F1":
                    lPanel.Visibility = Visibility.Hidden;
                    tabs.SelectedIndex = 0;
                    break;
                case "F2":
                    tabs.SelectedIndex = 1;
                    break;
                case "F3":
                    _web.ShowDevTools();
                    break;
            }
        }
        private int countload = 0;
        private void _web_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.ToString() == "https://m.vk.com/feed")
            {
                if (countload == 0) { 
                    lPanel.Visibility = Visibility.Visible;
                    countload++;
                }
                _web.Load("https://m.vk.com/audio?section=my");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cef.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _web.ShowDevTools();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private async void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            curVolumePos.Text = ((int)(volume.Value * 100)).ToString()+"%";
            await _web.setVolume(volume.Value);
        }

        private bool dragStarted = false;
        private async void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            await _web.setPosition(slider.Value);
            await _web.pauseStart();
            dragStarted = false;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragStarted = true;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragStarted) 
            {
                var position = slider.Value;
                TimeSpan time = TimeSpan.FromSeconds(position);
                curTrackPos.Text = time.ToString(@"mm\:ss");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                if (currentSelectedTrack == null)
                    return;
                int prevTrackIndex = 0;
                var thisTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.id == currentSelectedTrack.id);
                if (thisTrack.index == 0)
                {
                    prevTrackIndex = _tracks.Count-1;
                }
                else 
                {
                    prevTrackIndex = thisTrack.index - 1;
                }

                var playingTrack = _tracks.Select((t, i) => new { index = i, track = t }).SingleOrDefault(t => t.track.IsPlaying);
                if (playingTrack != null)
                {
                    _tracks[playingTrack.index] = new Track(playingTrack.track, false);
                }
                _tracks[prevTrackIndex] = new Track(_tracks[prevTrackIndex], true);
                CurrentSelectedTrack = new Track(_tracks[prevTrackIndex], true);

                await _web.play(string.Format("{0}_{1}", _tracks[prevTrackIndex].id_user, _tracks[prevTrackIndex].id));
            });
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                if (currentSelectedTrack == null)
                    return;
                int nextTrackIndex = 0;
                var thisTrack = _tracks.Select((t, i) => new { index = i, track = t }).Single(t => t.track.id == currentSelectedTrack.id);
                if (thisTrack.index == _tracks.Count-1)
                {
                    nextTrackIndex = 0;
                }
                else
                {
                    nextTrackIndex = thisTrack.index + 1;
                }

                var playingTrack = _tracks.Select((t, i) => new { index = i, track = t }).SingleOrDefault(t => t.track.IsPlaying);
                if (playingTrack != null)
                {
                    _tracks[playingTrack.index] = new Track(playingTrack.track, false);
                }
                _tracks[nextTrackIndex] = new Track(_tracks[nextTrackIndex], true);
                CurrentSelectedTrack = new Track(_tracks[nextTrackIndex], true);

                await _web.play(string.Format("{0}_{1}", _tracks[nextTrackIndex].id_user, _tracks[nextTrackIndex].id));
            });
        }
        private bool isLooping = false;
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                isLooping = !isLooping;

                PerelevaikaRGB.Opacity = isLooping ? PerelevaikaRGB.Opacity = 50 : PerelevaikaRGB.Opacity = 0f;
            });
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            lPanel.Visibility = Visibility.Visible;
            tabs.SelectedIndex = 0;
            await _web.addMoreAudios();
            await Task.Delay(1500);
            var newList = await _web.getMoreAudios(_tracks.Count);
            
            foreach (Track track in newList)
            {
                _tracks.Add(track);
            }

            countT.Text = string.Format("Треков загружено: {0}", _tracks.Count);
            tabs.SelectedIndex = 1;
            lPanel.Visibility = Visibility.Hidden;
        }
    }
}
