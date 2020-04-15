using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Pixelynx.Core
{
    public class Asset
    {
        #region Constructors.
        protected Asset(string name, AssetType type, Guid id = new Guid())
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Name = name; 
            Type = type;
        }

        public Asset(string name, AssetType type, string assetUri, Guid id = new Guid()) : this(name, type, id)
        {
            Uri = assetUri;
        }

        public Asset(string name, AssetType type, byte[] assetData, Guid id = new Guid()) : this(name, type, id)
        {
            Name = name;
            Type = type;
            RawData = assetData;
        }
        #endregion

        #region Properties.
        public Asset Parent { get; set; }
        public Guid Id { get; }
        public string Name { get; set; }
        public Thumbnail Thumbnail { get; set; }
        public AssetType Type { get; }
        public int Cost { get; set; }

        public string Uri { get; } = string.Empty;
        public byte[] RawData { get; private set; } = new byte[0];
        #endregion

        #region Public Methods.
        public byte[] GetRawData()
        {
            if (!RawData.Any())
            {
                using (var webClient = new WebClient()) 
                { 
                    RawData = webClient.DownloadData(Uri);
                }
            }

            return RawData;
        }
        #endregion
    }

    public enum AssetType { Model, Material, Animation }
    public enum FileType { Png, Jpeg }
}
