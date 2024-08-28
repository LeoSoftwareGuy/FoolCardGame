"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.on("ReceiveMessage", function (user, message) {
    getStatus();
});
