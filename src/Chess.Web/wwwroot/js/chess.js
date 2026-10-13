"use strict";

console.log("CHESS.JS LOADED");

const board = document.querySelector(".chess-board");
const gameId = board.dataset.gameId;
const squares = document.querySelectorAll(".chess-square");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chessHub")
    .withAutomaticReconnect([0, 5000, 15000, 30000]) // Try reconnecting instantly, then 5s, 15s, 30s
    .build();

let gameActive = true;
let selectedSquare = null;
let drawOfferPending = false;
let drawOfferSent = false;

const offerDrawButton = document.querySelector("#offer-draw-btn");
const forfeitButton = document.querySelector("#forfeit-btn");
const drawOfferPanel = document.querySelector("#draw-offer-panel");
const acceptDrawButton = document.querySelector("#accept-draw-btn");
const declineDrawButton = document.querySelector("#decline-draw-btn");
const actionMessage = document.querySelector("#game-action-message");

const moveHistoryList = document.querySelector("#move-history-list");
const noMovesMessage = document.querySelector("#no-moves-message");

let moveHistory = [];

const iAmWhite = board.dataset.isWhitePlayer.toLowerCase() === "true";

function setActionMessage(message) {
    actionMessage.textContent = message;
}

function resetDrawOffer() {
    drawOfferPending = false;
    drawOfferSent = false;

    drawOfferPanel.hidden = true;
    offerDrawButton.disabled = !gameActive;
    acceptDrawButton.disabled = false;
    declineDrawButton.disabled = false;
}

function disableGameActions() {
    offerDrawButton.disabled = true;
    forfeitButton.disabled = true;
    acceptDrawButton.disabled = true;
    declineDrawButton.disabled = true;

    drawOfferPanel.hidden = true;
}

function showGameEnd(titleText, messageText) {
    gameActive = false;
    selectedSquare = null;
    removeLegalMoves();
    disableGameActions();

    const modal = document.querySelector("#game-end-modal");

    document.querySelector("#game-end-title").textContent = titleText;
    document.querySelector("#game-end-message").textContent = messageText;

    modal.classList.remove("hidden");
}

function showGameMessage(titleText, messageText) {
    const modal = document.querySelector("#game-end-modal");

    document.querySelector("#game-end-title").textContent = titleText;
    document.querySelector("#game-end-message").textContent = messageText;

    modal.classList.remove("hidden");
}

function hideGameMessage() {
    document.querySelector("#game-end-modal").classList.add("hidden");
}

function checkGameState(status, endReason) {
    console.log("Game state:", status, endReason);

    if (status === "Active") {
        return;
    }

    const messages = {
        Checkmate: "Checkmate.",
        Stalemate: "Stalemate.",
        ThreefoldRepetition: "Draw by threefold repetition.",
        FiftyMoveRule: "Draw by the fifty-move rule.",
        InsufficientMaterial: "Draw by insufficient material.",
        Agreement: "Both players agreed to a draw.",
        Forfeit: "The game ended by forfeit.",
        Disconnect: "The game ended because of a disconnection."
    };

    let title;

    switch (status) {
        case "WhiteWin":
            title = "White wins!";
            break;
        case "BlackWin":
            title = "Black wins!";
            break;
        case "Draw":
            title = "Game drawn";
            break;
        default:
            title = "Game over";
            break;
    }

    showGameEnd(title, messages[endReason] ?? "");
}

document.querySelector("#closeBtn").addEventListener("click", () => {
    document.querySelector("#game-end-modal").classList.add("hidden");
});



connection.on("MoveMade", function (result) {
    const gameContainer = document.querySelector(".chess-game-container");

    gameContainer.classList.toggle("white-turn", result.isWhiteTurn);
    gameContainer.classList.toggle("black-turn", !result.isWhiteTurn);

    applyBoardChanges(result.boardChanges);
    addMoveToHistory(result.notation);

    if (drawOfferSent || drawOfferPending) {
        resetDrawOffer();
        setActionMessage("The draw offer expired after a move.");
    }

    checkGameState(result.status, result.endReason);
});

connection.on("GameEndedByTimeout", (isWhiteWinner) => {
    const iWon = iAmWhite

    showGameEnd(
        iWon ? "Victory!" : "Match Forfeited",
        iWon ? "Your opponent failed to reconnect. You win by default!"
            : "You were disconnected for too long and lost by default."
    );
});

connection.on("OpponentDisconnected", () => {
    gameActive = false;
    selectedSquare = null;
    removeLegalMoves();

    showGameMessage("Your opponent has disconnected...", "Waiting for them to reconnect (60s)...");
});

