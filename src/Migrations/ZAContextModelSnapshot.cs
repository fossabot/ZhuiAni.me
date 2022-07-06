// <auto-generated />
using System;
using Me.Xfox.ZhuiAnime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Me.Xfox.ZhuiAnime.Migrations
{
    [DbContext(typeof(ZAContext))]
    partial class ZAContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Anime", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("BangumiLink")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("bangumi_link");

                    b.Property<byte[]>("Image")
                        .HasColumnType("bytea")
                        .HasColumnName("image");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_anime");

                    b.HasIndex("BangumiLink")
                        .IsUnique()
                        .HasDatabaseName("ix_anime_bangumi_link");

                    b.ToTable("anime", (string)null);
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Episode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AnimeId")
                        .HasColumnType("bigint")
                        .HasColumnName("anime_id");

                    b.Property<string>("BangumiLink")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("bangumi_link");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_episode");

                    b.HasIndex("AnimeId")
                        .HasDatabaseName("ix_episode_anime_id");

                    b.HasIndex("BangumiLink")
                        .IsUnique()
                        .HasDatabaseName("ix_episode_bangumi_link");

                    b.ToTable("episode", (string)null);
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Link", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("address");

                    b.Property<string>("Annotation")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("annotation");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("discriminator");

                    b.HasKey("Id")
                        .HasName("pk_link");

                    b.ToTable("link", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("Link");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.AnimeLink", b =>
                {
                    b.HasBaseType("Me.Xfox.ZhuiAnime.Models.Link");

                    b.Property<long?>("AnimeId")
                        .HasColumnType("bigint")
                        .HasColumnName("anime_id");

                    b.HasIndex("AnimeId")
                        .HasDatabaseName("ix_link_anime_id");

                    b.HasDiscriminator().HasValue("AnimeLink");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.EpisodeLink", b =>
                {
                    b.HasBaseType("Me.Xfox.ZhuiAnime.Models.Link");

                    b.Property<long?>("EpisodeId")
                        .HasColumnType("bigint")
                        .HasColumnName("episode_id");

                    b.HasIndex("EpisodeId")
                        .HasDatabaseName("ix_link_episode_id");

                    b.HasDiscriminator().HasValue("EpisodeLink");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Episode", b =>
                {
                    b.HasOne("Me.Xfox.ZhuiAnime.Models.Anime", "Anime")
                        .WithMany("Episodes")
                        .HasForeignKey("AnimeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_episode_anime_anime_id");

                    b.Navigation("Anime");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.AnimeLink", b =>
                {
                    b.HasOne("Me.Xfox.ZhuiAnime.Models.Anime", "Anime")
                        .WithMany("Links")
                        .HasForeignKey("AnimeId")
                        .HasConstraintName("fk_link_anime_anime_id");

                    b.Navigation("Anime");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.EpisodeLink", b =>
                {
                    b.HasOne("Me.Xfox.ZhuiAnime.Models.Episode", "Episode")
                        .WithMany("Links")
                        .HasForeignKey("EpisodeId")
                        .HasConstraintName("fk_link_episode_episode_id");

                    b.Navigation("Episode");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Anime", b =>
                {
                    b.Navigation("Episodes");

                    b.Navigation("Links");
                });

            modelBuilder.Entity("Me.Xfox.ZhuiAnime.Models.Episode", b =>
                {
                    b.Navigation("Links");
                });
#pragma warning restore 612, 618
        }
    }
}
