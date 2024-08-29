"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Start the connection
connection.start().then(function () {
    console.log("SignalR connection established!");

}).catch(function (err) {
    return console.error("SignalR connection failed: " + err.toString());
});


// Wait for the message from server
connection.on("StatusUpdate", function (user) {
    // in theory all clients should get game status update
    cleanActionButtons();
    getStatus();
});

