using Games.Application.Challenges;
using Games.Application.Locations;

namespace Games.Application.ChallengeOverview;

// One row of the shared overview, with only the fields all three challenge types have. Location is the quiz location,
// the start of a location challenge, or the place of a social challenge, and is null for a quiz or a social challenge without one.
public sealed record ChallengeDto(
    Guid Id,
    ChallengeType Type,
    string Name,
    int Xp,
    string Status,
    LocationDto? Location);
