using Microsoft.EntityFrameworkCore;

namespace Me.Xfox.ZhuiAnime;

public partial class ZAContext : DbContext
{
    public DbSet<Modules.TorrentDirectory.Torrent> TorrentDirectoryTorrent { get; set; } = null!;
}
