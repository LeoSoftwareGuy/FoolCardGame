"use strict";

var gameHubConnection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Start the connection
gameHubConnection.start().then(function () {
    console.log("SignalR connection established!");

}).catch(function (err) {
    return console.error("SignalR connection failed: " + err.toString());
});


// Wait for the message from server
gameHubConnection.on("StatusUpdate", function (user) {
    cleanActionButtons();
    getStatus();
});

gameHubConnection.on("SurrenderFinished", function () {
    cleanActionButtons();
    getStatus();
});

gameHubConnection.on("TimePassed", function (message) {
    document.getElementById("currentGame_Info_timer").innerHTML = message + ' seconds until surrender';
})

