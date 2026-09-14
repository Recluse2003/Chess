using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence
{
    public class ChessDbContext : DbContext
    {
        public DbSet<ChessGameEntity> ChessGames => Set<ChessGameEntity>();

        public DbSet<MoveEntity> Moves => Set<MoveEntity>();

        public ChessDbContext(DbContextOptions<ChessDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChessGameEntity>(entity =>
            {
                entity.HasKey(game => game.Id);

                entity.Property(game => game.InitialFen)
                    .IsRequired();

                entity.Property(game => game.Fen)
                    .IsRequired();

                entity.Property(game => game.Status)
                    .HasConversion<string>();

                entity.Property(game => game.EndReason)
                    .HasConversion<string>();

                entity.HasMany(game => game.Moves)
                    .WithOne(move => move.Game)
                    .HasForeignKey(move => move.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MoveEntity>(entity =>
            {
                entity.HasKey(move => move.Id);

                entity.Property(move => move.From)
                    .HasMaxLength(2)
                    .IsRequired();

                entity.Property(move => move.To)
                    .HasMaxLength(2)
                    .IsRequired();

                entity.Property(move => move.FenAfterMove)
                    .IsRequired();

                entity.HasIndex(move => new
                {
                    move.GameId,
                    move.MoveNumber
                })
                .IsUnique();
            });
        }
    }
}
