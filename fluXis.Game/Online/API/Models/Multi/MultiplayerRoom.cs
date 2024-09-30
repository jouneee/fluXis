using System.Collections.Generic;
using fluXis.Shared.Components.Maps;
using fluXis.Shared.Components.Multi;
using fluXis.Shared.Components.Users;
using fluXis.Shared.Scoring;

namespace fluXis.Game.Online.API.Models.Multi;

public class MultiplayerRoom : IMultiplayerRoom
{
    public long RoomID { get; init; }
    public IMultiplayerRoomSettings Settings { get; init; } = new MultiplayerRoomSettings();
    public APIUser Host { get; set; } = null!;
    public List<IMultiplayerParticipant> Participants { get; init; } = new();
    public APIMap Map { get; set; } = null!;

    public List<ScoreInfo> Scores { get; set; } = new();
}
