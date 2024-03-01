using fluXis.Game.Graphics.Background;
using fluXis.Game.Online.Activity;
using fluXis.Game.Overlay.Toolbar;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace fluXis.Game.Screens;

public partial class FluXisScreenStack : ScreenStack
{
    [Resolved]
    private GlobalBackground backgrounds { get; set; }

    [CanBeNull]
    [Resolved(CanBeNull = true)]
    private Toolbar toolbar { get; set; }

    public bool AllowMusicControl => CurrentScreen is FluXisScreen { AllowMusicControl: true };

    private Bindable<UserActivity> activity { get; } = new();

    public FluXisScreenStack(Bindable<UserActivity> activity = null)
    {
        this.activity = activity ?? this.activity;

        RelativeSizeAxes = Axes.Both;
        ScreenPushed += updateScreen;
        ScreenExited += updateScreen;
    }

    private void updateScreen(IScreen last, IScreen next)
    {
        if (last is FluXisScreen lastScreen)
        {
            activity.UnbindFrom(lastScreen.Activity);
        }

        if (next is FluXisScreen nextScreen)
        {
            activity.BindTo(nextScreen.Activity);

            if (nextScreen.ApplyValuesAfterLoad && !nextScreen.IsLoaded) nextScreen.OnLoadComplete += _ => updateValues(nextScreen);
            else updateValues(nextScreen);
        }
        else
        {
            backgrounds.Zoom = 1f;
            backgrounds.ParallaxStrength = .05f;
            backgrounds.SetBlur(0f);
            backgrounds.SetDim(0.25f);
        }
    }

    private void updateValues(FluXisScreen screen)
    {
        if (backgrounds == null || toolbar == null)
            return;

        backgrounds.Zoom = screen.Zoom;
        backgrounds.ParallaxStrength = screen.ParallaxStrength;
        toolbar.ShowToolbar.Value = screen.ShowToolbar;
        backgrounds.SetBlur(screen.BackgroundBlur);
        backgrounds.SetDim(screen.BackgroundDim);
    }
}
