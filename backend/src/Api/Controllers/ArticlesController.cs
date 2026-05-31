using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Business.Interfaces;
using Business.DTOs;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IParseArticleMetadata _parseArticleMetadata;

        public ArticlesController( IParseArticleMetadata parseArticleMetadata)
        {
            _parseArticleMetadata = parseArticleMetadata;
        }
       [HttpPost]
       public async Task<IActionResult> SaveArticle([FromBody] ArticleDto articleDto)
       {
           var articleResponse = await _parseArticleMetadata.ParseMetadata(articleDto.Url);
           if (articleResponse == null)
           {
               return BadRequest("Failed to parse article metadata.");
           }
           return Ok(articleResponse);
       }

    }
}