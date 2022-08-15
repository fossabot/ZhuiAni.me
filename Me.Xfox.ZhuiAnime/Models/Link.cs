using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Me.Xfox.ZhuiAnime.Models;

public class Link
{
    public uint Id { get; set; }
    public Uri Address { get; set; } = new Uri("invalid://");
    public string Annotation { get; set; } = string.Empty;

    public class LinkConfiguration : IEntityTypeConfiguration<Link>
    {
        public void Configure(EntityTypeBuilder<Link> builder)
        {
        }
    }
}

public class AnimeLink : Link
{
    public Anime? Anime { get; set; }
    public uint? AnimeId { get; set; }

    public class AnimeLinkConfiguration : IEntityTypeConfiguration<AnimeLink>
    {
        public void Configure(EntityTypeBuilder<AnimeLink> builder)
        {
            builder.HasOne(e => e.Anime)
                .WithMany(e => e.Links);
        }
    }
}

public class EpisodeLink : Link
{
    public Episode? Episode { get; set; }
    public uint? EpisodeId { get; set; }

    public class EpisodeLinkConfiguration : IEntityTypeConfiguration<EpisodeLink>
    {
        public void Configure(EntityTypeBuilder<EpisodeLink> builder)
        {
            builder.HasOne(e => e.Episode)
                .WithMany(e => e.Links);
        }
    }
}