using fluXis.Shared.Components.Maps;
using fluXis.Shared.Components.Scores;
using fluXis.Shared.Utils;
using Newtonsoft.Json;

namespace fluXis.Shared.API.Responses.Maps;

public class MapLeaderboardClubs
{
    [JsonProperty("map")]
    public APIMap Map { get; set; } = null!;

    [JsonProperty("scores")]
    public List<APIClubScore> Scores { get; set; } = new();

    public MapLeaderboardClubs(APIMap map, IEnumerable<APIClubScore> scores)
    {
        Map = map;
        Scores = scores.ToList();
    }

    [JsonConstructor]
    [Obsolete(JsonUtils.JSON_CONSTRUCTOR_ERROR, true)]
    public MapLeaderboardClubs()
    {
    }
}
