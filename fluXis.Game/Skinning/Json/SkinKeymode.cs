using System.Collections.Generic;
using Newtonsoft.Json;

namespace fluXis.Game.Skinning.Json;

public class SkinKeymode
{
    [JsonProperty("column_width")]
    public int ColumnWidth { get; set; } = 114;

    [JsonProperty("hit_position")]
    public int HitPosition { get; set; } = 130;

    [JsonProperty("colors")]
    public List<string> Colors { get; set; } = new();
}
