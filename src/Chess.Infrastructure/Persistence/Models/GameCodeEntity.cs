using Chess.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Represents a lobby join code associated with a <see cref="Domain.Entities.ChessGame"/>.
    /// </summary>
    public class GameCodeEntity
    {
        public string Code { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid ChessGameId { get; set; }
        public ChessGameEntity ChessGame { get; set; } = null!;

        /// <summary>
        /// Contains the characters that can be used when generating lobby codes. <c>I</c>, <c>O</c>, 
        /// <c>0</c>, and <c>1</c>, are excluded, due to how easily they can be confused with each other. 
        /// </summary>
        private static readonly char[] Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        /// <summary>
        /// Generates a random lobby code.
        /// </summary>
        /// <param name="length">
        /// The number of characters in the generated code. Defaults is set to 5.
        /// </param>
        /// <returns>
        /// A randomly generated lobby code containing the specified number of characters.
        /// </returns>
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
