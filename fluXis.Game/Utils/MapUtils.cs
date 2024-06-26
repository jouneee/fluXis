using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using fluXis.Game.Database.Maps;
using fluXis.Game.Map;
using fluXis.Game.Map.Structures;

namespace fluXis.Game.Utils;

public static class MapUtils
{
    public static int CompareSets(RealmMapSet first, RealmMapSet second)
    {
        // compare title
        var compare = string.Compare(first.Metadata.Title, second.Metadata.Title, StringComparison.OrdinalIgnoreCase);

        if (compare != 0)
            return compare;

        compare = second.DateAdded.CompareTo(first.DateAdded);
        return compare;
    }

    public static RealmMapFilters UpdateFilters(this RealmMapFilters filters, MapInfo map, MapEvents events)
    {
        filters.Reset();

        foreach (var hitObject in map.HitObjects)
        {
            filters.Length = Math.Max(filters.Length, hitObject.EndTime);

            if (hitObject.LongNote)
                filters.LongNoteCount++;
            else
                filters.NoteCount++;
        }

        filters.NotesPerSecond = getNps(map.HitObjects);

        foreach (var timingPoint in map.TimingPoints)
        {
            if (filters.BPMMin == 0)
                filters.BPMMin = timingPoint.BPM;

            filters.BPMMin = Math.Min(filters.BPMMin, timingPoint.BPM);
            filters.BPMMax = Math.Max(filters.BPMMax, timingPoint.BPM);
        }

        foreach (var scrollVelocity in map.ScrollVelocities)
        {
            if (scrollVelocity.Multiplier != 1)
            {
                filters.HasScrollVelocity = true;
                break;
            }
        }

        if (events != null)
        {
            filters.HasLaneSwitch = events.LaneSwitchEvents.Count > 0;
            filters.HasFlash = events.FlashEvents.Count > 0;
        }

        return filters;
    }

    public static RealmMapFilters GetMapFilters(MapInfo map, MapEvents events)
        => new RealmMapFilters().UpdateFilters(map, events);

    private static float getNps(List<HitObject> hitObjects)
    {
        if (hitObjects.Count == 0) return 0;

        Dictionary<int, float> seconds = new Dictionary<int, float>();

        foreach (var hitObject in hitObjects)
        {
            int second = (int)hitObject.Time / 1000;

            var value = hitObject.Type switch
            {
                1 => 0.1f, // tick
                _ => 1
            };

            if (!seconds.TryAdd(second, value))
                seconds[second] += value;
        }

        return seconds.Average(x => x.Value);
    }

    public static string GetHash(string input) => BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
    public static string GetHash(Stream input) => BitConverter.ToString(SHA256.Create().ComputeHash(input)).Replace("-", "").ToLower();

    public static float GetDifficulty(float difficulty, float min, float mid, float max)
    {
        if (difficulty > 5)
            return mid + (max - mid) * getDifficulty(difficulty);
        if (difficulty < 5)
            return mid + (mid - min) * getDifficulty(difficulty);

        return mid;
    }

    private static float getDifficulty(float difficulty) => (difficulty - 5) / 5;
}
