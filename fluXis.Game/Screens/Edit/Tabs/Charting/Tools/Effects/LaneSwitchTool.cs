using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Screens.Edit.Tabs.Charting.Blueprints.Placement;
using fluXis.Game.Screens.Edit.Tabs.Charting.Blueprints.Placement.Effect;
using osu.Framework.Graphics;

namespace fluXis.Game.Screens.Edit.Tabs.Charting.Tools.Effects;

public class LaneSwitchTool : EffectTool
{
    public override string Name => "Lane Switch";
    public override string Description => "Changes the amount of lanes mid-song.";
    public override string Letter => "L";
    public override Drawable CreateIcon() => new FluXisIcon { Type = FluXisIconType.LaneSwitch };
    public override PlacementBlueprint CreateBlueprint() => new LaneSwitchPlacementBlueprint();
}
