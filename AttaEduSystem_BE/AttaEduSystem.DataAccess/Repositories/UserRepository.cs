﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserWithDetailsAsync(string userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<(List<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            string? status = null)
        {
            var query = _context.Users.AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(u => u.Status == status);
            }

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                var keyword = filterQuery.Trim().ToLowerInvariant();
                query = filterOn.ToLowerInvariant() switch
                {
                    "email" => query.Where(u => u.Email !=null && u.Email.ToLower().Contains(keyword)),
                    "fullname" => query.Where(u => u.FullName.ToLower().Contains(keyword)),
                    "phonenumber" => query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)),
                    "keyword" => query.Where(u =>
                        (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                        u.FullName.ToLower().Contains(keyword) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword))),
                    _ => query
                };
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = (sortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "email_desc" => query.OrderByDescending(u => u.Email),
                "email" => query.OrderBy(u => u.Email),
                "fullname_desc" => query.OrderByDescending(u => u.FullName),
                "fullname" => query.OrderBy(u => u.FullName),
                _ => query.OrderBy(u => u.Email)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
