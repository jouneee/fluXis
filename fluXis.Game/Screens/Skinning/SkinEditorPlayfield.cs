using fluXis.Game.Skinning;
using fluXis.Game.Skinning.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Skinning;

public partial class SkinEditorPlayfield : Container
{
    public SkinJson SkinJson { get; set; }
    public SkinManager SkinManager { get; set; }
    public int KeyMode { get; set; }

    private Container stageContainer;
    private Drawable hitline;
    private FillFlowContainer receptorContainer;
    private Container hitObjectContainer;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Y;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        addContent();
    }

    public void Reload()
    {
        Clear();
        addContent();
    }

    private void addContent()
    {
        InternalChildren = new Drawable[]
        {
            stageContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new[]
                {
                    SkinManager.GetStageBorder(false),
                    SkinManager.GetStageBackground(),
                    SkinManager.GetStageBorder(true)
                }
            },
            receptorContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre
            },
            hitObjectContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            },
            hitline = SkinManager.GetHitLine().With(line =>
            {
                line.Width = 1;
                line.RelativeSizeAxes = Axes.X;
            })
        };

        for (int i = 0; i < KeyMode; i++)
        {
            var receptor = SkinManager.GetReceptor(i + 1, KeyMode, false);
            receptorContainer.Add(receptor);
            receptor.Width = 1f / KeyMode;

            var hitObject = SkinManager.GetHitObject(i + 1, KeyMode);
            hitObject.Anchor = Anchor.BottomLeft;
            hitObject.Origin = Anchor.BottomLeft;
            hitObjectContainer.Add(hitObject);
        }
    }

    protected override void Update()
    {
        Width = SkinJson.GetKeymode(KeyMode).ColumnWidth * KeyMode;

        var hitpos = SkinJson.GetKeymode(KeyMode).HitPosition;
        hitline.Y = -hitpos;

        foreach (var drawable in hitObjectContainer)
        {
            int index = hitObjectContainer.IndexOf(drawable);

            drawable.Width = 1f / KeyMode;
            drawable.X = index * SkinJson.GetKeymode(KeyMode).ColumnWidth;

            float time = index * 100;
            drawable.Y = -hitpos - .5f * (time * 3);
        }
    }
}
