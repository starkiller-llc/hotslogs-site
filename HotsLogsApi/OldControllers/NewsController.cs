using Heroes.DataAccessLayer.Data;
using HotsLogsApi.MigrationControllers;
using HotsLogsApi.OldControllers.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.OldControllers;

[Route("[controller]")]
[Migration]
public class NewsController : ControllerBase
{
    private readonly HeroesdataContext _dc;

    public NewsController(HeroesdataContext dc)
    {
        _dc = dc;
    }

    [HttpGet]
    public IEnumerable<NewsItem> Get([FromQuery] string tags, [FromQuery] int? maxEntries = null)
    {
        var tagArray = tags.Split(',').Select(x => $"@{x}@").ToArray();
        var postsInitial = (from p in _dc.BlogPosts
            where !p.ExpireDate.HasValue || p.ExpireDate > DateTime.UtcNow
            where p.Tags.Contains(tagArray[0])
            select p).ToList();
        var posts = (from p in postsInitial
            where tagArray.All(p.Tags.Contains)
            orderby p.Priority, p.CreateDate
            select new NewsItem
            {
                Html = p.Content ?? string.Empty,
            }).ToList();

        return maxEntries.HasValue
            ? posts.AsEnumerable().Reverse().Take(maxEntries.Value).Reverse().ToList()
            : posts.ToList();
    }
}
