using System.Linq;
using fluXis.Game.Graphics.Background;
using fluXis.Game.Graphics.UserInterface.Panel;
using fluXis.Game.Map;
using fluXis.Game.Screens;
using fluXis.Game.Screens.Edit;
using osu.Framework.Allocation;

namespace fluXis.Game.Tests.Edit;

public partial class TestEditor : FluXisTestScene
{
    [Resolved]
    private MapStore maps { get; set; }

    protected override bool UseTestAPI => true;

    [BackgroundDependencyLoader]
    private void load()
    {
        CreateClock();

        var backgrounds = new GlobalBackground();
        TestDependencies.CacheAs(backgrounds);

        var screenStack = new FluXisScreenStack();
        TestDependencies.CacheAs(screenStack);

        var panels = new PanelContainer();
        TestDependencies.CacheAs(panels);

        Add(GlobalClock);
        Add(backgrounds);
        Add(screenStack);
        Add(panels);

        AddStep("Push existing map", () =>
        {
            var map = maps.GetFromGuid("262a7734-95a5-4115-85cf-87c898e55db6")?
                .Maps.FirstOrDefault();

            var loader = map is not null ? new EditorLoader(map, map.GetMapInfo()) : new EditorLoader();
            loader.StartTabIndex = 3;
            screenStack.Push(loader);
        });

        AddStep("Push new map", () => screenStack.Push(new EditorLoader()));
    }
}
