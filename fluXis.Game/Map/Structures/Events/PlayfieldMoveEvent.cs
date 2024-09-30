using fluXis.Game.Map.Structures.Bases;
using fluXis.Game.Screens.Gameplay.Ruleset;
using Newtonsoft.Json;
using osu.Framework.Graphics;

namespace fluXis.Game.Map.Structures.Events;

public class PlayfieldMoveEvent : IMapEvent, IHasDuration, IHasEasing, IApplicableToPlayfield
{
    [JsonProperty("time")]
    public double Time { get; set; }

    [JsonProperty("x")]
    public float OffsetX { get; set; }

    [JsonProperty("y")]
    public float OffsetY { get; set; }

    [JsonProperty("duration")]
    public double Duration { get; set; }

    [JsonProperty("ease")]
    public Easing Easing { get; set; } = Easing.OutQuint;

    public void Apply(Playfield playfield)
    {
        using (playfield.BeginAbsoluteSequence(Time))
        {
            playfield.MoveToX(OffsetX, Duration, Easing);
            playfield.MoveToY(OffsetY, Duration, Easing);
        }
    }
}
