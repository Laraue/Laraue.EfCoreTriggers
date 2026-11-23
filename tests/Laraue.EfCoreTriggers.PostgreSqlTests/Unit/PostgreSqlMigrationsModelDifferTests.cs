using System;
using Laraue.EfCoreTriggers.Common.Extensions;
using Laraue.EfCoreTriggers.Tests;
using Laraue.EfCoreTriggers.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Laraue.EfCoreTriggers.PostgreSqlTests.Unit
{
    [Collection(CollectionNames.PostgreSql)]
    public class PostgreSqlMigrationsModelDifferTests
    {
        [Fact]
        public void HasDifferences_IsTrue_WhenOnlyATriggerWasAdded()
        {
            var withoutTrigger = CreateContext(_ => { });
            var withTrigger = CreateContext(AddInsertTrigger);
            var differ = withTrigger.GetService<IMigrationsModelDiffer>();

            Assert.NotEmpty(differ.GetDifferences(withoutTrigger.GetRelationalModel(), withTrigger.GetRelationalModel()));
            Assert.True(differ.HasDifferences(withoutTrigger.GetRelationalModel(), withTrigger.GetRelationalModel()));
        }

        [Fact]
        public void HasDifferences_IsTrue_WhenOnlyATriggerWasRemoved()
        {
            var withTrigger = CreateContext(AddInsertTrigger);
            var withoutTrigger = CreateContext(_ => { });
            var differ = withoutTrigger.GetService<IMigrationsModelDiffer>();

            Assert.NotEmpty(differ.GetDifferences(withTrigger.GetRelationalModel(), withoutTrigger.GetRelationalModel()));
            Assert.True(differ.HasDifferences(withTrigger.GetRelationalModel(), withoutTrigger.GetRelationalModel()));
        }

        [Fact]
        public void HasDifferences_IsFalse_WhenTheTriggersAreTheSame()
        {
            var first = CreateContext(AddInsertTrigger);
            var second = CreateContext(AddInsertTrigger);
            var differ = second.GetService<IMigrationsModelDiffer>();

            Assert.Empty(differ.GetDifferences(first.GetRelationalModel(), second.GetRelationalModel()));
            Assert.False(differ.HasDifferences(first.GetRelationalModel(), second.GetRelationalModel()));
        }

        private static void AddInsertTrigger(EntityTypeBuilder<SourceEntity> entity) => entity
            .AfterInsert(trigger => trigger
                .Action(action => action
                    .Insert(inserted => new DestinationEntity { StringField = "12" })));

        private static DynamicDbContext CreateContext(Action<EntityTypeBuilder<SourceEntity>> setupTrigger)
        {
            var options = new ContextOptionsFactory<DynamicDbContext>().CreateDbContextOptions();

            return new DynamicDbContext(options, modelBuilder => setupTrigger(modelBuilder.Entity<SourceEntity>()));
        }
    }

    internal static class DbContextRelationalModelExtensions
    {
        public static IRelationalModel GetRelationalModel(this DbContext context) =>
            context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
    }
}
