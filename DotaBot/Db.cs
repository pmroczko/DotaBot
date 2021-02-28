﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;

namespace DotaBot
{
    // In order to do the migration - in Package Manager Console:
    // !!!!!(uncomment string in this file here before - loading connection string from app.config doesn't work for some reason)!!!!!
    // Add-Migration <Name>
    // Update-Database
    public class Db : DbContext
    {
        public Db(string connnection_string) : base()
        {
            this.connection_string = connnection_string;
        }

        public Db() : this(Configuration.GetConnectionString("DBModel")) {
            connection_string = Configuration.GetConnectionString("DBModel");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connection_string == "IN_MEMORY")
            {
                // https://docs.microsoft.com/en-us/ef/core/testing/ : Approach 3
                // It is not a relational database.
                // It doesn't support transactions.
                // It cannot run raw SQL queries.
                // It is not optimized for performance.
                optionsBuilder.UseInMemoryDatabase("InMemoryDb")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }
            else
            {
                // TODO: use below for Update-Database
                //var connection_string = "data source=SPAWEK-LEGION\\SQLEXPRESS;initial catalog=ProductComparerDB;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;MultipleActiveResultSets=True";
                optionsBuilder.UseSqlServer(connection_string);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DotaBotGame>()
                .Property(e => e.Players)
                .HasConversion(
                    v => string.Join(",SEPARATOR,", v),
                    v => v.Split(",SEPARATOR,", StringSplitOptions.RemoveEmptyEntries));

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<DotaBotGame> DotaBotGames { get; set; }

        private string connection_string;
    }

    public class DotaBotGame
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public string[] Players { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Time}: {GuildId}: {ChannelId}: ({String.Join(", ", Players)})";
        }
    }
}