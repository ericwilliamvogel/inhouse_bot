﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using db;
using System;

namespace Migrations.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("db.DataSet", b =>
            {
                b.Property<int>("_id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("_name")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("_desc")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("_id");

                b.ToTable("primary_table");
            });

            modelBuilder.Entity("db.EmoteUnlockedData", b =>
            {
                b.Property<int>("_id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<ulong>("_playerid")
                    .HasColumnType("NUMERIC(20,0)");


                b.Property<int>("_emoteid")
                    .HasColumnType("int");

                //b.HasKey("_increment");

                b.ToTable("emote_unlocked");
            });

            modelBuilder.Entity("db.PlayerData", b =>
            {
                b.Property<ulong>("_id")
                                    .ValueGeneratedOnAdd()
                    .HasColumnType("NUMERIC(20,0)")
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<long>("_steamid")
                    .HasColumnType("BIGINT");


                b.Property<int>("_ihlmmr")
                    .HasColumnType("int");

                b.Property<int>("_gameslost")
    .HasColumnType("int");

                b.Property<int>("_gameswon")
    .HasColumnType("int");

                b.Property<int>("_dotammr")
.HasColumnType("int");

                b.Property<int>("_adjmmr")
.HasColumnType("int");

                b.Property<int>("_status")
.HasColumnType("int");

                b.Property<int>("_gamestatus")
.HasColumnType("int");

                b.Property<int>("_region")
.HasColumnType("int");

                b.Property<int>("_role1")
.HasColumnType("int");

                b.Property<int>("_role2")
.HasColumnType("int");

                b.Property<long>("_xp")
.HasColumnType("bigint");

                b.Property<int>("_totalgames")
.HasColumnType("int");



                b.HasKey("_id");

                b.ToTable("player_data");
            });

            modelBuilder.Entity("db.QueueData", b =>
            {
                b.Property<ulong>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("NUMERIC(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);


                b.Property<DateTimeOffset>("_start")
        .HasColumnType("datetimeoffset");

                b.Property<int>("_position")
        .HasColumnType("int");

                b.HasKey("_id");

                b.ToTable("player_queue");
            });

            modelBuilder.Entity("db.LobbyPool", b =>
            {
                b.Property<int>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("NUMERIC(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<int>("_gameid")
.HasColumnType("int");

                b.Property<ulong>("_discordid")
.HasColumnType("NUMERIC(20,0)");

                b.HasKey("_id");

                b.ToTable("lobby_pool");
            });

            modelBuilder.Entity("db.SpectatorQueueData", b =>
            {
                b.Property<ulong>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("NUMERIC(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
                b.Property<DateTimeOffset>("_start")
        .HasColumnType("datetimeoffset");

                b.Property<int>("_position")
        .HasColumnType("int");

                b.HasKey("_id");

                b.ToTable("spectator_queue");
            });

            modelBuilder.Entity("db.CasterQueueData", b =>
            {
                b.Property<ulong>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("NUMERIC(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
                b.Property<DateTimeOffset>("_start")
        .HasColumnType("datetimeoffset");

                b.Property<int>("_position")
                        .HasColumnType("int");

                b.HasKey("_id");

                b.ToTable("caster_queue");
            });


            modelBuilder.Entity("db.TeamRecord", b =>
            {
                b.Property<int>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<int>("_side")
                        .HasColumnType("int");

                b.Property<int>("_canpick")
.HasColumnType("int");

                b.Property<int>("_gameid")
        .HasColumnType("int");

                b.Property<int>("_onwin")
.HasColumnType("INT");

                b.Property<int>("_onlose")
.HasColumnType("INT");

                b.Property<ulong>("_p1")
        .HasColumnType("NUMERIC(20,0)");

                b.Property<ulong>("_p2")
.HasColumnType("NUMERIC(20,0)");

                b.Property<ulong>("_p3")
.HasColumnType("NUMERIC(20,0)");

                b.Property<ulong>("_p4")
.HasColumnType("NUMERIC(20,0)");

                b.Property<ulong>("_p5")
.HasColumnType("NUMERIC(20,0)");

                b.HasKey("_id");

                b.ToTable("game_record");
            });
            /*modelBuilder.Entity<TeamRecord>(entity =>
            {
                entity.HasKey(e => new { e._id, e._side });
            });*/
            modelBuilder.Entity("db.GameData", b =>
            {
                b.Property<int>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<ulong>("_host")
.HasColumnType("NUMERIC(20,0)");

                b.Property<ulong>("_steamid")
.HasColumnType("BIGINT");



                b.Property<int>("_winner")
.HasColumnType("INT");


                b.HasKey("_id");

                b.ToTable("game_data");
            });

            modelBuilder.Entity("db.ChannelInfo", b =>
            {
                b.Property<ulong>("_id")
                        .ValueGeneratedOnAdd()
        .HasColumnType("NUMERIC(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<int>("_number")
.HasColumnType("int");

                b.Property<int>("_gameid")
.HasColumnType("int");

                b.Property<ulong>("_meessageid")
.HasColumnType("NUMERIC(20,0)");


                b.HasKey("_id");

                b.ToTable("discord_channel_info");
            });
            /*
            modelBuilder.Entity("db.PlayerAvailability", b =>
            {

            });

            modelBuilder.Entity("db.RoomAvailability", b =>
            {

            });*/
#pragma warning restore 612, 618
        }
    }
}
