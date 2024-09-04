
var user = {};
var gameStatus = null;
var authCookieName = 'auth_cookie';
var authCookieSecret = 'auth_secret';
init();

function init() {
    let cookieName = getCookie(authCookieName);
    let cookieSecret = getCookie(authCookieSecret);

    if (cookieName == null || cookieSecret == null) {
        document.getElementById('main').classList.add('hidden');
        document.getElementById('logoutBlock').classList.add('hidden');
        document.getElementById('loginBlock').classList.remove('hidden');

    } else {
        user.name = cookieName;
        user.secret = cookieSecret;
        document.getElementById('loginBlock').classList.add('hidden');
        document.getElementById('logoutBlock').classList.remove('hidden');
        document.getElementById('main').classList.remove('hidden');
        getStatus();
    }

    if (user.name) {
        document.getElementById('nameLabel').innerHTML = user.name;
    }
    else {
        document.getElementById('nameLabel').innerHTML = '';
    }
}


// AUTHENTICATION
// You create 2 cookies when you login
function login() {
    let name = document.getElementById('nameInput').value;
    let secret = generateGuid();

    setCookie(authCookieName, name, 1);
    setCookie(authCookieSecret, secret, 1)
    init();
}
// You delete 2 cookies when you logout
function logout() {
    deleteCookie(authCookieName);
    deleteCookie(authCookieSecret);
    init();
}
function deleteCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}
function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    } else if (days === 0) {
        expires = "; expires=Thu, 01 Jan 1970 00:00:00 UTC"; // Set to a date in the past
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
function generateGuid() {
    return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
        (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
    );
}





function createTable() {
    SendRequest({
        method: 'POST',
        url: '/Home/CreateTable',
        body: {},
        success: function (data) {
            let tableId = JSON.parse(data.responseText);
            sitToTheTable(tableId);
        },
        error: function (data) {
            alert("Something went wrong during creating of table")
        }
    });
}

function sitToTheTableClick(element) {
    let tableId = element.closest('.playingTable').getAttribute('table-id');
    sitToTheTable(tableId);
}

// passing params as if its GET method, bad approach but for some reason POST does not work.
function sitToTheTable(tableId) {
    SendRequest({
        method: 'POST',
        url: '/Home/JoinTable',
        body: {
            playerSecret: user.secret,
            playerName: user.name,
            tableId: tableId
        },
        success: function (data) {
            notifyUserBrowsersAboutUpdate();
        },
        error: function (data) {
            alert("Something went wrong during creating of table");
        }
    });
}


function startGame() {
    SendRequest({
        method: 'POST',
        url: '/Home/StartGame',
        body: {
            tableId: gameStatus.table.id,
            playerSecret: user.secret
        },
        success: function (data) {
            notifyUserBrowsersAboutUpdate();
        },
        error: function (data) {
            alert("Attack method did not work")
        }
    });
}

function getStatus() {
    SendRequest({
        method: 'GET',
        url: '/Home/GetStatus?playerSecret=' + user.secret,
        success: function (data) {
            let status = JSON.parse(data.responseText);
            gameStatus = status;

            resetClientView();
            if (status.table == null) {
                drawTables(status);
            } else {
                drawGameStatus(status);
                drawYourself();
                drawHand(status);
                drawDeckAndTrumpCard(status);
                drawBattleField(status);
                drawPlayersAndTheirHands(status);
                drawSurrenderButton(status);
                drawEndRoundButton(status);

                
            }
        },
        error: function (data) {
            alert("Something went wrong during creation of table")
        }
    });
}

function resetClientView() {
    // clear hand, deck and trump cards to refresh the state
    document.getElementById('yourPlayer').innerHTML = '';
    document.getElementById('playingCards').innerHTML = '';
    document.getElementById('deck').innerHTML = '';
    document.getElementById('field').innerHTML = '';
    document.getElementById('tables').innerHTML = '';
    document.getElementById('players').innerHTML = '';
    document.getElementById('currentGame_Info_gameStatus').innerHTML = '';
    document.getElementById('currentGame_Info_timer').innerHTML = '';
    document.getElementById('currentGame_Info_playerRole').innerHTML = '';
}
function drawTables(status) {
    let originalTableDiv = document.getElementsByClassName('playingTable')[0];
    let fragment = document.createDocumentFragment();

    for (let i = 0; i < status.tables.length; i++) {
        let table = status.tables[i];
        let tableDiv = originalTableDiv.cloneNode(true);

        let buttonElement = tableDiv.querySelector('.playingTable__btn');
        tableDiv.appendChild(buttonElement);
        tableDiv.setAttribute('table-id', table.id);

        for (var j = 0; j < table.players.length; j++) {
            let player = table.players[j];
            let playerLabel = document.createElement("label");
            playerLabel.innerHTML = player.name;
            playerLabel.classList.add('playingTable_player_name');
            tableDiv.appendChild(playerLabel);
        }
        fragment.appendChild(tableDiv);
    }
    document.getElementById('tables').appendChild(fragment);
}
function drawGameStatus(status) {
    if (status.table.status != null && status.table.status != undefined) {
        if (status.table.status != 'InProgress') {
            document.getElementById('currentGame_Info_gameStatus').innerHTML = status.table.status;
        }
        if (status.table.ownerSecretKey != null) {
            if (status.table.status == 'ReadyToBegin' && user.secret == status.table.ownerSecretKey) {
                document.getElementById('startGame').classList.remove('hidden');
            } else {
                document.getElementById('startGame').classList.add('hidden');
            }
        }

        if (status.table.status == 'InProgress') {
            if (status.table.defenderSecretKey == user.secret) {
                document.getElementById('currentGame_Info_playerRole').innerHTML = 'Defend';
            } else if (status.table.attackingSecretKey == user.secret) {
                document.getElementById('currentGame_Info_playerRole').innerHTML = 'Attack';
            } else {
                document.getElementById('currentGame_Info_playerRole').innerHTML = 'Support '
            }
        }

    }
}
function drawYourself() {
    let existingPlayerDiv = document.getElementById('yourPlayerDiv');
    if (existingPlayerDiv) {
        existingPlayerDiv.remove();
    }

    let yourPlayerDiv = document.getElementsByClassName('meAsPlayer')[0].cloneNode(true);
    yourPlayerDiv.getElementsByClassName('player__name')[0].innerHTML = user.name;
    yourPlayerDiv.getElementsByClassName('player__name')[0].title = user.name;
    document.getElementById('yourPlayer').appendChild(yourPlayerDiv);
}
function drawHand(status) {
    if (status.table && status.table.playerHand && status.table.playerHand.length > 0) {
        let originalPlayingCardDiv = document.getElementsByClassName('playingCard')[0];
        let fragment = document.createDocumentFragment();

        for (let card of status.table.playerHand) {
            let cardDiv = originalPlayingCardDiv.cloneNode(true);
            cardDiv.addEventListener('click', function (event) {
                this.classList.toggle('active');
                checkMoves();
            });

            cardDiv.getElementsByClassName('playingCard_info_rank')[0].innerHTML = getRank(card.rank, card.suit);
            cardDiv.getElementsByClassName('playingCard_info_suit')[0].innerHTML = getSuit(card.suit);
            cardDiv.getElementsByClassName('playingCard_info_largeSuit')[0].innerHTML = getSuit(card.suit);

            fragment.appendChild(cardDiv);
        }

        document.getElementById('playingCards').appendChild(fragment);
    }
}
function drawDeckAndTrumpCard(status) {
    if (status.table.trump != null) {
        let trumpCard = status.table.trump;
        let trumpCardDiv = document.getElementsByClassName('playingCard')[0].cloneNode(true);

        if (status.table.deckCardsCount == 0) {
            trumpCardDiv.getElementsByClassName('playingCard_info_rank')[0].innerHTML = '';
            trumpCardDiv.getElementsByClassName('playingCard_info_suit')[0].innerHTML = '';
            trumpCardDiv.getElementsByClassName('playingCard_info_largeSuit')[0].innerHTML = getSuit(trumpCard.suit);
        }
        else {
            trumpCardDiv.getElementsByClassName('playingCard_info_rank')[0].innerHTML = getRank(trumpCard.rank);
            trumpCardDiv.getElementsByClassName('playingCard_info_suit')[0].innerHTML = getSuit(trumpCard.suit);
            trumpCardDiv.getElementsByClassName('playingCard_info_largeSuit')[0].innerHTML = getSuit(trumpCard.suit);
        }

        document.getElementById('deck').appendChild(trumpCardDiv);
    }

    if (status.table.deckCardsCount > 1) {
        let cardDiv = document.getElementsByClassName('deck_card_shirt')[0].cloneNode(true);
        cardDiv.getElementsByClassName('deck_card_number')[0].innerHTML = status.table.deckCardsCount;
        document.getElementById('deck').appendChild(cardDiv);
    }
}

function drawBattleField(status) {
    if (status.table.cardsOnTheTable.length > 0) {
        // Cache the original card element before the loop
        let originalCardDiv = document.getElementsByClassName('playingCard')[0];
        let originalDefendingCardDiv = document.getElementsByClassName('defendingCard')[0];
        // Create a document fragment to batch append operations
        let fragment = document.createDocumentFragment();

        for (let i = 0; i < status.table.cardsOnTheTable.length; i++) {
            let cardOnTheTable = status.table.cardsOnTheTable[i];
            let cardDiv = originalCardDiv.cloneNode(true);

            cardDiv.getElementsByClassName('playingCard_info_rank')[0].innerHTML = getRank(cardOnTheTable.attackingCard.rank, cardOnTheTable.attackingCard.suit);
            cardDiv.getElementsByClassName('playingCard_info_suit')[0].innerHTML = getSuit(cardOnTheTable.attackingCard.suit);
            cardDiv.getElementsByClassName('playingCard_info_largeSuit')[0].innerHTML = getSuit(cardOnTheTable.attackingCard.suit);

            // If there is a defending card, attach it to the same cardDiv
            if (cardOnTheTable.defendingCard != null) {
                let defendingCardDiv = originalDefendingCardDiv.cloneNode(true);
                defendingCardDiv.getElementsByClassName('playingCard_info_rank')[0].innerHTML = getRank(cardOnTheTable.defendingCard.rank, cardOnTheTable.defendingCard.suit);
                defendingCardDiv.getElementsByClassName('playingCard_info_suit')[0].innerHTML = getSuit(cardOnTheTable.defendingCard.suit);
                defendingCardDiv.getElementsByClassName('playingCard_info_largeSuit')[0].innerHTML = getSuit(cardOnTheTable.defendingCard.suit);

                // Append defending card as a child of the attacking cardDiv
                cardDiv.appendChild(defendingCardDiv);
            } else {
                cardDiv.addEventListener('click', function (event) {
                    if (this.classList.contains('active')) {
                        this.classList.remove('active');
                    } else {
                        this.classList.add('active');
                    }
                    checkMoves();
                });
            }

            // Append the cardDiv (with or without defending card) to the fragment
            fragment.appendChild(cardDiv);
        }
        document.getElementById('field').appendChild(fragment);
    }
}


// We need to draw people clock wise
// If our index is 3 then 2 1 3 5 4
function drawPlayersAndTheirHands(status) {
    let playerIndexes = [];
    for (let i = status.table.myIndex - 1; i >= 0; i--) {
        playerIndexes.push({ index: i, gameIndex: i });
    }
    for (let i = status.table.players.length - 1; i >= status.table.myIndex; i--) {
        playerIndexes.push({ index: i, gameIndex: i + 1 });
    }

    let originalPlayerDiv = document.getElementsByClassName('player')[0];
    let singleCardBackTemplate = document.getElementsByClassName('player__card')[0]
    let fragmentForPlayers = document.createDocumentFragment();

    for (let i = 0; i < playerIndexes.length; i++) {
        let playerIndex = playerIndexes[i].index;
        let player = status.table.players[playerIndex];
        let playerDiv = originalPlayerDiv.cloneNode(true);

        playerDiv.getElementsByClassName('player__name')[0].innerHTML = player.name;
        playerDiv.getElementsByClassName('player__name')[0].title = player.name;
        // Clear any previous cards in this playerDiv
        let playerCardsDiv = playerDiv.getElementsByClassName('player__cards')[0];
        playerCardsDiv.innerHTML = ''; // Clear any previous cards

        fragmentForPlayers.appendChild(playerDiv);

        for (let j = 0; j < player.cardsCount; j++) {
            let cardDiv = singleCardBackTemplate.cloneNode(true);
            playerCardsDiv.appendChild(cardDiv);
        }
    }

    document.getElementById('players').appendChild(fragmentForPlayers);
}

function drawSurrenderButton(status) {
    if (user.secret == gameStatus.table.defenderSecretKey && status.table.cardsOnTheTable.length > 0) {
        let notDefendedCard = false;
        for (let i = 0; i < status.table.cardsOnTheTable.length; i++) {
            if (status.table.cardsOnTheTable[i].defendingCard == null) {
                notDefendedCard = true;
                break;
            }
        }
        if (notDefendedCard) {
            document.getElementById('playingCards_btn_surrender').classList.remove('hidden');
        }
    }
}
function drawEndRoundButton(status) {
    // So it must be attacking player, there should cards on the table and all cards should be defended
    //all attacking players must agree to finish the round, it should not be decided by 1 player
    if (user.secret == gameStatus.table.attackingSecretKey && status.table.cardsOnTheTable.length > 0) {
        let cardsOnTheTable = status.table.cardsOnTheTable;
        let attackingCardsCount = cardsOnTheTable.filter(card => card.attackingCard).length;
        let defendingCardsCount = cardsOnTheTable.filter(card => card.defendingCard).length;

        if (attackingCardsCount == defendingCardsCount && status.table.doIWishToFinishTheRound == false) {
            document.getElementById('playingCards_btn_endRound').classList.remove('hidden');
        }
    }
}


function checkMoves() {
    // shows move button to attack if hand cards are selected and if there not more than 5 cards on the table.
    let activeCardsOnTheHand = document.querySelectorAll('#playingCardsContainer .playingCard.active');
    if (activeCardsOnTheHand.length > 0 && user.secret != gameStatus.table.defenderSecretKey && gameStatus.table.cardsOnTheTable.length < 6) {
        document.getElementById('playingCards_btn_attack').classList.remove('hidden');
    } else {
        document.getElementById('playingCards_btn_attack').classList.add('hidden');
    }


    // shows defend button if table cards are selected
    let activeCardsOnTheBattleField = document.querySelectorAll('#field .playingCard.active');
    // only 1 card should be selected as attackign and 1 as defending
    if (activeCardsOnTheBattleField.length == 1 && activeCardsOnTheHand.length == 1 && user.secret == gameStatus.table.defenderSecretKey) {
        document.getElementById('playingCards_btn_attack').classList.add('hidden');
        document.getElementById('playingCards_btn_defend').classList.remove('hidden');
    } else {
        document.getElementById('playingCards_btn_defend').classList.add('hidden');
    }
}


function defend() {
    let defendingCard = document.querySelector('#playingCards .playingCard.active');
    let attackingCard = document.querySelector('#field .playingCard.active');

    if (defendingCard && attackingCard) {
        let defendingCardIndex = Array.prototype.indexOf.call(defendingCard.parentNode.children, defendingCard);
        let attackingCardIndex = Array.prototype.indexOf.call(attackingCard.parentNode.children, attackingCard);

        SendRequest({
            method: 'POST',
            url: '/Home/Defend',
            body: {
                tableId: gameStatus.table.id,
                playerSecret: user.secret,
                defendingCardIndex: defendingCardIndex,
                attackingCardIndex: attackingCardIndex
            },
            success: function (data) {
                notifyUserBrowsersAboutUpdate();
            },
            error: function (data) {
                alert("Attack method did not work");
            }
        });
    }
}

function surrender() {
    SendRequest({
        method: 'POST',
        url: '/Home/SurrenderRound',
        body: {
            tableId: gameStatus.table.id,
            playerSecret: user.secret
        },
        success: function (data) {
            notifyUserBrowsersAboutUpdate();
        },
        error: function (data) {
            alert("Surrender method did not work");
        }
    });
}

function endRound() {
    SendRequest({
        method: 'POST',
        url: '/Home/EndRound',
        body: {
            tableId: gameStatus.table.id,
            playerSecret: user.secret
        },
        success: function (data) {
            notifyUserBrowsersAboutUpdate();
        },
        error: function (data) {
            alert("EndRound method did not work");
        }
    });
}


function makeAMove() {
    let selectedCards = document.querySelectorAll('#playingCardsContainer .playingCard.active');
    let cardIndexes = [];
    if (selectedCards.length > 0) {
        let cards = document.querySelectorAll('#playingCards .playingCard');
        for (let i = 0; i < cards.length; i++) {
            if (cards[i].classList.contains('active')) {
                cardIndexes.push(i);
            }
        }
        SendRequest({
            method: 'POST',
            url: '/Home/Attack',
            body: {
                playerSecret: user.secret,
                tableId: gameStatus.table.id,
                cardIds: cardIndexes
            },
            success: function (data) {
                notifyUserBrowsersAboutUpdate();
            },
            error: function (data) {
                alert("Attack method did not work")
            }
        });
    }
}



function getRank(value, suitChar) {
    if (suitChar === '♦' || suitChar === '♥') {
        return "<label style='color:red'>" + value + "</label>";
    } else {
        return value;
    }
}

function getSuit(suitChar) {
    if (suitChar === '♦' || suitChar === '♥') {
        return "<label style='color:red'>" + suitChar + "</label>";
    } else {
        return suitChar;
    }
}

function cleanActionButtons() {
    document.getElementById("playingCards_btn_attack").classList.add('hidden');
    document.getElementById("playingCards_btn_defend").classList.add('hidden');
    document.getElementById("playingCards_btn_surrender").classList.add('hidden');
    document.getElementById("playingCards_btn_endRound").classList.add('hidden');
}


function notifyUserBrowsersAboutUpdate() {
    gameHubConnection.invoke("UpdateGameState", user.name).catch(function (err) {
        return alert(err.toString());
    });
}