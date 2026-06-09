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
        private readonly IControlArticleMetadata _controlArticleMetadata;

        public ArticlesController( IControlArticleMetadata controlArticleMetadata)
        {
            _controlArticleMetadata = controlArticleMetadata;
        }
       [HttpPost]
       public async Task<IActionResult> SaveArticle([FromBody] ArticleDto articleDto)
       {
           await _controlArticleMetadata.SaveArticleAsync(articleDto);

           return Ok("Article saved successfully.");
       }

       [HttpGet("{id}")]
       public async Task<IActionResult> GetArticle(int id)
       {
           var articleResponse = await _controlArticleMetadata.GetArticleAsync(id);
           if (articleResponse == null)
           {
               return NotFound("Article not found.");
           }

           return Ok(articleResponse);
       }
    }
}