"use strict";

const lobby = document.getElementById("lobby");
const gameId = lobby.dataset.gameId;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/lobbyHub")
    .build();

connection.on("PlayerJoined", function (username) {
    document.getElementById("blackPlayer").textContent = username;
    document.getElementById("startButton").disabled = false;
});

connection.on("GameStarted", function () {
    window.location.href = `/Chess/${gameId}`;
});

connection.on("PlayerLeft", function () {
    document.getElementById("blackPlayer").textContent = "Waiting for opponent...";
    document.getElementById("startButton").disabled = true;
});

connection.on("LobbyClosed", function () {
    alert("The host has closed the lobby.");
    window.location.href = "/Games/Index";
});

async function startConnection() {
    try {
        await connection.start();

        console.log("Connected to LobbyHub");

        await connection.invoke("JoinLobby", gameId);

        console.log("Joined lobby:", gameId);
    }
    catch (err) {
        console.error(err);
    }
}

document.getElementById("startButton")?.addEventListener("click", async function () {
    try {
        await connection.invoke("StartGame", gameId);
    }
    catch (err) {
        console.error(err);
    }
});

document.getElementById("leaveButton")?.addEventListener("click", async function () {
    try {
        await connection.invoke("LeaveLobby", gameId);
        window.location.href = "/Games/Index";
    }
    catch (err) {
        console.error(err);
    }
});

startConnection();