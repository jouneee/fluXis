using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using fluXis.Game.Audio;
using fluXis.Game.Database;
using fluXis.Game.Database.Maps;
using fluXis.Game.Database.Score;
using fluXis.Game.Graphics;
using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface;
using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Graphics.UserInterface.Context;
using fluXis.Game.IO;
using fluXis.Game.Map;
using fluXis.Game.Online;
using fluXis.Game.Online.API.Models.Scores;
using fluXis.Game.Online.API.Requests.Maps;
using fluXis.Game.Online.Fluxel;
using fluXis.Game.Overlay.Notifications;
using fluXis.Game.Overlay.Notifications.Tasks;
using fluXis.Shared.Replays;
using fluXis.Shared.Utils;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osuTK;

namespace fluXis.Game.Screens.Select.Info.Scores;

public partial class ScoreList : GridContainer
{
    [Resolved]
    private FluXisRealm realm { get; set; }

    [Resolved]
    private FluxelClient fluxel { get; set; }

    [Resolved]
    private MapStore maps { get; set; }

    [Resolved]
    private ReplayStorage replays { get; set; }

    [Resolved]
    private NotificationManager notifications { get; set; }

    [Resolved(CanBeNull = true)]
    private SelectScreen screen { get; set; }

    private RealmMap map;
    private ScoreListType type = ScoreListType.Local;

    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;

    private FillFlowContainer outOfDateContainer;
    private FluXisSpriteText noScoresText;
    private FluXisScrollContainer scrollContainer;
    private FillFlowContainer<LeaderboardTypeButton> typeSwitcher;
    private LoadingIcon loadingIcon;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        RowDimensions = new[]
        {
            new Dimension(GridSizeMode.AutoSize),
            new Dimension()
        };

