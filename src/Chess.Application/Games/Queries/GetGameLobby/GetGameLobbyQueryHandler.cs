

using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameLobby
{
    /// <summary>
    /// Handles the execution of a <see cref="GetGameLobbyQuery"/> request. 
    /// Validates that user requesting the chess lobby details has the permissions to do so, and that
    /// the game exists, and isn't already active.
    /// </summary>
    public class GetGameLobbyQueryHandler : IRequestHandler<GetGameLobbyQuery, Result<GameLobbyDto>>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly IGameCodeRepository _codeRepository;
        private readonly IUserRepository _userRepository;

        public GetGameLobbyQueryHandler(
            IChessGameRepository gameRepository,
            IGameCodeRepository codeRepository,
            IUserRepository userRepository)
        {
            _gameRepository = gameRepository;
            _codeRepository = codeRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetGameLobbyQuery"/>. Ensures the <see cref="ChessGame"/> that
        /// the user is attempting to retrieve lobby details for exists, isn't already active, and that the
        /// user is actually allowed to.
        /// </summary>
        /// <param name="query">The details required to retrieve the chess lobby details.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A successful query will provide a <see cref="Result"/> stating so, and contains a 
        /// <see cref="GameLobbyDto"/> which contains the required lobby details. A failed query will provide a 
        /// <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result<GameLobbyDto>> Handle(GetGameLobbyQuery query, CancellationToken cancellationToken)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(query.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            bool isPlayer = chessGame.WhitePlayerId == query.UserId ||
                            chessGame.BlackPlayerId == query.UserId;

            if (!isPlayer)
                return Error.Unauthorized("Games.NotPlayer", "You are not a player in this game.");

            string? joinCode = await _codeRepository.GetCodeByGameIdAsync(query.GameId);

            if (joinCode == null)
                return Error.NotFound("Games.GameAlreadyStarted", "The game is no longer in the lobby.");

            string? whiteUsername = await _userRepository.GetUsernameByIdAsync(chessGame.WhitePlayerId);

            string? blackUsername = null;

            if (chessGame.BlackPlayerId != null)
                blackUsername = await _userRepository.GetUsernameByIdAsync(chessGame.BlackPlayerId);

            return new GameLobbyDto
            {
                GameId = chessGame.Id,
                JoinCode = joinCode,
                IsWhitePlayer = chessGame.WhitePlayerId == query.UserId, 
                WhitePlayerUsername = whiteUsername!,
                BlackPlayerUsername = blackUsername
            };
        }
    }
}
