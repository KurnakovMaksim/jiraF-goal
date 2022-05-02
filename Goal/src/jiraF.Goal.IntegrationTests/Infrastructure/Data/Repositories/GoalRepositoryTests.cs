﻿using jiraF.Goal.API.Contracts;
using jiraF.Goal.API.Domain;
using jiraF.Goal.API.Infrastructure.Data.Contexts;
using jiraF.Goal.API.Infrastructure.Data.Entities;
using jiraF.Goal.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace jiraF.Goal.IntegrationTests.Infrastructure.Data.Repositories
{
    public class GoalRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly IGoalRepository _goalRepository;
        public GoalRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _dbContext.Goals.AddRange(new List<GoalEntity>
            {
                new GoalEntity { Title = "Title1", DateOfCreate = DateTime.UtcNow, Description = "Desc1" },
                new GoalEntity { Title = "Title2", DateOfCreate = DateTime.UtcNow, Description = "Desc2" },
                new GoalEntity { Title = "Title3", DateOfCreate = DateTime.UtcNow, Description = "Desc3" },
            });
            _dbContext.SaveChanges();
            _goalRepository = new GoalRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.DisposeAsync();
        }

        [Fact]
        public async Task GetAsync_CanGetAllGoals_EnumerableOfGoalModel()
        {
            // Act
            IEnumerable<GoalModel> result = await _goalRepository.GetAsync();

            // Assert
            Assert.NotNull(result.ToList());
            Assert.True(result.Any());
            Assert.Equal(_dbContext.Goals.Count(), result.Count());
        }
    }
}
