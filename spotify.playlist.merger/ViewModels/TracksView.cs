﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.UI;
using spotify.playlist.merger.Data;
using spotify.playlist.merger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace spotify.playlist.merger.ViewModels
{
    public partial class MainPageViewModel : ViewModelBase
    {
        private Views.TracksViewControl _tracksViewControl;
        public Views.TracksViewControl TracksViewControl
        {
            get
            {
                if (_tracksViewControl == null)
                {
                    _tracksViewControl = new Views.TracksViewControl();
                }
                return _tracksViewControl;
            }
        }

        #region Default

        private AdvancedCollectionView _tracksCollectionView;
        public AdvancedCollectionView TracksCollectionView
        {
            get => _tracksCollectionView;
            set
            {
                _tracksCollectionView = value;
                RaisePropertyChanged("TracksCollectionView");
            }
        }

        ObservableCollection<Track> _selectedTracks = new ObservableCollection<Track>();
        public ObservableCollection<Track> SelectedTracks
        {
            get => _selectedTracks;
            set
            {
                _selectedTracks = value;
                RaisePropertyChanged("SelectedTracks");
            }
        }

        private void SelectedTracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasSelectedTracks = (SelectedTracks.Count > 0);
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is MediaItemBase media) media.IsSelected = !media.IsSelected;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is MediaItemBase media) media.IsSelected = !media.IsSelected;
                }
            }
        }

        private bool _isTracksViewBusy;
        public bool IsTracksViewBusy
        {
            get => _isTracksViewBusy;
            set
            {
                _isTracksViewBusy = value;
                RaisePropertyChanged("IsTracksViewBusy");
            }
        }

        private bool _isTracksLoading;
        public bool IsTracksLoading
        {
            get => _isTracksLoading;
            set
            {
                _isTracksLoading = value;
                RaisePropertyChanged("IsTracksLoading");
            }
        }

        private bool _isTracksViewOpen;
        public bool IsTracksViewOpen
        {
            get => _isTracksViewOpen;
            set
            {
                _isTracksViewOpen = value;
                RaisePropertyChanged("IsTracksViewOpen");
                if (!value) ResetTracksView();
                if (value) IsSelectedPlaylistViewOpen = !value;
            }
        }

        private bool _hasSelectedTracks;
        public bool HasSelectedTracks
        {
            get => _hasSelectedTracks;
            set
            {
                _hasSelectedTracks = value;
                RaisePropertyChanged("HasSelectedTracks");
            }
        }

        private Playlist _activePlaylist;
        public Playlist ActivePlaylist
        {
            get => _activePlaylist;
            set
            {
                _activePlaylist = value;
                RaisePropertyChanged("ActivePlaylist");
            }
        }

        private RelayCommand<Track> _tracksViewItemClickCommand;
        public RelayCommand<Track> TracksViewItemClickCommand
        {
            get
            {
                if (_tracksViewItemClickCommand == null)
                {
                    _tracksViewItemClickCommand = new RelayCommand<Track>((item) =>
                    {
                        if (!item.IsSelected)
                            SelectedTracks.Add(item);
                        else
                            SelectedTracks.Remove(item);
                    });
                }
                return _tracksViewItemClickCommand;
            }
        }

        private RelayCommand _playSelectedTracksCommand;
        public RelayCommand PlaySelectedTracksCommand
        {
            get
            {
                if (_playSelectedTracksCommand == null)
                {
                    _playSelectedTracksCommand = new RelayCommand(async () =>
                    {
                        IsTracksViewBusy = true;

                        var ids = _tracksCollectionCopy.Where(c => c.IsSelected).Select(e => e.Id);
                        if (!await DataSource.Current.PlaySpotifyMedia(ids, 0))
                        {
                            ShowNotification(NotificationType.Error, "An error occured while trying to play selected track(s), please check your internet connection");
                        }
                        else
                        {
                            var items = SelectedTracks.ToList();
                            foreach (var item in items)
                            {
                                SelectedTracks.Remove(item);
                            }
                        }

                        IsTracksViewBusy = false;
                    });
                }
                return _playSelectedTracksCommand;
            }
        }

        private RelayCommand _removeSelectedTracksCommand;
        public RelayCommand RemoveSelectedTracksCommand
        {
            get
            {
                if (_removeSelectedTracksCommand == null)
                {
                    _removeSelectedTracksCommand = new RelayCommand(async () =>
                    {

                        var uris = _tracksCollectionCopy.Where(c => c.IsSelected).Select(e => e.Uri);
                        await RemoveTracksFromPlaylist(uris, ActivePlaylist.Id);
                    });
                }
                return _removeSelectedTracksCommand;
            }
        }

        private RelayCommand _clearSelectedTracksCommand;
        public RelayCommand ClearSelectedTracksCommand
        {
            get
            {
                if (_clearSelectedTracksCommand == null)
                {
                    _clearSelectedTracksCommand = new RelayCommand(() =>
                    {
                        SelectedTracks.Clear();
                    });
                }
                return _clearSelectedTracksCommand;
            }
        }

        private RelayCommand<Track> _removeTrackCommand;
        public RelayCommand<Track> RemoveTrackCommand
        {
            get
            {
                if (_removeTrackCommand == null)
                {
                    _removeTrackCommand = new RelayCommand<Track>(async (item) =>
                    {
                        await RemoveTracksFromPlaylist(new List<string> { item.Uri }, ActivePlaylist.Id);
                    });
                }
                return _removeTrackCommand;
            }
        }

        private RelayCommand<Track> _addToQueueCommand;
        public RelayCommand<Track> AddToQueueCommand
        {
            get
            {
                if (_addToQueueCommand == null)
                {
                    _addToQueueCommand = new RelayCommand<Track>(async (item) =>
                    {
                        if (await DataSource.Current.AddToQueue(item.Uri))
                        {
                            ShowNotification(NotificationType.Success, "Track successfuly added to queue");
                        }
                        else
                            ShowNotification(NotificationType.Error, "An error occured, failed to add track to queue");

                    });
                }
                return _addToQueueCommand;
            }
        }

        private void ResetTracksView()
        {
            ActivePlaylist = null;
            if (TracksCollectionView != null) TracksCollectionView.Clear();
            _filteredTracksCollection.Clear();
            _tracksCollectionCopy.Clear();
            SelectedTracks.Clear();
            TrackSearchText = null;
        }

        private async Task LoadTracks(Playlist playlist)
        {
            ResetTracksView();
            ActivePlaylist = playlist;
            IsTracksViewBusy = true;
            IsTracksViewOpen = true;

            IsTracksViewBusy = false;

            IsTracksLoading = true;

            int total = playlist.Count;
            int startIndex = 0;
            List<Track> tracks;
            int index;

            while (startIndex < total)
            {
                tracks = await DataSource.Current.GetTracksPaged(playlist.Id, startIndex);

                if (tracks == null) break;
                startIndex += tracks.Count;

                if (TracksCollectionView == null)
                {
                    TracksCollectionView = new AdvancedCollectionView(tracks, true);
                    index = 1;
                    foreach (var item in tracks)
                    {
                        item.IndexA = index;
                        _tracksCollectionCopy.Add(item);
                        index++;
                    }
                }
                else
                {
                    index = TracksCollectionView.Count;
                    using (TracksCollectionView.DeferRefresh())
                    {
                        foreach (var item in tracks)
                        {
                            item.IndexA = index;
                            TracksCollectionView.Add(item);
                            _tracksCollectionCopy.Add(item);
                            index++;
                        }
                    }
                }
            }
            UpdateTracksIndex();

            IsTracksLoading = false;
        }

        private void UpdateTracksIndex()
        {
            int index = 1;
            using (TracksCollectionView.DeferRefresh())
            {
                foreach (var obj in TracksCollectionView)
                {
                    if (obj is Track item)
                    {
                        item.IndexA = index;
                        index++;
                    }
                }
            }
        }

        private async Task RemoveTracksFromPlaylist(IEnumerable<string> uris, string playlistId)
        {
            IsTracksViewBusy = true;

            //should we show dialog?
            if (await DataSource.Current.RemoveFromPlaylist(playlistId, uris) != null)
            {
                var items = _tracksCollectionCopy.Where(c => c.IsSelected).ToList();
                using (TracksCollectionView.DeferRefresh())
                {
                    foreach (var obj in items)
                    {
                        SelectedTracks.Remove(obj);
                        _filteredTracksCollection.Remove(obj);
                        _tracksCollectionCopy.Remove(obj);
                        TracksCollectionView.Remove(obj);
                    }
                    UpdateTracksIndex();
                }
                if (uris.Count() == 1)
                    ShowNotification(NotificationType.Success, "Track successfuly removed from playlist.");
                else
                    ShowNotification(NotificationType.Success, uris.Count() + " tracks successfuly removed from playlist.");
            }
            else
                ShowNotification(NotificationType.Error, "An error occured, please ensure you have an active internet connection.");

            IsTracksViewBusy = false;
        }

        #endregion

        #region Sorting & Filtering

        private Sorting _selectedTrackSort;
        public Sorting SelectedTrackSort
        {
            get { return _selectedTrackSort; }
            set
            {
                _selectedTrackSort = value;
                RaisePropertyChanged("SelectedTrackSort");
                if (value != null) SortTrackCollection(value);
            }
        }

        readonly List<Track> _tracksCollectionCopy = new List<Track>();
        readonly List<Track> _filteredTracksCollection = new List<Track>();
        public ObservableCollection<Sorting> TracksSortList { get; } = new ObservableCollection<Sorting>(Sorting._tracksSortList);
        public ObservableCollection<Sorting> PlaylistSortList { get; } = new ObservableCollection<Sorting>(Sorting._playlistSortList);

        private string _trackSearchText;
        public string TrackSearchText
        {
            get { return _trackSearchText; }
            set
            {
                _trackSearchText = value;
                RaisePropertyChanged("TrackSearchText");
                FilterTracksCollectionView();
            }
        }

        private void SortTrackCollection(Sorting sorting)
        {
            if (TracksCollectionView == null)
                return;

            IsTracksLoading = true;

            try
            {
                using (TracksCollectionView.DeferRefresh())
                {
                    TracksCollectionView.SortDescriptions.Clear();
                    TracksCollectionView.SortDescriptions.Add(new SortDescription(sorting.Property, sorting.SortDirection));
                }

                if (TracksCollectionView.FirstOrDefault() != null)
                {
                    Messenger.Default.Send(new MessengerHelper
                    {
                        Item = TracksCollectionView.FirstOrDefault(),
                        Action = MessengerAction.ScrollToItem,
                        Target = TargetView.Tracks
                    });
                }

                UpdateTracksIndex();
            }
            catch (Exception)
            {
                //
            }

            IsTracksLoading = false;
        }

        public void FilterTracksCollectionView()
        {
            if (TracksCollectionView == null)
                return;

            IsTracksLoading = true;

            //make sure the filtered items are clear
            string searchStr = TrackSearchText;
            _filteredTracksCollection.Clear();
            using (TracksCollectionView.DeferRefresh())
            {
                if (!string.IsNullOrEmpty(searchStr))
                {
                    TracksCollectionView.Filter = c => (((Track)c).Title).Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase) ||
                        (((Track)c).Album).Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase) ||
                        (((Track)c).Artist).Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase);

                    _filteredTracksCollection.AddRange(_tracksCollectionCopy.Where(c => c.Title.Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase) ||
                    c.Album.Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase) ||
                    c.Artist.Contains(TrackSearchText, StringComparison.CurrentCultureIgnoreCase)));
                }
                else
                {
                    TracksCollectionView.Filter = c => c != null; //bit of a hack to clear filters
                    _filteredTracksCollection.AddRange(_tracksCollectionCopy);
                }
            }

            UpdateTracksIndex();

            IsTracksLoading = false;
        }

        #endregion
    }
}