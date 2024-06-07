using System;
using System.Collections.Generic;
using System.IO;
using fluXis.Game.Map;
using fluXis.Game.Utils;
using fluXis.Shared.Utils;
using JetBrains.Annotations;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using Realms;

namespace fluXis.Game.Database.Maps;

public class RealmMap : RealmObject
{
    [PrimaryKey]
    public Guid ID { get; set; }

    public string Difficulty { get; set; } = string.Empty;
    public RealmMapMetadata Metadata { get; set; } = null!;
    public RealmMapSet MapSet { get; set; } = null!;

    /**
     * -2 = Local
     * -1 = Blacklisted (server side)
     * 0 = Unsubmitted
     * 1 = Pending
     * 2 = Impure
     * 3 = Pure
     * everything over 100 is from other games
     */
    [MapTo(nameof(Status))]
    public int StatusInt { get; set; } = -2;

    [Ignored]
    public MapStatus Status { set => StatusInt = (int)value; }

    public string FileName { get; set; } = string.Empty;
    public long OnlineID { get; set; } = -1;
    public RealmMapFilters Filters { get; set; } = null!;
    public int KeyCount { get; set; } = 4;
    public float Rating { get; set; }

    public string Hash { get; set; } = string.Empty;
    public string OnlineHash { get; set; } = string.Empty;

    [Ignored]
    public bool UpToDate => Hash == OnlineHash;

    public float AccuracyDifficulty { get; set; } = 8;
    public float HealthDifficulty { get; set; } = 8;

    public DateTimeOffset LastLocalUpdate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? LastOnlineUpdate { get; set; }
    public DateTimeOffset? LastPlayed { get; set; }

    public RealmMap([CanBeNull] RealmMapMetadata meta = null)
    {
        Metadata = meta ?? new RealmMapMetadata();
        ID = Guid.NewGuid();
    }

    [UsedImplicitly]
    private RealmMap()
    {
    }

    public override string ToString() => $"{ID} - {Metadata}";

    public void ResetOnlineInfo()
    {
        OnlineID = -1;
        LastOnlineUpdate = null;
        OnlineHash = string.Empty;
        Status = MapStatus.Local;
    }

    public virtual MapInfo GetMapInfo() => GetMapInfo<MapInfo>();

    [CanBeNull]
    public virtual T GetMapInfo<T>()
        where T : MapInfo, new()
    {
        try
        {
            var path = MapFiles.GetFullPath(MapSet.GetPathForFile(FileName));
            var json = File.ReadAllText(path);
            var hash = MapUtils.GetHash(json);
            var map = json.Deserialize<T>();
            map.Map = this;
            map.Hash = hash;
            return map;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to load map from path: " + MapSet.GetPathForFile(FileName));
            return null;
        }
    }

    public virtual Texture GetBackground()
    {
        var backgrounds = MapSet.Resources?.BackgroundStore;
        return backgrounds?.Get(MapSet.GetPathForFile(Metadata.Background));
    }

    public virtual Stream GetBackgroundStream()
    {
        var backgrounds = MapSet.Resources?.BackgroundStore;
        Logger.Log("Backgrounds: " + backgrounds);
        return backgrounds?.GetStream(MapSet.GetPathForFile(Metadata.Background));
    }

    public virtual Texture GetPanelBackground()
    {
        var croppedBackgrounds = MapSet.Resources?.CroppedBackgroundStore;
        return croppedBackgrounds?.Get(MapSet.GetPathForFile(Metadata.Background));
    }

    public virtual Track GetTrack()
    {
        var tracks = MapSet.Resources?.TrackStore;
        return tracks?.Get(MapSet.GetPathForFile(Metadata.Audio));
    }

    public static RealmMap CreateNew()
    {
        var set = new RealmMapSet(new List<RealmMap>
        {
            new(null) { Filters = new RealmMapFilters() }
        });

        return set.Maps[0];
    }
}

public enum MapStatus
{
    Local = -2,
    Blacklisted = -1,
    Unsubmitted = 0,
    Pending = 1,
    Impure = 2,
    Pure = 3
}
