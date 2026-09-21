"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/lobbyHub").build();

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

var lobby = document.getElementById("lobby");
var gameId = lobby.dataset.gameId;

connection.on("PlayerJoined", function (username) {
    document.getElementById("blackPlayer").textContent = username;
    document.getElementById("startButton").disabled = false;
});

connection.on("GameStarted", function () {
    window.location.href = `/Chess/${gameId}`;
});

document.getElementById("startButton").addEventListener("click", async function (event) {

    await connection.invoke("StartGame", gameId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.invoke("JoinGame", gameId).catch(function (err) {
    return console.error(err.toString());
});
event.preventDefault();
