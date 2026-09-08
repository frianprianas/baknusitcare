using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BaknusITCare.Data;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public class KnowledgeService : IKnowledgeService
    {
        private readonly ApplicationDbContext _dbContext;

        public KnowledgeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<KnowledgeArticle>> GetArticlesAsync(string? category = null, string? search = null)
        {
            var query = _dbContext.KnowledgeArticles.Where(a => a.IsPublished).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a => 
                    a.Title.ToLower().Contains(search) || 
                    a.Summary.ToLower().Contains(search) || 
                    a.Content.ToLower().Contains(search)
                );
            }

            return await query.OrderByDescending(a => a.HelpfulVotes).ThenByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<KnowledgeArticle?> GetArticleByIdAsync(int id)
        {
            var article = await _dbContext.KnowledgeArticles.FindAsync(id);
            if (article != null)
            {
                article.Views++;
                await _dbContext.SaveChangesAsync();
            }
            return article;
        }

        public async Task<KnowledgeArticle> CreateArticleAsync(KnowledgeArticle article)
        {
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            await _dbContext.KnowledgeArticles.AddAsync(article);
            await _dbContext.SaveChangesAsync();
            return article;
        }

        public async Task<bool> IncrementHelpfulVoteAsync(int id)
        {
            var article = await _dbContext.KnowledgeArticles.FindAsync(id);
            if (article == null) return false;

            article.HelpfulVotes++;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
