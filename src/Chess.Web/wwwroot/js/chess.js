"use strict";

console.log("CHESS.JS LOADED");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chessHub")
    .withAutomaticReconnect([0, 5000, 15000, 30000]) // Try reconnecting instantly, then 5s, 15s, 30s
    .build();

async function startConnection() {
    try {
        await connection.start();
        console.log("Connected to ChessHub");

        await connection.invoke("JoinGame", gameId);
        console.log("Joined game:", gameId);
    }
    catch (err) {
        console.error(err);
    }
}

startConnection();

connection.onreconnecting((error) => {
    gameActive = false;

    selectedSquare = null;
    removeLegalMoves();

    const modal = document.querySelector("#game-end-modal");
    document.querySelector("#game-end-title").textContent = "Connection Lost...";
    document.querySelector("#game-end-message").textContent = "Attempting to reconnect and restore your match...";

    modal.classList.remove("hidden");
});

connection.onreconnected(async (newConnectionId) => {
    try {
        document.querySelector("#game-end-message").textContent = "Restoring game state...";

        await connection.invoke("JoinGame", gameId); 
    } catch (err) {
        console.error("Failed to automatically rejoin the game: ", err);

        document.querySelector("#game-end-title").textContent = "Rejoin Failed";
        document.querySelector("#game-end-message").textContent = "Could not synchronize with the match.";
    }
});

connection.on("GameRejoined", (viewGameDto) => {
    gameActive = true;

    selectedSquare = null;
    removeLegalMoves();

    const turnSpan = document.querySelector("#currentTurn");
    if (turnSpan) {
        turnSpan.textContent = viewGameDto.isWhiteTurn ? "White" : "Black";
    }

    const squares = document.querySelectorAll(".chess-square");
    squares.forEach(square => {
        square.innerHTML = "";
    });

    viewGameDto.pieces.forEach(piece => {
        const targetSquare = document.querySelector(
            `.chess-square[data-file="${piece.file}"][data-rank="${piece.rank}"]`
        );

        if (targetSquare) {
            const pieceElement = document.createElement("span");
            pieceElement.classList.add("chess-piece");
            pieceElement.setAttribute("data-piece", piece.piece);
            pieceElement.textContent = getPieceSymbol(piece.piece);

            targetSquare.appendChild(pieceElement);
        }
    });

    document.querySelector("#game-end-modal").classList.add("hidden");
});

connection.onclose((error) => {
    console.assert(connection.state === signalR.HubConnectionState.Disconnected);

    gameActive = false;

    const modal = document.querySelector("#game-end-modal");

    document.querySelector("#game-end-title").textContent = "You disconnected from the server";
    document.querySelector("#game-end-message").textContent = "The match has been forfeited.";

    modal.classList.remove("hidden");
});

connection.on("OpponentDisconnected", () => {
    gameActive = false;

    selectedSquare = null;
    removeLegalMoves();

    const modal = document.querySelector("#game-end-modal");

    document.querySelector("#game-end-title").textContent = "Your opponent has disconnected...";
    document.querySelector("#game-end-message").textContent = "Waiting for them to reconnect (60s)...";

    modal.classList.remove("hidden");
});

connection.on("OpponentReconnected", () => {
    gameActive = true;
    document.querySelector("#game-end-modal").classList.add("hidden");
});

connection.on("GameEndedByForfeit", (isWhiteWinner) => {
    gameActive = false;

    const iAmWhite = board.dataset.isWhitePlayer === "true"; 

    const modal = document.querySelector("#game-end-modal");
    const title = document.querySelector("#game-end-title");
    const message = document.querySelector("#game-end-message");

    const iWon = (isWhiteWinner && iAmWhite) || (!isWhiteWinner && !iAmWhite);

    if (iWon) {
        title.textContent = "Victory!";
        message.textContent = "Your opponent failed to reconnect. You win by default!";
    } else {
        title.textContent = "Match Forfeited";
        message.textContent = "You were disconnected for too long and lost by default.";
    }

    modal.classList.remove("hidden");
});

const board = document.querySelector(".chess-board");

if (!board) {
    throw new Error("Chess board was not found!");
}

const gameId = board.dataset.gameId;
const squares = document.querySelectorAll(".chess-square");

console.log("Game ID:", gameId);
console.log("Squares:", squares.length);

let gameActive = true;

let selectedSquare = null;

document.querySelector("#closeBtn").addEventListener("click", () => {
        document.querySelector("#game-end-modal").classList.add("hidden");
    });

