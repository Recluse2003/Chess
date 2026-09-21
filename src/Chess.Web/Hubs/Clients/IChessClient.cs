using Chess.Application.Games.Commands.MakeMove;

namespace Chess.Web.Hubs.Clients
{
    public interface IChessClient
    {
        Task PlayerJoined(bool IsWhitePlayer, string playerUsername);
        Task MoveMade(MoveResultDto result);
    }
}
