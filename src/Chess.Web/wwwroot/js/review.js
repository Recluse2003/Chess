const board = document.querySelector(".chess-board");
const gameId = board.dataset.gameId;

const firstMoveButton = document.getElementById("first-move");
const previousMoveButton = document.getElementById("previous-move");
const nextMoveButton = document.getElementById("next-move");
const lastMoveButton = document.getElementById("last-move");

const moveNumberCounter = document.getElementById("move-number");

const numberOfMoves = Number(board.dataset.numberOfMoves);
let currentMoveNumber = numberOfMoves

changeMoveButtonsAvailability();

firstMoveButton.addEventListener("click", () => {
    if (currentMoveNumber > 0) {
        reviewMove(0);
    }
});

previousMoveButton.addEventListener("click", () => {
    if (currentMoveNumber > 0) {
        reviewMove(currentMoveNumber - 1);
    }
});

nextMoveButton.addEventListener("click", () => {
    if (currentMoveNumber < numberOfMoves) {
        reviewMove(currentMoveNumber + 1);
    }
});

lastMoveButton.addEventListener("click", () => {
    if (currentMoveNumber < numberOfMoves) {
        reviewMove(numberOfMoves);
    }
});

function changeMoveButtonsAvailability() {
    if (currentMoveNumber === 0) {
        firstMoveButton.disabled = true;
        previousMoveButton.disabled = true;
    }
    else {
        firstMoveButton.disabled = false;
        previousMoveButton.disabled = false;
    }

    if (currentMoveNumber === numberOfMoves) {
        nextMoveButton.disabled = true;
        lastMoveButton.disabled = true;
    }
    else {
        nextMoveButton.disabled = false;
        lastMoveButton.disabled = false;
    }
}

async function reviewMove(targetMoveNumber) {
    try {
        const response = await fetch(`${window.location.pathname}?handler=ReviewMove`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector(
                    'input[name="__RequestVerificationToken"]'
                ).value
            },
            body: JSON.stringify({
                gameId: gameId,
                currentMoveNumber: currentMoveNumber,
                targetMoveNumber: targetMoveNumber
            })
        });

        if (!response.ok) {
            console.error("Failed to retrieve board changes.");
            return;
        }

        const boardChanges = await response.json();

        applyBoardChanges(boardChanges);

        currentMoveNumber = targetMoveNumber;

        moveNumberCounter.textContent = `${currentMoveNumber} / ${numberOfMoves}`;

        changeMoveButtonsAvailability();
    }
    catch (err) {
        console.error(err);
    }
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