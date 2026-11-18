using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeRuns;

public sealed class SocialChallengeSolver
{
    // Returns the new run when the player starts one, or null when the player joins the active run.
    public SocialChallengeRun? Activate(
        SocialChallenge challenge,
        SocialChallengeRun? activeRun,
        SocialChallengeRun? solvedRun,
        Guid playerId,
        double latitude,
        double longitude,
        DateTime now)
    {
        if (challenge.Status != SocialChallengeStatus.Published)
        {
            throw new DomainException("Only a published social challenge can be activated.");
        }
        if (solvedRun is not null)
        {
            throw new DomainException("You have already solved this social challenge.");
        }
        if (!challenge.IsWithinRange(latitude, longitude))
        {
            throw new DomainException("You are outside the social challenge range. Come closer to activate it.");
        }
        if (!challenge.IsOpenAt(now))
        {
            throw new DomainException("The social challenge time window is not open now.");
        }

        if (activeRun is not null && activeRun.Status == SocialChallengeRunStatus.Active && activeRun.IsExpiredAt(now))
        {
            activeRun.Expire(now);
        }
        if (activeRun is not null && activeRun.Status == SocialChallengeRunStatus.Active)
        {
            activeRun.Join(playerId, now);
            return null;
        }

        return new SocialChallengeRun(challenge.Id, playerId, now, challenge.Window.EndAfter(now));
    }

    // The challenge state is not checked here: a run started while the challenge was published lasts until it ends or is solved.
    public void Check(SocialChallenge challenge, SocialChallengeRun run, Guid playerId, double? latitude, double? longitude, DateTime now)
    {
        if (run.Status == SocialChallengeRunStatus.Solved && run.HasParticipant(playerId))
        {
            return;
        }
        if (!run.HasParticipant(playerId))
        {
            throw new DomainException("You are not in this run. Activate the challenge again to join it.");
        }
        if (run.IsExpiredAt(now))
        {
            if (run.Status == SocialChallengeRunStatus.Active)
            {
                run.Expire(now);
            }
            return;
        }

        if (latitude is null || longitude is null || !challenge.IsWithinRange(latitude.Value, longitude.Value))
        {
            run.Remove(playerId);
        }
        else
        {
            run.RecordCheck(playerId, now);
        }
        run.RemoveInactive(now);
        run.Solve(challenge.Xp, challenge.RequiredParticipants, now);
    }
}
