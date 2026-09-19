using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.Exceptions
{
    public class GameStateTransitionException : DomainException
    {
        public GameStateTransitionException(string message) : base(message) { }

        public static GameStateTransitionException NotAcceptingPlayers()
            => new("The game is not currently accepting new players.");

        public static GameStateTransitionException OpponentAlreadyJoined()
            => new("The game already contains an opponent.");

        public static GameStateTransitionException MatchAlreadyStarted()
            => new("The game has already been started.");

        public static GameStateTransitionException OpponentMissing()
            => new("The game cannot start without an opposing player.");

        public static GameStateTransitionException NotTheHost()
            => new("Only the host player is authorized to start the match.");
    }
}
