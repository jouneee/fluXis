using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Skinning.Json;
using osu.Framework.Graphics;

namespace fluXis.Game.Skinning.Default;

public class DefaultSkinJson : SkinJson
{
    public override Colour4 GetLaneColor(int lane, int maxLanes)
        => FluXisColors.GetLaneColor(lane, maxLanes).Lighten(.6f);
}
