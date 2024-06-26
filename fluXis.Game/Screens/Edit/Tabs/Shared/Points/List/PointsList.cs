using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Game.Audio;
using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Map.Structures;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;

namespace fluXis.Game.Screens.Edit.Tabs.Shared.Points.List;

public abstract partial class PointsList : Container
{
    [Resolved]
    protected EditorMap Map { get; private set; }

    [Resolved]
    private EditorClock clock { get; set; }

    public Action<IEnumerable<Drawable>> ShowSettings { get; set; }
    public Action RequestClose { get; set; }

    private bool initialLoad = true;
    private FillFlowContainer<PointListEntry> flow;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = new FluXisScrollContainer
        {
            RelativeSizeAxes = Axes.Both,
            ScrollbarVisible = false,
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Children = new Drawable[]
                        {
                            new FluXisSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                WebFontSize = 20,
                                Text = "Points"
                            },
                            new AddButton(CreateAddEntries())
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight
                            }
                        }
                    },
                    flow = new FillFlowContainer<PointListEntry>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10)
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        RegisterEvents();

        initialLoad = false;
        sortPoints();
    }

    protected abstract void RegisterEvents();

    protected abstract PointListEntry CreateEntryFor(ITimedObject obj);

    protected abstract IEnumerable<AddButtonEntry> CreateAddEntries();

    protected void Create(ITimedObject obj)
    {
        obj.Time = (float)clock.CurrentTime;
        Map.Add(obj);

        var entry = flow.FirstOrDefault(e => e.Object == obj);
        entry?.OpenSettings();
    }

    private void sortPoints()
    {
        flow.OrderBy(e => e.Object.Time).ForEach(e => flow.SetLayoutPosition(e, e.Object.Time));
    }

    protected void AddPoint(ITimedObject obj)
    {
        var entry = CreateEntryFor(obj);

        if (entry != null)
        {
            entry.ShowSettings = ShowSettings;
            entry.RequestClose = RequestClose;
            entry.OnClone = Create;
            flow.Add(entry);
        }

        if (!initialLoad)
            sortPoints();
    }

    public void ShowPoint(ITimedObject obj)
    {
        var entry = flow.FirstOrDefault(e => e.Object == obj);
        entry?.OpenSettings();
    }

    protected void UpdatePoint(ITimedObject obj)
    {
        var entry = flow.FirstOrDefault(e => e.Object == obj);
        entry?.UpdateValues();

        sortPoints();
    }

    protected void RemovePoint(ITimedObject obj)
    {
        var entry = flow.FirstOrDefault(e => e.Object == obj);

        if (entry != null)
            flow.Remove(entry, true);
    }

    protected override bool OnClick(ClickEvent e) => true;

    private partial class AddButton : PointsListIconButton, IHasPopover
    {
        private IEnumerable<AddButtonEntry> entries { get; }

        public AddButton(IEnumerable<AddButtonEntry> entries)
            : base(null)
        {
            this.entries = entries;
            Action = this.ShowPopover;
        }

        private void createAndHide(Action action)
        {
            action();
            this.HidePopover();
        }

        public Popover GetPopover()
        {
            return new FluXisPopover
            {
                HandleHover = false,
                ContentPadding = 0,
                BodyRadius = 5,
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    ChildrenEnumerable = entries.Select(x => new Entry(x.Text, () => createAndHide(x.CreateAction)))
                }
            };
        }

        private partial class Entry : CompositeDrawable
        {
            private string text { get; }
            private Action create { get; }

            public Entry(string text, Action create)
            {
                this.text = text;
                this.create = create;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(200, 30);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    },
                    new FluXisSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        WebFontSize = 16,
                        Text = text,
                        Margin = new MarginPadding(10)
                    }
                };
            }

            protected override bool OnClick(ClickEvent e)
            {
                create();
                return false;
            }
        }
    }

    protected class AddButtonEntry
    {
        public string Text { get; }
        public Action CreateAction { get; }

        public AddButtonEntry(string text, Action create)
        {
            Text = text;
            CreateAction = create;
        }
    }

    public partial class PointsListIconButton : CircularContainer
    {
        [Resolved]
        private UISamples samples { get; set; }

        protected virtual IconUsage ButtonIcon => FontAwesome.Solid.Plus;
        protected Action Action { get; init; }

        protected Box Background { get; private set; }
        protected Box Hover { get; private set; }
        protected SpriteIcon Icon { get; private set; }

        public PointsListIconButton(Action action)
        {
            Action = action;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(30);
            Masking = true;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FluXisColors.Background3
                },
                Hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                Icon = new SpriteIcon
                {
                    Icon = ButtonIcon,
                    Size = new Vector2(16),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        protected virtual void UpdateColors(bool hovered)
        {
            if (hovered)
                Hover.FadeTo(.2f, 50);
            else
                Hover.FadeOut(200);
        }

        protected override bool OnClick(ClickEvent e)
        {
            samples.Click();
            Action?.Invoke();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            samples.Hover();
            UpdateColors(true);
            return false; // else the list closes
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateColors(false);
        }
    }
}
