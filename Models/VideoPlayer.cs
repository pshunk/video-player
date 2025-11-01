using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using System.Windows;

namespace video_player_wpf.Models
{
    public class VideoPlayer : MediaPlayer
    {
        private LibVLC libvlc;
        private bool isPlayingMedia = false;
        public bool IsPlayingMedia
        {
            get { return isPlayingMedia; }
        }
        private bool isMuted = false;
        public bool IsMuted
        {
            get { return isMuted; }
        }
        private int oldVolume;
        public TrackDescription[] AudioTracks
        {
            get { return AudioTrackDescription; }
        }
        public TrackDescription CurrentAudioTrack
        {
            get { return AudioTrackDescription.Where(track => track.Id == AudioTrack).ElementAt(0); }
        }
        public TrackDescription[] SubtitleTracks
        {
            get { return SpuDescription; }
        }
        public TrackDescription CurrentSubtitleTrack
        {
            get { return SpuDescription.Where(track => track.Id == Spu).ElementAt(0); }
        }
        public TrackDescription[] VideoTracks
        {
            get { return VideoTrackDescription; }
        }
        public TrackDescription CurrentVideoTrack
        {
            get { return VideoTrackDescription.Where(track => track.Id == VideoTrack).ElementAt(0); }
        }
        private const int seekInterval = 10000;

        public VideoPlayer(LibVLC libvlc) : base(libvlc)
        {
            this.libvlc = libvlc;
            EnableMouseInput = false;
            this.Stopped += (sender, e) =>
            {
                isPlayingMedia = false;
            };
            Volume = 100;
        }

        public void PlayFile(string file)
        {
            Media = new(libvlc, file);

            Media.ParsedChanged += (sender, e) =>
            {
                if (e.ParsedStatus == MediaParsedStatus.Done)
                {
                    if (Media.Duration > 0)
                    {
                        Play();
                        isPlayingMedia = true;
                    }
                    else
                    {
                        MessageBox.Show("Not a valid video file");
                    }
                    Media.Dispose();
                }
            };

            Media.Parse();
        }

        public void PlayPause()
        {
            if (State == VLCState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Seek(int i)
        {
            if (i > 0)
            {
                Time += seekInterval;
            }
            else
            {
                Time -= seekInterval;
            }
        }

        public void MuteUnmute()
        {
            if (!isMuted)
            {
                oldVolume = Volume;
                Volume = 0;
                isMuted = true;
            }
            else
            {
                Volume = oldVolume;
                isMuted = false;
            }
        }
    }
}
