using System.Collections.Generic;
using System.Threading.Tasks;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public interface IKnowledgeService
    {
        Task<List<KnowledgeArticle>> GetArticlesAsync(string? category = null, string? search = null);
        Task<KnowledgeArticle?> GetArticleByIdAsync(int id);
        Task<KnowledgeArticle> CreateArticleAsync(KnowledgeArticle article);
        Task<bool> IncrementHelpfulVoteAsync(int id);
    }
}
