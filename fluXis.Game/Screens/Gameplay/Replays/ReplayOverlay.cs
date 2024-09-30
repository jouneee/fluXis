using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Online;
using fluXis.Game.Utils.Extensions;
using fluXis.Shared.Replays;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Gameplay.Replays;

public partial class ReplayOverlay : FillFlowContainer
{
    private Replay replay { get; }

    public ReplayOverlay(Replay replay)
    {
        this.replay = replay;
    }

    [BackgroundDependencyLoader]
    private void load(UserCache users)
    {
        AutoSizeAxes = Axes.Both;
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Direction = FillDirection.Vertical;
        Y = 80;
        Alpha = .8f;

        InternalChildren = new Drawable[]
        {
            new FluXisSpriteText
            {
                FontSize = 32,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Text = "Replay Mode (Experimental)"
            },
            new FluXisSpriteText
            {
                FontSize = 24,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Text = $"Watching {replay.GetPlayer(users)?.NameWithApostrophe} replay",
                Alpha = .8f
            }
        };
    }
}
