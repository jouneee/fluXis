using fluXis.Shared.Components.Maps;

namespace fluXis.Game.Online.API.Requests.MapSets;

public class MapSetRequest : APIRequest<APIMapSet>
{
    protected override string Path => $"/mapset/{id}";

    private long id { get; }

    public MapSetRequest(long id)
    {
        this.id = id;
    }
}
