using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Vote;
using HotsLogsApi.Models;
using System;
using System.Linq;
using System.Net;

namespace HotsLogsApi.BL.Migration.Helpers;

public class VoteHelper
{
    private readonly AppUser _appUser;
    private readonly HeroesdataContext _dc;
    private readonly VoteArgs _args;
    private readonly DateTime _betaEnd = new(2020, 9, 1);

    public VoteHelper(VoteArgs args, AppUser appUser, HeroesdataContext dc)
    {
        _args = args;
        _appUser = appUser;
        _dc = dc;
    }

    public VoteResponse Vote()
    {
        var res = new VoteResponse();
        var (status, ok, bad) = VoteInternal(_args.Up, _args.PlayerId, _args.ReplayId, _args.Perform);
        if (status == HttpStatusCode.OK)
        {
            res.Success = true;
            res.Up = ok.Up;
            res.Down = ok.Down;
            res.VotingPlayer = ok.VotingPlayer;
            res.TargetPlayer = ok.TargetPlayer;
            res.SelfRep = ok.SelfRep;
            res.TargetRep = ok.TargetRep;
            res.ReplayId = ok.ReplayId;
            return res;
        }

        res.Success = false;
        res.ErrorMessage = bad.Message;
        res.NeededRep = bad.NeededRep;
        res.Rep = bad.Rep;
        return res;
    }

    private (HttpStatusCode, VoteResponse, VoteErrorResponse) VoteInternal(
        bool up,
        int playerId,
        int replayId,
        bool flag)
    {
        if (_appUser is null)
        {
            return (HttpStatusCode.Forbidden, null, VoteErrorResponse.From("Must be logged in"));
        }

        var userId = _appUser.Id;
        var authPlayer = (from p in _dc.Players
            from u in p.Net48Users
            where u.Id == userId
            select p).SingleOrDefault();
        if (authPlayer == null)
        {
            return (HttpStatusCode.Forbidden, null, VoteErrorResponse.From("Must be logged in"));
        }

        if (authPlayer.PlayerId == playerId)
        {
            return ((HttpStatusCode)422, null, VoteErrorResponse.From("Cannot vote for self"));
        }

        var participated =
            _dc.ReplayCharacters.Any(x => x.ReplayId == replayId && x.PlayerId == authPlayer.PlayerId);

        if (!participated)
        {
            return (
                (HttpStatusCode)422, null,
                VoteErrorResponse.From("Can only vote in replays you've participated in"));
        }

        var jointGames = (from x in _dc.ReplayCharacters
            where new[] { playerId, authPlayer.PlayerId }.Contains(x.PlayerId)
            group x by x.ReplayId
            into grp1
            where grp1.Count() == 2
            select grp1.Key).Count();

        var friends = jointGames >= 15;

        var response = HttpStatusCode.OK;

        var rep = _dc.Reputations.SingleOrDefault(x => x.PlayerId == playerId);
        var selfRep = _dc.Reputations.SingleOrDefault(x => x.PlayerId == authPlayer.PlayerId);
        var vote = _dc.Votes.SingleOrDefault(x => x.TargetPlayerId == playerId && x.TargetReplayId == replayId);

        var existingVoteUp = false;
        var existingVoteDown = false;

        if (vote?.Up != null)
        {
            if (vote.Up.Value != 0)
            {
                existingVoteUp = true;
            }
            else
            {
                existingVoteDown = true;
            }
        }

        if (selfRep == null)
        {
            selfRep = new Heroes.DataAccessLayer.Models.Reputation
            {
                PlayerId = authPlayer.PlayerId,
                Reputation1 = 0,
            };
            _dc.Reputations.Add(selfRep);
        }

        if (rep == null)
        {
            rep = new Heroes.DataAccessLayer.Models.Reputation
            {
                PlayerId = playerId,
                Reputation1 = 0,
            };
            _dc.Reputations.Add(rep);
        }

        bool? upResult = null;
        bool? downResult = null;
        var friendUpvoteUndo = false;

        void UndoVoteDown()
        {
            rep.Reputation1 += 10;
            selfRep.Reputation1 += 5;
            downResult = false;
        }

        void DoVoteUp()
        {
            rep.Reputation1 += friends ? 1 : 10;
            upResult = true;
        }

        void UndoVoteUp()
        {
            if (friends)
            {
                friendUpvoteUndo = true;
                return;
            }

            rep.Reputation1 -= 10;
            upResult = false;
        }

        void DoVoteDown()
        {
            rep.Reputation1 -= 10;
            selfRep.Reputation1 -= 5;
            downResult = true;
        }

        if (up)
        {
            if (flag && !existingVoteUp)
            {
                if (existingVoteDown)
                {
                    UndoVoteDown();
                }

                DoVoteUp();
            }
            else if (!flag && existingVoteUp)
            {
                UndoVoteUp();
            }
        }
        else
        {
            if (flag && !existingVoteDown)
            {
                var downvoteRepRequirement = DateTime.UtcNow < _betaEnd ? 0 : 100;
                if (selfRep.Reputation1 < downvoteRepRequirement)
                {
                    var errObj = new VoteErrorResponse
                    {
                        Message = "Not enough reputation for downvoting",
                        Rep = selfRep.Reputation1,
                        NeededRep = downvoteRepRequirement,
                    };
                    return ((HttpStatusCode)422, null, errObj);
                }

                if (existingVoteUp)
                {
                    UndoVoteUp();
                }

                DoVoteDown();
            }
            else if (!flag && existingVoteDown)
            {
                UndoVoteDown();
            }
        }

        if (friendUpvoteUndo)
        {
            return ((HttpStatusCode)422, null, VoteErrorResponse.From("Cannot undo an upvote to a friend"));
        }

        if (vote == null)
        {
            vote = new Heroes.DataAccessLayer.Models.Vote
            {
                VotingPlayerId = authPlayer.PlayerId,
                TargetPlayerId = playerId,
                TargetReplayId = replayId,
            };
            _dc.Votes.AddRange(vote);
        }

        if (flag)
        {
            vote.Up = up ? 1ul : 0ul;
        }
        else
        {
            switch (up)
            {
                case true when existingVoteUp:
                case false when existingVoteDown:
                    vote.Up = null;
                    break;
            }
        }

        _dc.SaveChanges();

        var result = new VoteResponse
        {
            ReplayId = replayId,
            Up = upResult,
            Down = downResult,
            VotingPlayer = authPlayer.PlayerId,
            TargetPlayer = playerId,
            SelfRep = selfRep.Reputation1,
            TargetRep = rep.Reputation1,
        };

        return (response, result, null);
    }
}

public class VoteErrorResponse
{
    public string Message { get; set; }
    public int Rep { get; set; }
    public int NeededRep { get; set; }

    public static VoteErrorResponse From(string msg)
    {
        return new VoteErrorResponse { Message = msg };
    }
}