connection.on("OpponentReconnected", () => {
    gameActive = true;
    hideGameMessage();
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

// The opponent has offered a draw
connection.on("DrawOffered", () => {
    if (!gameActive) return;

    drawOfferPending = true;
    drawOfferPanel.hidden = false;
    offerDrawButton.disabled = true;

    setActionMessage("Your opponent has offered a draw.");
});

// A draw offer was declined
connection.on("DrawOfferDeclined", () => {
    if (!gameActive) return;

    resetDrawOffer();
    setActionMessage("The draw offer was declined.");
});

// A player resigned
connection.on("GameEndedByForfeit", (isWhiteWinner) => {
    const iWon = iAmWhite === isWhiteWinner;

    showGameEnd(
        iWon ? "Victory!" : "Game over",
        isWhiteWinner
            ? "White wins by forfeit."
            : "Black wins by forfeit."
    );
});

// Both players agreed to a draw
connection.on("GameEndedByMutualAgreement", () => {
    showGameEnd(
        "Game drawn",
        "Both players agreed to a draw."
    );
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

function applyBoardChanges(boardChanges) {

    boardChanges.forEach(boardChange => {

        const square = document.querySelector(`[data-file="${boardChange.file}"][data-rank="${boardChange.rank}"]`);

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

function renderMoveHistory() {
    moveHistoryList.replaceChildren();

    if (moveHistory.length === 0) {
        const message = document.createElement("p");
        message.textContent = "No moves have been played yet.";
        moveHistoryList.appendChild(message);
        return;
    }

    for (let i = 0; i < moveHistory.length; i += 2) {
        const row = document.createElement("div");
        row.classList.add("move-history-row");

        const moveNumber = document.createElement("span");
        moveNumber.textContent = `${Math.floor(i / 2) + 1}.`;

        const whiteMove = document.createElement("span");
        whiteMove.textContent = moveHistory[i];

        const blackMove = document.createElement("span");
        blackMove.textContent = moveHistory[i + 1] ?? "";

        row.append(moveNumber, whiteMove, blackMove);
        moveHistoryList.appendChild(row);
    }
}

function addMoveToHistory(notation) {
    if (!notation) {
        return;
    }

    moveHistory.push(notation);
    renderMoveHistory();
}


// Offer a draw
offerDrawButton.addEventListener("click", async () => {
    if (!gameActive || drawOfferSent || drawOfferPending) return;

    offerDrawButton.disabled = true;

    try {
        await connection.invoke("OfferDraw", gameId);

        drawOfferSent = true;
        setActionMessage("Draw offer sent. Waiting for your opponent.");
    } catch (error) {
        console.error("Failed to offer draw:", error);
        setActionMessage(error.message || "Could not offer a draw.");
        offerDrawButton.disabled = false;
    }
});

// Resign the game
forfeitButton.addEventListener("click", async () => {
    if (!gameActive) return;

    if (!confirm("Are you sure you want to resign this game?")) {
        return;
    }

    forfeitButton.disabled = true;

    try {
        await connection.invoke("ForfeitGame", gameId);
    } catch (error) {
        console.error("Failed to resign:", error);
        setActionMessage(error.message || "Could not resign.");
        forfeitButton.disabled = false;
    }
});

// Accept a draw offer
acceptDrawButton.addEventListener("click", async () => {
    if (!gameActive || !drawOfferPending) return;

    acceptDrawButton.disabled = true;
    declineDrawButton.disabled = true;

    try {
        await connection.invoke("ResponseToDrawOffer", gameId, true);
    } catch (error) {
        console.error("Failed to accept draw:", error);
        setActionMessage(error.message || "Could not accept the draw.");
        acceptDrawButton.disabled = false;
        declineDrawButton.disabled = false;
    }
});

// Decline a draw offer
declineDrawButton.addEventListener("click", async () => {
    if (!gameActive || !drawOfferPending) return;

    acceptDrawButton.disabled = true;
    declineDrawButton.disabled = true;

    try {
        await connection.invoke("ResponseToDrawOffer", gameId, false);
    } catch (error) {
        console.error("Failed to decline draw:", error);
        setActionMessage(error.message || "Could not decline the draw.");
        acceptDrawButton.disabled = false;
        declineDrawButton.disabled = false;
    }
});



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

connection.onreconnecting(() => {
    gameActive = false;
    selectedSquare = null;
    removeLegalMoves();

    showGameMessage("Connection Lost...", "Attempting to reconnect and restore your match...");
});

connection.onreconnected(async (newConnectionId) => {
    try {
        showGameMessage("Connection Lost...", "Restoring game state...");

        await connection.invoke("JoinGame", gameId);
    } catch (err) {
        console.error("Failed to automatically rejoin the game: ", err);
        showGameEnd("Rejoin Failed", "Could not synchronize with the match.")
    }
});

connection.onclose((error) => {
    console.assert(connection.state === signalR.HubConnectionState.Disconnected);

    gameActive = false;

    const modal = document.querySelector("#game-end-modal");

    document.querySelector("#game-end-title").textContent = "You disconnected from the server";
    document.querySelector("#game-end-message").textContent = "The match has been forfeited.";

    modal.classList.remove("hidden");
});

startConnection();