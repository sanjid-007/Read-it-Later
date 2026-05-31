using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTOs;

namespace Business.Interfaces
{
    public interface IParseArticleMetadata
    {
         Task<ArticleResponse> ParseMetadata(string url);
    }
}