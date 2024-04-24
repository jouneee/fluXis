using fluXis.Game.Audio;
using fluXis.Game.Graphics;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Overlay.Network;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace fluXis.Game.Overlay.User.Header;

public partial class HeaderEditButton : CompositeDrawable
{
    [Resolved]
    private UISamples samples { get; set; }

    [CanBeNull]
    [Resolved(CanBeNull = true)]
    private FluXisGame game { get; set; }

    private Box hover;
    private Box flash;

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.X;
        Height = 50;
        CornerRadius = 25;
        Masking = true;
        EdgeEffect = FluXisStyles.ShadowSmall;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = FluXisColors.Background2
            },
            hover = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            },
            flash = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            },
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Horizontal,
                Colour = FluXisColors.Text,
                Padding = new MarginPadding { Horizontal = 20 },
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Size = new Vector2(20),
                        Child = new SpriteIcon
                        {
                            Icon = FontAwesome6.Solid.Pen,
                            Size = new Vector2(20)
                        }
                    },
                    new FluXisSpriteText
                    {
                        Text = "Edit",
                        WebFontSize = 16
                    }
                }
            }
        };
    }

    protected override bool OnClick(ClickEvent e)
    {
        samples.Click();
        flash.FadeOutFromOne(1000, Easing.OutQuint);

        game?.OpenDashboard(DashboardTabType.Account);
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        samples.Hover();
        hover.FadeTo(.2f, 50);

        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        hover.FadeOut(200);
    }
}
