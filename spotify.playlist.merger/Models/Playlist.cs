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
            DurationStr = duration;
            Owner = owner;
            Type = type;
            if (type == PlaylistCategoryType.MyPlaylist) CanModify = true;
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

        private int _indexB;
        public int IndexB
        {
            get => _indexB;
            set => _ = SetProperty(_indexB, value, () => _indexB = value);
        }

        private int _indexC;
        public int IndexC
        {
            get => _indexC;
            set => _ = SetProperty(_indexC, value, () => _indexC = value);
        }

        private string _durationStr = "0";
        public string DurationStr
        {
            get { return _durationStr; }
            set { SetProperty(_durationStr, value, () => _durationStr = value); }
        }

        private bool _canModify;
        public bool CanModify
        {
            get { return _canModify; }
            set { SetProperty(this._canModify, value, () => this._canModify = value); }
        }

        private User _owner;
        public User Owner
        {
            get { return _owner; }
            set { SetProperty(this._owner, value, () => this._owner = value); }
        }

        internal Playlist Clone()
        {
            return new Playlist
            {
                Id = Id,
                Title = Title,
                Uri = Uri,
                Image = Image,
                Description = Description,
                Count = Count,
                DurationStr = DurationStr,
                Owner = Owner,
                Type = Type,
                IndexA = IndexA,
                IndexB = IndexB,
                IndexC = IndexC,
                CanModify = CanModify,
            };
        }
    }
}
