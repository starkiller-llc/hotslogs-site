// Copyright (c) Starkiller LLC. All rights reserved.

using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.Auth;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.MigrationControllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class ReputationController : ControllerBase
{
    private readonly DateTime _betaEnd = new(2020, 9, 1);
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HeroesdataContext _dc;

    public ReputationController(UserManager<ApplicationUser> userManager, HeroesdataContext dc)
    {
        _userManager = userManager;
        _dc = dc;
    }

    [HttpPost("VoteDown")]
    public ActionResult VoteDown([FromQuery] int playerId, [FromQuery] int replayId, [FromQuery] bool flag)
    {
        return Vote(false, playerId, replayId, flag);
    }

    [HttpPost("VoteUp")]
    public ActionResult VoteUp([FromQuery] int playerId, [FromQuery] int replayId, [FromQuery] bool flag)
    {
        return Vote(true, playerId, replayId, flag);
    }

    private ActionResult Vote(bool up, int playerId, int replayId, bool flag)
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Forbid();
            //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Must be logged in");
        }

        var userId = _userManager.GetUserId(User);
        var authPlayer = (from p in _dc.Players
            from u in p.Net48Users
            where u.Id.ToString() == userId
            select p).SingleOrDefault();
        if (authPlayer == null)
        {
            return Forbid();
            //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Must be logged in");
        }

        if (authPlayer.PlayerId == playerId)
        {
            return Problem(detail: "Cannot vote for self", statusCode: 422);
            //return Request.CreateErrorResponse((HttpStatusCode)422, "Cannot vote for self");
        }

        var participated =
            _dc.ReplayCharacters.Any(x => x.ReplayId == replayId && x.PlayerId == authPlayer.PlayerId);

        if (!participated)
        {
            return Problem(
                detail: "Can only vote in replays you've participated in",
                statusCode: 422);
        }

        var jointGames = (from x in _dc.ReplayCharacters
            where new[] { playerId, authPlayer.PlayerId }.Contains(x.PlayerId)
            group x by x.ReplayId
            into grp1
            where grp1.Count() == 2
            select grp1.Key).Count();

        var friends = jointGames >= 15;

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
            selfRep = new Reputation
            {
                PlayerId = authPlayer.PlayerId,
                Reputation1 = 0,
            };
            _dc.Reputations.Add(selfRep);
        }

        if (rep == null)
        {
            rep = new Reputation
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
                    return Problem(detail: errObj.ToJson(), statusCode: 422);
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
            return Problem(detail: "Cannot undo an upvote to a friend", statusCode: 422);
            //return Request.CreateErrorResponse((HttpStatusCode)422, "Cannot undo an upvote to a friend");
        }

        if (vote == null)
        {
            vote = new Vote
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
            if (up && existingVoteUp)
            {
                vote.Up = null;
            }
            else if (!up && existingVoteDown)
            {
                vote.Up = null;
            }
        }

        _dc.SaveChanges();

        var json = JsonConvert.SerializeObject(
            new
            {
                Up = upResult,
                Down = downResult,
                VotingPlayer = authPlayer.PlayerId,
                TargetPlayer = playerId,
                SelfRep = selfRep.Reputation1,
                TargetRep = rep.Reputation1,
            });

        return Content(json, "application/json");
    }
}
