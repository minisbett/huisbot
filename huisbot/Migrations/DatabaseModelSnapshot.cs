﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using huisbot.Persistence;

#nullable disable

namespace huisbot.Migrations
{
    [DbContext(typeof(Database))]
    partial class DatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.13");

            modelBuilder.Entity("huisbot.Models.Utility.BeatmapAlias", b =>
                {
                    b.Property<string>("Alias")
                        .HasColumnType("TEXT")
                        .HasColumnName("alias");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.HasKey("Alias")
                        .HasName("pk_beatmap_aliases");

                    b.ToTable("beatmap_aliases", (string)null);
                });

            modelBuilder.Entity("huisbot.Models.Utility.OsuDiscordLink", b =>
                {
                    b.Property<ulong>("DiscordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("discord_id");

                    b.Property<int>("OsuId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("osu_id");

                    b.HasKey("DiscordId")
                        .HasName("pk_osu_discord_links");

                    b.ToTable("osu_discord_links", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
