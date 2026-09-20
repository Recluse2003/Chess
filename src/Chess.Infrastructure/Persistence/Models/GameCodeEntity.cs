using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Infrastructure.Persistence.Models
{
    public class GameCodeEntity
    {
        public string Code { get; set; } = null!; // Primary Key, e.g., "X7K9B"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid ChessGameId { get; set; }     // Foreign Key
        public ChessGameEntity ChessGame { get; set; } = null!;

        // Static helper to generate a unique code structure
        private static readonly char[] Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        public static string Generate(int length = 5)
        {
            return string.Create(length, Random.Shared, (span, rand) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = Chars[rand.Next(Chars.Length)];
                }
            });
        }
    }
}
