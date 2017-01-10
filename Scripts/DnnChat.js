//disable the enter key, knockout rebinds it later
$(function () {
    $("#Form").bind("keypress", function (e) {
        if (e.keyCode == 13) {
            return false;
        }
        return true;
    });
});

function DnnChat($, ko, settings) {
    var moduleid = settings.moduleId;
    var userid = settings.userId;
    var username = settings.userName;
    var startmessage = settings.startMessage;
    var stateReconnecting = settings.stateReconnecting;
    var stateReconnected = settings.stateReconnected;
    var stateConnected = settings.stateConnected;
    var stateDisconnected = settings.stateDisconnected;
    var stateConnectionSlow = settings.stateConnectionSlow;
    var alreadyInRoom = settings.alreadyInRoom;
    var anonUsersRooms = settings.anonUsersRooms;
    var messageMissingRoom = settings.messageMissingRoom;
    var messagePasswordEntry = settings.messagePasswordEntry;
    var defaultRoomId = settings.defaultRoomId;
    var errorSendingMessage = settings.errorSendingMessage;
    var defaultAvatarUrl = settings.defaultAvatarUrl;
    var allUsersNotification = settings.allUsersNotification;
    
    var roomArchiveLink = settings.roomArchiveLink;
    var emoticonsUrl = settings.emoticonsUrl; //<%= ResolveUrl(ControlPath + "images/emoticons/simple/") %>
    var userroles = settings.roles;
    var messageDeleteConfirmation = settings.messageDeleteConfirmation;
    var focus = true;
    var pageTitle = document.title;
    var unread = 0;
    var mentions = 0;
    var firstConnection = true;

    var activeRoomId = '';

    if (username == '')
        username = 'phantom';

    $(window).focus(function () {
        focus = true;
        unread = 0;
        mentions = 0;
        //clear the title of unread
        window.setTimeout(SetTitle, 200);
    });

    $(window).blur(function () {
        focus = false;
        SetTitle();
    });

    //user connection mapping function
    function ConnectionRecord(u) {
        this.connectionRecordId = u.ConnectionRecordId;
        this.authorName = u.UserName;
        this.userId = u.UserId;
        this.moduleId = u.ModuleId;
        this.connectedDate = u.ConnectedDate;
        this.disconnectedDate = u.DisconnectedDate;
        this.ipAddress = u.IpAddress;
        this.roomId = u.RoomId;
        this.photoUrl = u.PhotoUrl;
        // "/profilepic.ashx?userId=" + u.UserId + "&h=32&w=32";
        //this.profileUrl = "/Activity-Feed/userid/" + u.UserId; //http://www.dnnchat.com/Activity-Feed/userId/1

        this.targetMessageAuthor = function () {
            var foundRoom = findRoom(this.roomId);
            if (foundRoom) {
                foundRoom.newMessageText(foundRoom.newMessageText() + ' @' + this.authorName + ' ');
                foundRoom.setTextFocus();
            }
        };
    }

    //user connection view model
    var usersViewModel = {
        connectionRecords: ko.observableArray([])
    };

    ko.bindingHandlers.dateString = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = valueAccessor();
            var valueUnwrapped = ko.utils.unwrapObservable(value);
            if (valueUnwrapped) {
                //TODO: add a formatting option for the date
                $(element).text(moment.utc(valueUnwrapped).local().format('h:mm:ss a'));
            }
        }
    };

    ko.bindingHandlers.enterKey = {
        init: function (element, valueAccessor, allBindings, vm) {
            ko.utils.registerEventHandler(element, "keydown", function (event) {
                if (event.keyCode === 13) {
                    ko.utils.triggerEvent(element, "change");
                    valueAccessor().call(vm, vm);
                }
                return true;
            });
        }
    };

    //message mapping function
    function Message(m) {
        this.messageId = m.MessageId;
        this.connectionId = m.ConnectionId;
        this.messageText = m.MessageText;
        this.messageDate = m.MessageDate;
        this.authorName = m.AuthorName;
        this.authorUserId = m.AuthorUserId;
        this.roomId = m.RoomId;
        this.defaultAvatarUrl = defaultAvatarUrl;
        this.photoUrl = m.PhotoUrl;

        //patch from @briandukes to highlight your own posts
        this.cssName = "messageRow ChatMessage";
        if (checkMention(m.MessageText, chatHub.state.username)) {
            this.cssName += " ChatMentioned";
        }
        if (m.AuthorName === chatHub.state.username) {
            this.cssName += " ChatSelf";
        }

        this.targetMessageAuthor = function () {
            //todo: is there a more efficient way to find the message entry for this room?
            var foundRoom = findRoom(this.roomId);
            if (foundRoom) {
                foundRoom.newMessageText(foundRoom.newMessageText() + ' @' + this.authorName + ' ');
                foundRoom.setTextFocus();
            }
        };

        this.deleteMessage = function () {
            if (confirm(messageDeleteConfirmation))
                chatHub.server.deleteMessage(this.messageId, moduleid);
        };
    }

    //this can probably be removed
    var messageModel = {
        messages: ko.observableArray([])
    };

    //used for the list of rooms
    var roomModel = {
        rooms: ko.observableArray([]),
        ShowRoomList: function () {
            //get an updated list of rooms for the roomlist
            chatHub.server.getRoomList();

            //open the roomlist dialog
            $(".RoomList").dialog({
                width: '100%',
                modal: true
                , dialogClass: "dnnFormPopup"
            });
        },
        HideLobby: function () {
            $(".RoomList").hide();
        }
    };

    //used to manage which rooms a user is in
    var userRoomModel = {
        rooms: ko.observableArray([])
        , activeRoom: ko.observable(activeRoomId)
        , sortRoomsAscending: function () { this.rooms(this.rooms().sort(function (a, b) { return a.roomName === b.roomName ? 0 : (a.roomName.toLowerCase() < b.roomName.toLowerCase() ? -1 : 1); })); }
    };

    //Room mapping function
    function Room(r) {
        this.roomId = r.RoomId;
        this.roomName = r.RoomName;
        this.roomDescription = r.RoomDescription;
        //this is used to be able to "scroll" properly when a new message comes in, need to be able to know what the outer div is, it is this id
        this.roomNameId = "room-" + r.RoomId;

        this.private = r.Private; //is the room private or not. If so, we should be prompting for Password

        this.roomArchiveLink = roomArchiveLink.slice(0, -1) + r.RoomId;

        this.messages = ko.observableArray([]);
        this.connectionRecords = ko.observableArray([]);

        this.sortRoomUsersAscending = function () {
            this.connectionRecords(this.connectionRecords().sort(function (a, b) { return a.authorName == b.authorName ? 0 : (a.authorName.toLowerCase() < b.authorName.toLowerCase() ? -1 : 1); }));
        };

        this.userCount = ko.computed(function () {
            //count connectionRecords to see how many users are in a Room
            return this.connectionRecords().length;
        }, this);


        this.awayMessageCount = ko.observable(0);
        this.awayMentionCount = ko.observable(0);

        this.formattedAwayMessageCount = ko.computed(function () {
            return "(" + this.awayMessageCount + ")";
        }, this);

        this.formattedAwayMentionCount = ko.computed(function () {
            return '(' + this.awayMentionCount + ')';
        }, this);

        //add a message without parsing
        this.addSystemMessage = function (m) {
            this.messages.push(m);
        }.bind(this);

        this.addMessage = function (m) {
            this.messages.push(replaceMessage(m));

            //check if this is the current room
            if (!this.showRoom()) {
                this.awayMessageCount(this.awayMessageCount() + 1);
                if (checkMention(m.messageText, chatHub.state.username)) {
                    this.awayMentionCount(this.awayMentionCount() + 1);
                }
            } else {
                //only scroll if the room is currently visible
                var parentDiv = "#" + this.roomNameId;

                if ($(parentDiv).scrollTop() + $(parentDiv).height() < $(parentDiv)[0].scrollHeight - 250) {
                    //pause
                } else {
                    $(parentDiv).scrollTop($(parentDiv)[0].scrollHeight);
                }
            }
        }.bind(this);

        this.deleteMessage = function (m) {
            this.messages.remove(function (item) { return item.messageId == m.messageId; });
            this.messages.remove(m);
        }.bind(this);

        this.addConnectionRecord = function (cr) {
            this.connectionRecords.push(cr);
        }.bind(this);

        this.removeConnectionRecords = function () {
            this.connectionRecords.removeAll();
        };

        //this.visible = ko.observable(true);

        this.addOnEnter = function (event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            if (keyCode === 13) {
                this.sendMessage();
                return false;
            }
            return true;
        };

        this.setActiveRoom = function () {
            userRoomModel.activeRoom(this.roomId);
            this.awayMessageCount(0);
            this.awayMentionCount(0);
        };

        this.showRoom = ko.computed(function () {
            return this.roomId === userRoomModel.activeRoom();
        }, this);

        this.textFocus = ko.observable(false);

        //clear out the message text to start
        this.newMessageText = ko.observable("");

        this.setTextFocus = function () {
            this.textFocus(true);
            //TODO: In IE10, the cursor position shows up before the username on the first time in, after that it works fine.
        };

        this.sendMessage = function () {
            //remove all HTML tags first for safety
            var msgSend = $.trim(this.newMessageText().replace(/(<([^>]+)>)/ig, ""));

            //make sure the chat string isn't empty
            if (msgSend != '') {

                // Call the chat method on the server
                if ($.connection.hub.state === $.connection.connectionState.connected) {
                    //console.log("connected");
                    chatHub.server.send(msgSend, this.roomId);
                    //clear the textbox for the next message
                    this.newMessageText('');
                    showStatus(stateConnected);
                } else {
                    alert(errorSendingMessage);
                    showStatus(errorSendingMessage);
                }
            }
        };

        this.disconnectRoom = function () {
            chatHub.server.leaveRoom(this.roomId, moduleid);
            userRoomModel.rooms.remove(this);
            userRoomModel.activeRoom(defaultRoomId);
        };
             
        this.joinRoom = function () {
            //check if the userid >0 otherwise don't let them join
            if (chatHub.state.userid > 0 || this.roomId === defaultRoomId) {
                var foundRoom = findRoom(this.roomId); //check if the user is already in this room
                if (!foundRoom) {
                    if (this.roomId != userRoomModel.activeRoom) {
                        //check if a password is required for the room
                        if (this.private) { //
                            //this is firing for page refreshes as well. Need to determine how to handle that
                            var password = getPassword();
                            chatHub.server.getRoomInfo(this.roomId, moduleid, password);
                            this.setActiveRoom();
                        } else {
                            chatHub.server.getRoomInfo(this.roomId, moduleid);
                            this.setActiveRoom();
                        }
                    }
                    if ($(".RoomList").hasClass('ui-dialog-content')) {
                        $(".RoomList").dialog('close');
                    }
                } else {
                    alert(alreadyInRoom);
                    if ($(".RoomList").hasClass('ui-dialog-content')) {
                        $(".RoomList").dialog('close');
                    }
                }
            } else {
                alert(anonUsersRooms);
                if ($(".RoomList").hasClass('ui-dialog-content')) {
                    $(".RoomList").dialog('close');
                }
            }
            userRoomModel.sortRoomsAscending();
        };
    }

    function findRoom(rId) {
        return ko.utils.arrayFirst(userRoomModel.rooms(), function (room) {
            return room.roomId === rId;
        });
    }

    function getPassword() {
        return window.prompt(messagePasswordEntry, "");
    };


    var chatHub = $.connection.chatHub;
    $.connection.hub.logging = false;

    //define the client state with information from DNN, this will get used after the connection starts
    chatHub.state.moduleid = moduleid;
    chatHub.state.userid = userid;
    chatHub.state.username = username;
    chatHub.state.startMessage = startmessage;
    chatHub.state.defaultRoomId = defaultRoomId;
    chatHub.state.userroles = userroles;

    // Declare a function to actually create a message on the chat hub so the server can invoke it
    chatHub.client.newMessage = function (data) {
        var m = new Message(data);

        //lookup the proper ROOM in the array and push a message to it
        var curRoom = findRoom(m.roomId);
        if (curRoom) {
            curRoom.addMessage(m);
        } else {
            //If the room isn't found display an alert
            alert(messageMissingRoom);
        }

        if (focus === false) {
            //handle new messages if window isn't in focus
            updateUnread(checkMention(m.messageText, chatHub.state.username));
        }
    };

    chatHub.client.newMessageNoParse = function (data) {

        var m = new Message(data);
        var curRoom = findRoom(m.roomId);
        if (curRoom) {
            curRoom.addSystemMessage(m);
        } else {
            //If the room isn't found display an alert
            alert(messageMissingRoom);
        }
    };


    chatHub.client.deleteMessage = function (data) {
        var m = new Message(data);
        var curRoom = findRoom(m.roomId);
        if (curRoom) {
            curRoom.deleteMessage(m);
        }
    };

    //wire up the click handler for the button after the connection starts
    this.init = function (element) {
        $.connection.hub.start().done(function () {
            //nothing to do here?
        });

    };

    $.connection.hub.starting(function () {
        showStatus(stateConnected);
    });
    //logic below based on code from Jabbr (http://jabbr.net)
    $.connection.hub.stateChanged(function (change) {
        if (change.newState === $.connection.connectionState.reconnecting) {
            //do something on reconnect   
            showStatus(stateReconnecting);
        }

        else if (change.newState === $.connection.connectionState.disconnected) {
            showStatus(stateDisconnected);
        }

        else if (change.newState === $.connection.connectionState.connected) {
            if (!firstConnection) {
                //do something on subsequent connections
                showStatus(stateReconnected);

            } else {
                //do something else on first connection
                showStatus(stateConnected);
            }
        }
    });

    $.connection.hub.disconnected(function () {
        showStatus(stateDisconnected);
        // Restart the connection
        setTimeout(function () {
            $.connection.hub.start();
        }, 5000);
    });


    $.connection.hub.connectionSlow(function () {
        showStatus(stateConnectionSlow);
    });

    chatHub.client.join = function () {
        //fire the connection back to ChatHub that allows us to access the state, and join rooms
        chatHub.server.join();
    };

    //when a connection starts we can't use the "state", the properties defined above, so we have to fire this method after that connection starts
    chatHub.client.populateUser = function (allRooms, myRooms) {
        $.each(allRooms, function (i, item) {
            var r = new Room(item);
            roomModel.rooms.push(r);
        });

        //usersViewModel.connectionRecords.removeAll();
        userRoomModel.rooms.removeAll();
        $.each(myRooms, function (i, item) {
            var r = new Room(item);
            r.joinRoom();
            r.sortRoomUsersAscending();
        });
        chatHub.state.startMessage = "";
        userRoomModel.sortRoomsAscending();
    };

    chatHub.client.fillLobby = function (allRooms) {
        roomModel.rooms.removeAll();
        $.each(allRooms, function (i, item) {
            var r = new Room(item);
            roomModel.rooms.push(r);
        });
    };

    chatHub.client.messageJoin = function (item) {
        var r = new Room(item);
        r.joinRoom();
    };

    chatHub.client.joinRoom = function (item) {
        var r = new Room(item);
        var foundRoom = findRoom(r.roomId);
        if (!foundRoom) {
            userRoomModel.rooms.push(r);
            chatHub.server.joinRoom(r.roomId, moduleid);
        }
    };

    chatHub.client.badPassword = function (badPasswordMessage) {
        alert(badPasswordMessage);
        //if you entered a bad password, we'll force you to the default room
        userRoomModel.activeRoom(defaultRoomId);
    };

    chatHub.client.scrollBottom = function (roomId) {
        var parentDiv = "#room-" + roomId;
        $(parentDiv).scrollTop($(parentDiv)[0].scrollHeight);
    };

    //this method get's called from the Hub when you update your 
    //name using the /nick SOMETHING call in the text window
    chatHub.client.updateName = function (newName) {
        chatHub.state.username = newName;
    };

    var emoticons = {
        ':-)': 'smiling.png',
        ':)': 'smiling.png',
        '=)': 'smiling.png',
        ';)': 'winking.png',
        ';P': 'winking_tongue_out.png',
        ';D': 'winking_grinning.png',
        ':D': 'grinning.png',
        '=D': 'grinning.png',
        ':P': 'tongue_out.png',
        ':(': 'frowning.png',
        ':\\': 'unsure_2.png',
        ':|': 'tired.png',
        '>:D': 'malicious.png',
        '>:)': 'spiteful.png',
        '(Y)': 'thumbs_up.png',
        '(N)': 'thumbs_down.png'
    }, url = emoticonsUrl, patterns = [],
            metachars = /[[\]{}()*+?.\\|^$\-,&#\s]/g;

    // build a regex pattern for each defined property
    for (var i in emoticons) {
        if (emoticons.hasOwnProperty(i)) { // escape metacharacters
            patterns.push('(' + i.replace(metachars, "\\$&") + ')');
        }
    }

    function replaceMessage(message) {
        //urls
        var exp = /(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig;
        message.messageText = message.messageText.replace(exp, "<a href='$1' target='_blank'>$1</a>");

        //emoticons
        message.messageText = message.messageText.replace(new RegExp(patterns.join('|'), 'g'), function (match) {
            return typeof emoticons[match] != 'undefined' ? '<img src="' + url + emoticons[match] + '"/>' : match;
        });

        //check for long string of characters
        return message;
    }

    //take all the users and put them in the view model
    chatHub.client.updateUserList = function (data, roomId) {
        var curRoom = findRoom(roomId);
        if (curRoom) {
            curRoom.removeConnectionRecords();
            $.each(data, function (i, item) {
                var cr = new ConnectionRecord(item);
                //lookup the proper ROOM in the array and push the connection to it
                curRoom.connectionRecords.push(cr);
            });
        }

        //sort the list of users
        //usersViewModel.connectionRecords.sort(function (left, right) { return left.authorName == right.authorName ? 0 : (left.authorName.toLowerCase() < right.authorName.toLowerCase() ? -1 : 1); });
        curRoom.sortRoomUsersAscending();
        //update the online user count

        //$('#currentCount').text(data.length);
    };

    //TODO: handle these click events with knockout

    //todo: this is still being used at the Page level notifications, but not for the Room counts
    function updateUnread(mentioned) {
        if (focus === false) {
            if (mentioned === true)
                mentions = mentions + 1;
            unread = unread + 1;
            SetTitle();
        }
    }

    function SetTitle() {
        if (focus == false) {
            document.title = "(" + unread + ") " + "(" + mentions + ") " + pageTitle;
        } else {
            document.title = pageTitle;
        }
    }

    function checkMention(messageText, un) {
        if (String(messageText).toLowerCase().indexOf(String(un).toLowerCase()) !== -1) {
            {
                return true;
            }
        }
        /* this is used to see if someone said "@all" and makes sure to notify everyone */
        if (String(messageText).toLowerCase().indexOf(String(allUsersNotification).toLowerCase()) !== -1) {
            {
                return true;
            }
        }

        return false;
    }

    //for autocomplete of usernames look at 
    //http://stackoverflow.com/questions/7537002/autocomplete-combobox-with-knockout-js-template-jquery 

    ko.applyBindings(userRoomModel, document.getElementById('userRoomList'));
    ko.applyBindings(userRoomModel, document.getElementById('roomView'));
    ko.applyBindings(roomModel, document.getElementById('roomList'));
}

/* used to format the counters when a room isn't active */
function formatCount(value) {
    if (value > 0)
        return "(" + value + ")";
    else {
        return "";
    }
}

function showStatus(message) {
    $('#ChatStatus').html(message);
}
