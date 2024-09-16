"use strict";

var gameHubConnection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Start the connection
gameHubConnection.start().then(function () {
    console.log("SignalR connection established!");
}).catch(function (err) {
    return console.error("SignalR connection failed: " + err.toString());
});

// Wait for the message from server
gameHubConnection.on("StatusUpdate", function () {
    cleanActionButtons();
    getStatus();
});


gameHubConnection.on("AfkPlayerIsOut", function (message) {
    alert(message);
    cleanActionButtons();
    getStatus();
});

gameHubConnection.on("RoundFinished", function () {
    cleanActionButtons();
    getStatus();
});

gameHubConnection.on("TimePassed", function (message, isSurrender) {
    updateSurrenderTimers(message, 'defending', 'yourPlayer', isSurrender);
    updateSurrenderTimers(message, 'attacking', 'players', isSurrender);
});

// Reusable function for updating surrender timers
// Idea is that we check if the timer is present in the container and update it
// Based on user role he will have different class names for the timer and container
function updateSurrenderTimers(message, role, containerId, isSurrender) {
    const container = document.getElementById(containerId);
    if (!container) return;

    if (containerId === 'yourPlayer') {
        // Check specific 'yourPlayerDiv' for defending timer
        const timerElement = container.getElementsByClassName(`player__timer__${role}`)[0];
        if (timerElement && timerElement.innerHTML.trim() !== '') {
            if (isSurrender) {
                timerElement.innerHTML = `I am surrendering in ${message} seconds`;
            }
            else {
                timerElement.innerHTML = `Lets finish the round in ${message} seconds`;
            }

        }
    } else if (containerId === 'players') {
        // Check all player divs within 'players' for attacking timer
        const playerDivs = container.getElementsByClassName('player');
        for (let i = 0; i < playerDivs.length; i++) {
            const playerDiv = playerDivs[i];
            const timerElement = playerDiv.getElementsByClassName(`player__timer__${role}`)[0];
            if (timerElement && timerElement.innerHTML.trim() !== '') {
                if (isSurrender) {
                    timerElement.innerHTML = `I am surrendering in ${message} seconds`;
                }
                else {
                    timerElement.innerHTML = `Lets finish the round in ${message} seconds`;
                }
            }
        }
    }
}