squares.forEach(square => {

    square.addEventListener("click", async () => {

        const file = Number(square.dataset.file);
        const rank = Number(square.dataset.rank);

        // Clicking a legal destination
        if (square.classList.contains("legal-move") && selectedSquare && gameActive) {

            let promotionPiece = null;

            const piece = selectedSquare.querySelector(".chess-piece");

            if ((piece?.dataset.piece === "P" && rank === 7) ||
                (piece?.dataset.piece === "p" && rank === 0)) {

                promotionPiece = await promotePawn();
            }

            const success = await makeMove(
                Number(selectedSquare.dataset.file),
                Number(selectedSquare.dataset.rank),
                file,
                rank,
                promotionPiece
            );

            if (success) {
                removeLegalMoves();
            }

            return;
        }

        // Clicking a piece
        if (square.querySelector(".chess-piece") && gameActive) {

            // Clicking the already selected piece
            if (square === selectedSquare) {
                removeLegalMoves();
                return;
            }

            // Selecting a different piece
            removeLegalMoves();

            await getLegalMoves(file, rank);

            return;
        }

        // Clicking an empty, non-legal square
        removeLegalMoves();
    });

});

function promotePawn() {
    const modal = document.querySelector("#promotion-modal");

    modal.classList.remove("hidden");

    return new Promise(resolve => {

        const buttons = modal.querySelectorAll("button");

        buttons.forEach(button => {
            button.onclick = () => {

                const piece = button.dataset.piece;

                modal.classList.add("hidden");

                resolve(piece);
            };
        });

    });
}


async function makeMove(fromFile, fromRank, toFile, toRank, promotionPiece) {
    try {
        await connection.invoke("MakeMove", {
            gameId: gameId,
            fromFile: fromFile,
            fromRank: fromRank,
            toFile: toFile,
            toRank: toRank,
            promotionPiece: promotionPiece
        });

        return true;
    }
    catch (err) {
        console.error(err);
        return false;
    }
}

connection.on("MoveMade", function (result) {
    applyBoardChanges(result.boardChanges);

    document.getElementById("currentTurn").textContent = result.isWhiteTurn ? "White" : "Black";

    checkGameState(result.status, result.endReason);
});

function applyBoardChanges(boardChanges) {

    boardChanges.forEach(boardChange => {

        const square = document.querySelector(
            `[data-file="${boardChange.file}"][data-rank="${boardChange.rank}"]`
        );

        if (!square) {
            return;
        }

        const existingPiece = square.querySelector('.chess-piece');

        if (existingPiece) {
            existingPiece.remove()
        }

        if (boardChange.piece !== null) {
            const piece = document.createElement('span');

            piece.classList.add('chess-piece');
            piece.dataset.piece = boardChange.piece;
            piece.textContent = getPieceSymbol(boardChange.piece);

            square.appendChild(piece);
        }
    });
}

function getPieceSymbol(piece) {
    const pieces = {
        'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
        'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
    };

    return pieces[piece] || '';
}

function checkGameState(status, endReason) {

    console.log("Game state:", status, endReason);

    if (status === "Active") {
        return;
    }

    gameActive = false;

    const modal = document.querySelector("#game-end-modal");
    const title = document.querySelector("#game-end-title");
    const message = document.querySelector("#game-end-message");

    if (status === "WhiteWin") {
        title.textContent = "White wins!";
    }
    else if (status === "BlackWin") {
        title.textContent = "Black wins!";
    }
    else if (status === "Draw") {
        title.textContent = "Draw";
    }

    switch (endReason) {
        case "Checkmate":
            message.textContent = "Checkmate.";
            break;

        case "Stalemate":
            message.textContent = "Stalemate.";
            break;

        case "ThreefoldRepetition":
            message.textContent = "Draw by threefold repetition.";
            break;

        case "FiftyMoveRule":
            message.textContent = "Draw by the fifty-move rule.";
            break;

        case "InsufficientMaterial":
            message.textContent = "Draw by insufficient material.";
            break;

        default:
            message.textContent = "";
            break;
    }

    modal.classList.remove("hidden");
}


async function getLegalMoves(file, rank) {

    const params = new URLSearchParams({
        handler: "PossibleMoves",
        gameId,
        file,
        rank
    });

    const response = await fetch(`?${params}`);

    if (!response.ok) {
        return;
    }

    const moves = await response.json();

    selectedSquare = document.querySelector(`.chess-square[data-file="${file}"][data-rank="${rank}"]`)

    selectedSquare.classList.add('selected-square')

    moves.forEach(move => {

        const targetSquare = document.querySelector(`.chess-square[data-file="${move.file}"][data-rank="${move.rank}"]`);

        if (targetSquare) {
            targetSquare.classList.add('legal-move');
        }

    });
}

function removeLegalMoves() {

    document.querySelectorAll('.legal-move').forEach(square => square.classList.remove('legal-move'));

    document.querySelectorAll('.selected-square').forEach(square => square.classList.remove('selected-square'));

    selectedSquare = null;
}