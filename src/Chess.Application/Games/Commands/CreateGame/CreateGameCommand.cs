using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="WhitePlayerId"></param>
    public record CreateGameCommand(string WhitePlayerId) : IRequest<Result<CreateGameDto>>;
}
