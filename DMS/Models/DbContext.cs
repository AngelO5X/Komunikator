using Microsoft.EntityFrameworkCore;

public class DmsDbContext : DbContext
{
    
    public DmsDbContext(DbContextOptions<DmsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserInfo> UserInfos { get; set; }
    public DbSet<PrivateMessage> PrivateMessages { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    //     => options.UseSqlite("Data Source=speaknow.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Info)
            .WithOne(i => i.User)
            .HasForeignKey<UserInfo>(i => i.UUID);

        modelBuilder.Entity<PrivateMessage>()
            .ToTable(t => t.HasCheckConstraint("CK_Message_NotEmpty", "trim(Content) <> ''"));

        modelBuilder.Entity<User>().ToTable("Użytkownicy");
        modelBuilder.Entity<UserInfo>().ToTable("Info_Użytkownicy");
        modelBuilder.Entity<PrivateMessage>().ToTable("Wiadomości_Prywatne");
    }
}