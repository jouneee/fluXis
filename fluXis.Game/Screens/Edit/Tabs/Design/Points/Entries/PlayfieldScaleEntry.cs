using System.Collections.Generic;
using System.Linq;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Map.Events;
using fluXis.Game.Map.Structures;
using fluXis.Game.Screens.Edit.Tabs.Shared.Points.List;
using fluXis.Game.Screens.Edit.Tabs.Shared.Points.Settings;
using fluXis.Game.Screens.Edit.Tabs.Shared.Points.Settings.Preset;
using fluXis.Game.Utils;
using osu.Framework.Graphics;

namespace fluXis.Game.Screens.Edit.Tabs.Design.Points.Entries;

public partial class PlayfieldScaleEntry : PointListEntry
{
    protected override string Text => "Playfield Scale";
    protected override Colour4 Color => Colour4.FromHex("#D279C4");

    private PlayfieldScaleEvent scale => Object as PlayfieldScaleEvent;

    public PlayfieldScaleEntry(PlayfieldScaleEvent obj)
        : base(obj)
    {
    }

    protected override ITimedObject CreateClone() => new PlayfieldScaleEvent
    {
        Time = Object.Time,
        ScaleX = scale.ScaleX,
        ScaleY = scale.ScaleY,
        Duration = scale.Duration,
        Easing = scale.Easing
    };

    protected override Drawable[] CreateValueContent()
    {
        return new Drawable[]
        {
            new FluXisSpriteText
            {
                Text = $"{scale.ScaleX.ToStringInvariant("0.00")}x{scale.ScaleY.ToStringInvariant("0.00")} {(int)scale.Duration}ms",
                Colour = Color
            }
        };
    }

    protected override IEnumerable<Drawable> CreateSettings()
    {
        return base.CreateSettings().Concat(new Drawable[]
        {
            new PointSettingsLength<PlayfieldScaleEvent>(Map, scale, BeatLength),
            new PointSettingsTextBox
            {
                Text = "ScaleX",
                DefaultText = scale.ScaleX.ToStringInvariant(),
                OnTextChanged = box =>
                {
                    if (box.Text.TryParseFloatInvariant(out var result))
                        scale.ScaleX = result;
                    else
                        box.NotifyError();

                    Map.Update(scale);
                }
            },
            new PointSettingsTextBox
            {
                Text = "ScaleY",
                DefaultText = scale.ScaleY.ToStringInvariant(),
                OnTextChanged = box =>
                {
                    if (box.Text.TryParseFloatInvariant(out var result))
                        scale.ScaleY = result;
                    else
                        box.NotifyError();

                    Map.Update(scale);
                }
            }
        });
    }
}