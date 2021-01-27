﻿namespace spotify.playlist.merger.Models
{
    public class Playlist : MediaItemBase
    {
        public Playlist() { }
        public Playlist(string id, string title, string uri, string imgUrl, string description, int count, string duration, User owner, PlaylistCategoryType type)
            : base(id, title, uri, imgUrl)
        {
            Description = Helpers.CleanString(description);
            Count = count;
            Duration = duration;
            Owner = owner;
            Type = type;
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(_description, value, () => _description = value); }
        }

        private int _count;
        public int Count
        {
            get => _count;
            set => _ = SetProperty(_count, value, () => _count = value);
        }

        private PlaylistCategoryType _type;
        public PlaylistCategoryType Type
        {
            get => _type;
            set => _ = SetProperty(_type, value, () => _type = value);
        }

        private int _position;
        public int Position
        {
            get => _position;
            set => _ = SetProperty(_position, value, () => _position = value);
        }

        private int _positionAlt;
        public int PositionAlt
        {
            get => _positionAlt;
            set => _ = SetProperty(_positionAlt, value, () => _positionAlt = value);
        }

        private string _duration = "0";
        public string Duration
        {
            get { return _duration; }
            set { SetProperty(_duration, value, () => _duration = value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(this._isSelected, value, () => this._isSelected = value); }
        }

        private User _owner;
        public User Owner
        {
            get { return _owner; }
            set { SetProperty(this._owner, value, () => this._owner = value); }
        }
    }
}