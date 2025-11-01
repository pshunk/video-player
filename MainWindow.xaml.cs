using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using LibVLCSharp.WPF;
using Microsoft.Win32;
using Nikse.SubtitleEdit.Core.Common;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using video_player_wpf.Models;
using Path = System.IO.Path;

namespace video_player_wpf
{
    public partial class MainWindow : Window
    {
        LibVLC libVLC;
        VideoPlayer vp;
        bool isFullscreen = false;
        int videoHeight = 1;
        int videoWidth = 1;
        string file = "";
        int currentSubtitleId = -1;
        int? externalSubtitleId = null;
        bool isFirstPlay = true;
        SubtitleCollection subtitles = new();
        bool subtitlesEnabled = false;
        Dictionary dictionary;

        public MainWindow()
        {
            InitializeComponent();
            DeserializeXml();
            string path = "Resources/ffmpeg";
            string binaryPath = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), path);
            GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = "./Resources/ffmpeg" });
            this.KeyDown += MainWindow_KeyDown;
            this.SizeChanged += MainWindow_SizeChanged;
            libVLC = new LibVLC();
            vp = new VideoPlayer(libVLC);
            vp.Playing += Vp_Playing;
            vp.Paused += Vp_Paused;
            vp.Stopped += Vp_Stopped;
            vp.TimeChanged += Vp_TimeChanged;
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = vp;
        }

        private void Vp_Playing(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPauseIcon.Source = new BitmapImage(new Uri(@"/icons/pause_icon.png", UriKind.Relative));
                PlayPauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                VideoTrackBar.IsEnabled = true;
                TimerLabel.IsEnabled = true;
                MuteButton.IsEnabled = true;
                if (isFirstPlay)
                {
                    VolumeTrackBar.IsEnabled = true;
                    UpdateTimer();
                    UpdateTrackLabels();
                    PopulateMenus();
                    ChooseSubtitle();
                    isFirstPlay = false;
                }
            });
            if (videoHeight == 1 || videoWidth == 1)
            {
                Media media = vp.Media;
                videoHeight = (int)media.Tracks.Where(track => track.Id == vp.VideoTrack).ElementAt(0).Data.Video.Height;
                videoWidth = (int)media.Tracks.Where(track => track.Id == vp.VideoTrack).ElementAt(0).Data.Video.Width;
                media.Dispose();
                ResizeVideoView();
            }
        }

        private void Vp_Paused(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPauseIcon.Source = new BitmapImage(new Uri(@"/icons/play_icon.png", UriKind.Relative));
            });
        }

        private void Vp_Stopped(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPauseIcon.Source = new BitmapImage(new Uri(@"/icons/play_icon.png", UriKind.Relative));
                PlayPauseButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                VideoTrackBar.IsEnabled = false;
                VideoTrackBar.Value = 0;
                TimerLabel.IsEnabled = false;
                TimerLabel.Content = "0:00/0:00";
                VolumeTrackBar.IsEnabled = false;
                VolumeTrackBar.Value = 100;
                MuteButton.IsEnabled = false;
                subtitlesEnabled = false;
                UpdateTrackLabels();
                PopulateMenus();
                DisplaySubtitles();
                videoHeight = 1;
                videoWidth = 1;
                subtitles.SubtitleDelay = 0;
                SubtitleDelayLabel.Content = "0";
            });
        }

        private void Vp_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                UpdateTimer();
                if (subtitles.chunks.Count > 0 && subtitlesEnabled)
                {
                    subtitles.UpdateSub(e.Time);
                    DisplaySubtitles();
                }
            });
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeVideoView();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && isFullscreen)
            {
                LeaveFullscreen();
            }

            if (e.Key == Key.F)
            {
                if (!isFullscreen)
                {
                    GoFullscreen();
                }
                else
                {
                    LeaveFullscreen();
                }
            }

            if (vp.IsPlayingMedia)
            {
                if (e.Key == Key.Space)
                {
                    vp.PlayPause();
                }
                if (e.Key == Key.Left)
                {
                    vp.Time = subtitles.PreviousSub(vp.Time);
                    DisplaySubtitles();
                    UpdateTimer();
                }
                if (e.Key == Key.Right)
                {
                    vp.Time = subtitles.NextSub();
                    DisplaySubtitles();
                    UpdateTimer();
                }
                if (e.Key == Key.J)
                {
                    vp.Seek(-1);
                }
                if (e.Key == Key.K)
                {
                    vp.Seek(1);
                }
                if (e.Key == Key.F1)
                {
                    subtitles.DecreaseSubtitleDelay();
                    SubtitleDelayLabel.Content = subtitles.SubtitleDelay;
                }
                if (e.Key == Key.F2)
                {
                    subtitles.IncreaseSubtitleDelay();
                    SubtitleDelayLabel.Content = subtitles.SubtitleDelay;
                }
            }
        }

        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (vp.IsPlayingMedia)
            {
                vp.PlayPause();
            }
        }

        void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (vp.IsPlayingMedia)
            {
                vp.Stop();
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            MuteUnmute();
        }

        private void VideoThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            VideoTrackBar.Value += e.HorizontalChange;
            vp.Time = (long)(VideoTrackBar.Value * vp.Length / 1000);
        }

        private void VideoRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            VideoTrackBar.Value = VideoTrackBar.ValueFromPoint(Mouse.GetPosition(VideoTrackBar));
            vp.Time = (long)(VideoTrackBar.Value * vp.Length / 1000);
        }

        private void VolumeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            VolumeTrackBar.Value += e.HorizontalChange;
            vp.Volume = (int)VolumeTrackBar.Value;
        }

        private void VolumeRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeTrackBar.Value = VolumeTrackBar.ValueFromPoint(Mouse.GetPosition(VolumeTrackBar));
            vp.Volume = (int)VolumeTrackBar.Value;
        }

        private void Word_MouseEnter(object sender, MouseEventArgs e)
        {
            Inline source = (Inline)e.Source;
            string s = source.ContentStart.GetTextInRun(LogicalDirection.Forward);
            if (s.ContainsLetter())
            {
                source.Background = Brushes.LightSalmon;
                Word word = (Word)source.Tag;
                ShowDefinition(word);
                DefinitionPopup.IsOpen = true;
            }
        }

        private void Word_MouseLeave(object sender, MouseEventArgs e)
        {
            (e.Source as Inline).Background = null;
            Definition.Inlines.Clear();
            DefinitionPopup.IsOpen = false;
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            Nullable<bool> result = ofd.ShowDialog();
            if (result.Value)
            {
                isFirstPlay = true;
                file = ofd.FileName;
                vp.PlayFile(file);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FullscreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GoFullscreen();
        }

        private void GoFullscreen()
        {
            MenuBar.Visibility = Visibility.Collapsed;
            VideoTrackBar.Visibility = Visibility.Collapsed;
            ControlBar.Visibility = Visibility.Collapsed;
            Grid.SetRow(VideoViewBackground, 0);
            Grid.SetRowSpan(VideoViewBackground, 4);
            this.WindowStyle = WindowStyle.None;
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.WindowState = WindowState.Maximized;
            isFullscreen = true;
        }

        private void LeaveFullscreen()
        {
            MenuBar.Visibility = Visibility.Visible;
            VideoTrackBar.Visibility = Visibility.Visible;
            ControlBar.Visibility = Visibility.Visible;
            Grid.SetRow(VideoViewBackground, 1);
            Grid.SetRowSpan(VideoViewBackground, 1);
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Maximized;
            isFullscreen = false;
            ResizeVideoView();
        }

        private void ResizeVideoView()
        {
            Dispatcher.Invoke(() =>
            {
                double videoRatio = (double)videoWidth / (double)videoHeight;
                double backgroundRatio = VideoViewBackground.ActualWidth / VideoViewBackground.ActualHeight;
                if (videoRatio > backgroundRatio)
                {
                    VideoView.Width = VideoViewBackground.ActualWidth;
                    VideoView.Height = VideoView.Width * videoHeight / videoWidth;
                }
                else
                {
                    VideoView.Height = VideoViewBackground.ActualHeight;
                    VideoView.Width = VideoView.Height * videoWidth / videoHeight;
                }
            });
        }

        private void MuteUnmute()
        {
            if (vp.IsPlayingMedia)
            {
                vp.MuteUnmute();
                if (vp.IsMuted)
                {
                    MuteIcon.Source = new BitmapImage(new Uri(@"/icons/volume_mute_fill_icon.png", UriKind.Relative));
                    VolumeTrackBar.IsEnabled = false;
                }
                else
                {
                    MuteIcon.Source = new BitmapImage(new Uri(@"/icons/volume_up_fill_icon.png", UriKind.Relative));
                    VolumeTrackBar.IsEnabled = true;
                }
            }
        }

        private void UpdateTimer()
        {
            try
            {
                VideoTrackBar.Value = Convert.ToInt32(vp.Time * 1000 / vp.Length);
                TimerLabel.Content = $"{FormatTime(vp.Time)}/{FormatTime(vp.Length)}";
            }
            catch (DivideByZeroException)
            {
                TimerLabel.Content = "0:00/0:00";
            }
        }

        private static String FormatTime(long time)
        {
            TimeSpan ms = TimeSpan.FromMilliseconds(time);
            if (ms.Hours > 0)
            {
                return $"{ms.Hours}:{ms.Minutes:D2}:{ms.Seconds:D2}";
            }
            else
            {
                return $"{ms.Minutes}:{ms.Seconds:D2}";
            }
        }

        private void UpdateTrackLabels()
        {
            try
            {
                AudioTrackLabel.Content = vp.CurrentAudioTrack.Name;
                SubtitleTrackLabel.Content = vp.CurrentSubtitleTrack.Name;
                SubtitleDelayLabel.Content = subtitles.SubtitleDelay;
            }
            catch
            {
                AudioTrackLabel.Content = "";
                SubtitleTrackLabel.Content = "";
                SubtitleDelayLabel.Content = "";
            }
        }

        private void PopulateMenus()
        {
            PopulateAudioTracks();
            PopulateSubtitleTracks();
        }

        private void PopulateAudioTracks()
        {
            AudioTrackMenuItem.Items.Clear();
            if (!vp.IsPlayingMedia)
            {
                AudioTrackMenuItem.IsEnabled = false;
            }
            else
            {
                TrackDescription[] audioTracks = vp.AudioTracks;
                if (audioTracks.Length > 0)
                {
                    AudioTrackMenuItem.IsEnabled = true;
                    MenuItem[] audioTrackMenuItems = new MenuItem[audioTracks.Length];
                    for (int i = 0; i < audioTracks.Length; i++)
                    {
                        MenuItem audioTrackMenuItem = new();
                        audioTrackMenuItem.Uid = $"{audioTracks[i].Id}";
                        audioTrackMenuItem.Header = $"{audioTracks[i].Name}";
                        audioTrackMenuItem.Click += AudioTrackDropDownMenuItem_Click;
                        if (vp.CurrentAudioTrack.Id == audioTracks[i].Id)
                        {
                            audioTrackMenuItem.IsChecked = true;
                        }
                        audioTrackMenuItems[i] = audioTrackMenuItem;
                    }
                    foreach (MenuItem item in audioTrackMenuItems)
                    {
                        AudioTrackMenuItem.Items.Add(item);
                    }
                }
            }
        }

        private void PopulateSubtitleTracks()
        {
            SubtitleTrackMenuItem.Items.Clear();
            if (!vp.IsPlayingMedia)
            {
                SubtitleTrackMenuItem.IsEnabled = false;
            }
            else
            {
                TrackDescription[] subtitleTracks = vp.SubtitleTracks;
                if (subtitleTracks.Length > 0)
                {
                    SubtitleTrackMenuItem.IsEnabled = true;
                    MenuItem[] subtitleTrackMenuItems = new MenuItem[subtitleTracks.Length];
                    for (int i = 0; i < subtitleTracks.Length; i++)
                    {
                        MenuItem subtitleTrackMenuItem = new();
                        subtitleTrackMenuItem.Uid = $"{subtitleTracks[i].Id}";
                        subtitleTrackMenuItem.Header = $"{subtitleTracks[i].Name}";
                        subtitleTrackMenuItem.Click += SubtitleTrackDropDownMenuItem_Click;
                        if (vp.CurrentSubtitleTrack.Id == subtitleTracks[i].Id)
                        {
                            subtitleTrackMenuItem.IsChecked = true;
                        }
                        subtitleTrackMenuItems[i] = subtitleTrackMenuItem;
                    }
                    foreach (MenuItem item in subtitleTrackMenuItems)
                    {
                        SubtitleTrackMenuItem.Items.Add(item);
                    }
                }
            }
        }

        private void AudioTrackDropDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in AudioTrackMenuItem.Items)
            {
                if (item.Uid != (e.Source as MenuItem).Uid)
                {
                    item.IsChecked = false;
                }
                else if (item.IsChecked == false)
                {
                    item.IsChecked = true;
                    vp.SetAudioTrack(Convert.ToInt32((e.Source as MenuItem).Uid));
                    UpdateTrackLabels();
                }
            }
        }

        private void SubtitleTrackDropDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in SubtitleTrackMenuItem.Items)
            {
                if (item.Uid != (e.Source as MenuItem).Uid)
                {
                    item.IsChecked = false;
                }
                else if (item.IsChecked == false)
                {
                    item.IsChecked = true;
                    vp.SetSpu(Convert.ToInt32((e.Source as MenuItem).Uid));
                    UpdateTrackLabels();
                    ChooseSubtitle();
                }
            }
        }

        private void ChooseSubtitle()
        {
            if (externalSubtitleId == null)
            {
                if (File.Exists(Path.ChangeExtension(file, "srt")) || File.Exists(Path.ChangeExtension(file, "ass")))
                {
                    externalSubtitleId = vp.CurrentSubtitleTrack.Id;
                }
            }
            if (vp.CurrentSubtitleTrack.Id == externalSubtitleId)
            {
                if (File.Exists(Path.ChangeExtension(file, "srt")))
                {
                    LoadSubtitles(Subtitle.Parse(Path.ChangeExtension(file, "srt")));
                }
                else if (File.Exists(Path.ChangeExtension(file, "ass")))
                {
                    LoadSubtitles(Subtitle.Parse(Path.ChangeExtension(file, "ass")));
                }
            }
            else
            {
                if (vp.CurrentSubtitleTrack.Id != -1)
                {
                    var mediaInfo = FFProbe.Analyse(file);
                    string fileType = mediaInfo.PrimarySubtitleStream.CodecName;
                    using (MemoryStream stream = new())
                    {
                        FFMpegArguments.FromFileInput(file).OutputToPipe(new StreamPipeSink(stream), options => options.DisableChannel(Channel.Audio).DisableChannel(Channel.Video).ForceFormat(fileType)).ProcessSynchronously();
                        StreamReader reader = new(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        LoadSubtitles(Subtitle.Parse(stream, fileType));
                    }
                }
                else
                {
                    subtitlesEnabled = false;
                }
            }
        }

        private void LoadSubtitles(Subtitle subtitle)
        {
            subtitle.Sort(Nikse.SubtitleEdit.Core.Enums.SubtitleSortCriteria.StartTime);
            subtitles.LoadSubtitle(subtitle);
            vp.SetSpu(-1);
            subtitlesEnabled = true;
        }

        private void DisplaySubtitles()
        {
            if (!subtitlesEnabled || !subtitles.DisplaySubs)
            {
                SubtitleDisplay.Inlines.Clear();
            }
            else if (subtitles.currentSubtitleId != currentSubtitleId)
            {
                SubtitleDisplay.Inlines.Clear();
                var text = subtitles.CurrentChunk.text;
                foreach (Word word in text)
                {
                    if (Kawazu.Utilities.HasKanji(word.Surface))
                    {
                        TextBlock t = new();
                        Run furigana = new(word.Furigana);
                        furigana.FontSize = 18;
                        t.TextAlignment = TextAlignment.Center;
                        t.Inlines.Add(furigana);
                        t.Inlines.Add(new LineBreak());
                        t.Inlines.Add(word.Surface);
                        SubtitleDisplay.Inlines.Add(t);
                        t.Inlines.LastInline.Tag = word;
                        t.Inlines.Last().MouseEnter += Word_MouseEnter;
                        t.Inlines.Last().MouseLeave += Word_MouseLeave;
                    }
                    else
                    {
                        SubtitleDisplay.Inlines.Add(word.Surface);
                        SubtitleDisplay.Inlines.LastInline.Tag = word;
                        SubtitleDisplay.Inlines.Last().MouseEnter += Word_MouseEnter;
                        SubtitleDisplay.Inlines.Last().MouseLeave += Word_MouseLeave;
                    }
                }
                currentSubtitleId = subtitles.CurrentChunk.id;
            }
        }

        private void DeserializeXml()
        {
            var serializer = new XmlSerializer(typeof(Dictionary));
            using (StreamReader reader = new("./Resources/Dictionary/JMdict_e.xml"))
            {
                dictionary = (Dictionary)serializer.Deserialize(reader);
            }
        }

        private void ShowDefinition(Word word)
        {
            Definition.Inlines.Add(new Bold(new Run(word.OriginalForm)));
            Definition.Inlines.Add(new LineBreak());
            Definition.Inlines.Add(new LineBreak());
            JMdictEntrySense[] definitions;
            try
            {
                definitions = dictionary.Entries.Where(entry => entry.Word.Equals(word.OriginalForm)).First().Senses;
            }
            catch
            {
                definitions = [];
            }
            if (definitions.Any())
            {
                for (int i = 0; i < definitions.Length; i++)
                {
                    Definition.Inlines.Add("● ");
                    for (int j = 0; j < definitions[i].Definitions.Length; j++)
                    {
                        Definition.Inlines.Add(definitions[i].Definitions[j].Definition);
                        if (j != definitions[i].Definitions.Length - 1)
                        {
                            Definition.Inlines.Add("; ");
                        }
                    }
                    if (i != definitions.Length - 1)
                    {
                        Definition.Inlines.Add(new LineBreak());
                    }
                }
            }
            else
            {
                Definition.Inlines.Add("Definition could not be found.");
            }
        }
    }
}