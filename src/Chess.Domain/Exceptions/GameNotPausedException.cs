namespace Chess.Domain.Exceptions
{
    public class GameNotPausedException : DomainException
    {
        public GameNotPausedException() : base("The chess game is not currently in a paused state.") { }
    }
}