        Content = new[]
        {
            new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    CornerRadius = 10,
                    Masking = true,
                    Shear = new Vector2(-.1f, 0),
                    Margin = new MarginPadding { Bottom = 10 },
                    EdgeEffect = FluXisStyles.ShadowSmall,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FluXisColors.Background2
                        },
                        new FluXisSpriteText
                        {
                            Text = "Scores",
                            FontSize = 32,
                            Shear = new Vector2(.1f, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            X = 20
                        },
                        typeSwitcher = new FillFlowContainer<LeaderboardTypeButton>
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            X = -40,
                            Children = Enum.GetValues<ScoreListType>().Select(t => new LeaderboardTypeButton
                            {
                                Type = t,
                                ScoreList = this
                            }).ToList()
                        }
                    }
                }
            },
            new Drawable[]
            {
                new FluXisContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = 20 },
                    Children = new Drawable[]
                    {
                        noScoresText = new FluXisSpriteText
                        {
                            Text = "No scores yet!",
                            FontSize = 32,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0
                        },
                        scrollContainer = new FluXisScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarAnchor = Anchor.TopRight
                        },
                        loadingIcon = new LoadingIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(50),
                            Alpha = 0
                        },
                        outOfDateContainer = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome6.Solid.TriangleExclamation,
                                    Colour = Colour4.FromHex("#ffd500"),
                                    Size = new Vector2(20),
                                    Shadow = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft
                                },
                                new FluXisSpriteText
                                {
                                    Text = "Your local version of this map is out of date!",
                                    FontSize = 24,
                                    Shadow = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        ScheduleAfterChildren(() => setType(ScoreListType.Local));

        maps.MapBindable.BindValueChanged(onMapChanged, true);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        maps.MapBindable.ValueChanged -= onMapChanged;
    }

    private void setType(ScoreListType type)
    {
        this.type = type;
        typeSwitcher.Children.ForEach(c => c.Selected = c.Type == type);
    }

    public void Refresh()
    {
        if (map == null)
            return;

        changeMap(map);
    }

    private void onMapChanged(ValueChangedEvent<RealmMap> e) => changeMap(e.NewValue);

    private void changeMap(RealmMap map)
    {
        if (!IsLoaded)
        {
            Schedule(() => changeMap(map));
            return;
        }

        loadingIcon.FadeIn(200);
        outOfDateContainer.FadeOut(200);

        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        scrollContainer.ScrollContent.Clear();
        this.map = map;

        cancellationToken = cancellationTokenSource.Token;
        Task.Run(() => loadScores(cancellationToken), cancellationToken);
    }

    private void loadScores(CancellationToken cancellationToken)
    {
        List<ScoreListEntry> scores = new();

        switch (type)
        {
            case ScoreListType.Local:
                realm?.Run(r => r.All<RealmScore>().ToList().ForEach(s =>
                {
                    if (s.MapID == map.ID)
                    {
                        var info = s.ToScoreInfo();
                        var detach = s.Detach();

                        scores.Add(new ScoreListEntry
                        {
                            ScoreInfo = info,
                            Map = map,
                            Player = s.Player,
                            DeleteAction = () =>
                            {
                                realm.RunWrite(r2 =>
                                {
                                    var realmScore = r2.Find<RealmScore>(detach.ID);

                                    if (realmScore == null)
                                        return;

                                    r2.Remove(realmScore);
                                });
                            },
                            ReplayAction = replays.Exists(s.ID) ? () => screen?.ViewReplay(map, detach) : null
                        });
                    }
                }));
                break;

            case ScoreListType.Global or ScoreListType.Country or ScoreListType.Club:
                if (map.OnlineID == -1)
                {
                    showNotSubmittedError();
                    return;
                }

                var onlineScores = getScores(cancellationToken);
                if (onlineScores == null) return;

                scores.AddRange(onlineScores);
                break;

            default:
                noScoresText.Text = $"{type} leaderboards are not available yet!";
                Schedule(() =>
                {
                    noScoresText.FadeIn(200);
                    loadingIcon.FadeOut(200);
                });
                return;
        }

        Schedule(() =>
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            scores.Sort((a, b) => b.ScoreInfo.Score.CompareTo(a.ScoreInfo.Score));
            scores.ForEach(s => addScore(s, scores.IndexOf(s) + 1));

            if (scrollContainer.ScrollContent.Children.Count == 0)
                noScoresText.Text = map.MapSet.Managed ? "Scores are not available for this map!" : "No scores yet!";

            noScoresText.FadeTo(scrollContainer.ScrollContent.Children.Count == 0 ? 1 : 0, 200);
            loadingIcon.FadeOut(200);
        });
    }

    private void showNotSubmittedError()
    {
        noScoresText.Text = "This map is not submitted online!";
        Schedule(() =>
        {
            noScoresText.FadeIn(200);
            loadingIcon.FadeOut(200);
        });
    }

    [CanBeNull]
    private List<ScoreListEntry> getScores(CancellationToken cancellationToken)
    {
        var req = new MapLeaderboardRequest(type, map.OnlineID);
        req.Perform(fluxel);

        if (cancellationToken.IsCancellationRequested)
            return null;

        if (req.Response.Status != 200)
        {
            noScoresText.Text = req.Response.Message;
            Schedule(() =>
            {
                noScoresText.FadeTo(1, 200);
                loadingIcon.FadeOut(200);
            });
            return null;
        }

        var onlineMap = req.Response.Data.Map;

        realm?.RunWrite(r =>
        {
            var m = r.Find<RealmMap>(map.ID);

            if (m == null)
                return;

            m.OnlineHash = map.OnlineHash = onlineMap.Hash;

            // i would like to know why i didn't put "last update" on the map but on the mapset
            // m.LastOnlineUpdate = map.LastOnlineUpdate = onlineMap.LastUpdate;

            if (m.StatusInt != onlineMap.Status)
            {
                m.MapSet.SetStatus(onlineMap.Status);
                map.MapSet.SetStatus(onlineMap.Status);
            }
        });

        if (!map.UpToDate)
            Schedule(() => outOfDateContainer.FadeIn(200));

        return req.Response.Data.Scores.Select(x => new ScoreListEntry
        {
            ScoreInfo = x.ToScoreInfo(),
            Map = map,
            Player = UserCache.GetUser(x.UserId),
            DownloadAction = () => downloadScore(map, x)
        }).ToList();
    }

    private void downloadScore(RealmMap map, APIScore score)
    {
        if (realm.Run(r => r.All<RealmScore>().Any(s => s.OnlineID == score.Id)))
        {
            notifications.SendSmallText("You already have this score!");
            return;
        }

        var realmScore = realm.RunWrite(r => r.Add(RealmScore.FromScoreInfo(map, score.ToScoreInfo(), score.Id))).Detach();

        var notification = new TaskNotificationData
        {
            Text = "Downloading replay...",
            TextWorking = "Downloading...",
            TextFinished = "Done!",
            TextFailed = "Failed to download replay!",
        };

        notifications.AddTask(notification);

        // ReSharper disable once MethodSupportsCancellation
        Task.Run(() =>
        {
            try
            {
                var req = new WebRequest($"{fluxel.Endpoint.AssetUrl}/replay/{score.Id}.frp");
                req.AllowInsecureRequests = true;
                req.Perform();

                if (req.ResponseStream == null)
                    return;

                var json = req.GetResponseString();
                var replay = json.Deserialize<Replay>();
                replays.Save(replay, realmScore.ID);

                notification.State = LoadingState.Complete;
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to download replay for score {score.Id}: {e.Message}", LoggingTarget.Network, LogLevel.Error);
                notification.State = LoadingState.Failed;
            }
        });
    }

    private void addScore(ScoreListEntry entry, int index = -1)
    {
        entry.Place = index;
        entry.Y = scrollContainer.ScrollContent.Children.Count > 0 ? scrollContainer.ScrollContent.Children[^1].Y + scrollContainer.ScrollContent.Children[^1].Height + 5 : 0;
        scrollContainer.ScrollContent.Add(entry);
    }

    private partial class LeaderboardTypeButton : ClickableContainer
    {
        public ScoreListType Type { get; init; }
        public ScoreList ScoreList { get; init; }

        public bool Selected
        {
            set => content.BorderThickness = value ? 3 : 0;
        }

        [Resolved]
        private UISamples samples { get; set; }

        private Container content;
        private Box hover;
        private Box flash;

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = 100;
            Height = 30;
            Shear = new Vector2(.2f, 0);

            var color = Type switch
            {
                ScoreListType.Local => Colour4.FromHSV(120f / 360f, .6f, 1f),
                ScoreListType.Global => Colour4.FromHSV(30f / 360f, .6f, 1f),
                ScoreListType.Country => Colour4.FromHSV(0f, .6f, 1f),
                ScoreListType.Friends => Colour4.FromHSV(210f / 360f, .6f, 1f),
                ScoreListType.Club => Colour4.FromHSV(270f / 360f, .6f, 1f),
                _ => FluXisColors.Background4
            };

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                CornerRadius = 5,
                Masking = true,
                BorderColour = ColourInfo.GradientVertical(color, color.Lighten(1)),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = color
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
                    new FluXisSpriteText
                    {
                        Text = Type.ToString(),
                        FontSize = 18,
                        Shear = new Vector2(-.1f, 0),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Colour4.Black,
                        Alpha = .75f
                    }
                }
            };

            Action = () =>
            {
                ScoreList.setType(Type);
                ScoreList.Refresh();
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            flash.FadeOutFromOne(1000, Easing.OutQuint);
            samples.Click();
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeTo(.2f, 50);
            samples.Hover();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeOut(200);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            content.ScaleTo(.9f, 1000, Easing.OutQuint);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            content.ScaleTo(1, 1000, Easing.OutElastic);
        }
    }
}

public enum ScoreListType
{
    Local,
    Global,
    Country,
    Friends,
    Club
}
