using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Security.AuthorizationToken.Storage
{
    public class OcelotSecurityStorageDbContent : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        public OcelotSecurityStorageDbContent(ILoggerFactory loggerFactory, DbContextOptions<OcelotSecurityStorageDbContent> option) : base(option)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(this._loggerFactory);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorizationToken>(b =>
            {
                b.ToTable("Ocelot_SecurityToken");
                b.HasKey(f => f.Id);
                b.Property(f => f.Token).HasMaxLength(2000).IsRequired();
                b.Property(f => f.WarnInfo).HasMaxLength(1000);
                b.Property(f => f.Expiration).IsRequired();
                b.Property(f => f.AddTime).IsRequired();
            });
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<AuthorizationToken> AuthorizationTokens { set; get; }
    }

}
