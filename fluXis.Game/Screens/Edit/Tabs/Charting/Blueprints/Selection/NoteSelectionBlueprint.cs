using fluXis.Game.Map.Structures;
using fluXis.Game.Map.Structures.Bases;
using fluXis.Game.Screens.Edit.Tabs.Charting.Playfield;

namespace fluXis.Game.Screens.Edit.Tabs.Charting.Blueprints.Selection;

public partial class NoteSelectionBlueprint : ChartingSelectionBlueprint
{
    public new HitObject Object => (HitObject)base.Object;

    public new EditorHitObject Drawable => (EditorHitObject)base.Drawable;

    public override double FirstComparer => Object.Time;
    public override double SecondComparer => Object.EndTime;

    public NoteSelectionBlueprint(ITimedObject info)
        : base(info)
    {
        Width = EditorHitObjectContainer.NOTEWIDTH;
    }

    protected override void Update()
    {
        base.Update();

        if (Parent != null)
            Position = Parent.ToLocalSpace(HitObjectContainer.ScreenSpacePositionAtTime(Object.Time, Object.Lane));
    }
}
