"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/lobbyHub").build();

var lobby = document.getElementById("lobby");
var gameId = lobby.dataset.gameId;

connection.on("PlayerJoined", function (username) {
    document.getElementById("blackPlayer").textContent = username;
    document.getElementById("startButton").disabled = false;
});

connection.on("GameStarted", function () {
    window.location.href = `/Chess/${gameId}`;
});

document.getElementById("startButton").addEventListener("click", function (event) {

    connection.invoke("StartGame", gameId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.invoke("JoinGame", gameId).catch(function (err) {
    return console.error(err.toString());
});
event.preventDefault();
