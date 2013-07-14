//TODO: 7/11/2013   User Counts in Room isn't doing anything

//TODO: 7/11/2013   Highlight the active Room
//TODO: 7/11/2013   Welcome messages for Rooms aren't loading
//TODO: 7/11/2013   If you "connect" you aren't reconnected to your previous rooms
//TODO: 7/13/2013   Auto scrolling doesn't work

//TODO: 7/13/2013   if you've got any open connection records things go crazy (multiple connections to a room)

//TODO: the connection fails with websockets and no fall back
//TODO: reconnections appear to keep happening for logged in users, populating the user list multiple times

function DnnChat($, ko, settings) {

    var moduleid = settings.moduleId;
    var userid = settings.userId;
    var username = settings.userName;
    var startmessage = settings.startMessage;
    var sendMessageReconnecting = settings.sendMessageReconnecting;
    var stateReconnecting = settings.stateReconnecting;

    var stateReconnected = settings.stateReconnected;
    var stateConnected = settings.stateConnected;
    var stateDisconnected = settings.stateDisconnected;
    var alreadyInRoom = settings.alreadyInRoom;
    var defaultRoomId = settings.defaultRoomId;

    var emoticonsUrl = settings.emoticonsUrl; //<%= ResolveUrl(ControlPath + "images/emoticons/simple/") %>

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
            ko.utils.registerEventHandler(element, "keyup", function (event) {
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
        this.roomId = m.RoomId;

        //this.cssName = m.MessageText.toLowerCase().indexOf(chatHub.state.username.toLowerCase()) !== -1 ? "ChatMessage ChatMentioned dnnClear" : "ChatMessage dnnClear";
        //patch from @briandukes to highlight your own posts
        this.cssName = "ChatMessage dnnClear";
        if (checkMention(m.MessageText, chatHub.state.username)) {
            this.cssName += " ChatMentioned";
        }
        if (m.AuthorName === chatHub.state.username) {
            this.cssName += " ChatSelf";
        }
        this.targetMessageAuthor = function () {
            //$('#msg').val($('#msg').val() + ' @' + $(this).text() + ' ').focus();
            alert(m.RoomId);
            var parentRoom = findRoom(m.roomId);
            if (parentRoom) {
                alert('parent text: ' + parentRoom.newMessageText());
            }
            
            //parent.newMessageText().(parent.newMessageText() + ' @' + this.authorName + ' ').focus();
        };
    }

    var messageModel = {
        messages: ko.observableArray([])
    };

    //used for the list of rooms
    var roomModel = {
        rooms: ko.observableArray([]),
        ShowLobby: function () {
            $(".LobbyRoomList").dialog({
                width: '600px',
                modal: true
                , dialogClass: "dnnFormPopup"
            });
        },
        HideLobby: function () {
            $(".LobbyRoomList").hide();
        }
    };

    //used to manage which rooms a user is in
    var userRoomModel = {
        rooms: ko.observableArray([])
        , activeRoom: ko.observable(activeRoomId)
    };

    //Room mapping function
    function Room(r) {

        this.roomId = r.RoomId;
        this.roomName = r.RoomName;
        this.roomDescription = r.RoomDescription;
        //this.roomCount = r.RoomCount; //TODO: implement a count of how many people are in the room

        this.messages = ko.observableArray([]);
        this.connectionRecords = ko.observableArray([]);

        //add a message without parsing
        this.addSystemMessage = function (m) {
            this.messages.push(m);
        }.bind(this);
        
        this.addMessage = function (m) {
            this.messages.push(replaceMessage(m));
        }.bind(this);
        this.addConnectionRecord = function (cr) {
            this.connectionRecords.push(cr);
        }.bind(this);

        this.removeConnectionRecords = function () {
            this.connectionRecords.removeAll();
        };

        //this.visible = ko.observable(true);


        this.addOnEnter = function(event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            alert(event.keyCode);
            if (keyCode === 13) {
                this.sendMessage();
                return false;
            }
            return true;
        };

        this.setActiveRoom = function () {
            userRoomModel.activeRoom(this.roomId);
        };

        this.showRoom = ko.computed(function () {
            return this.roomId === userRoomModel.activeRoom();
        }, this);


        //clear out the message text to start
        this.newMessageText = ko.observable("");

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
                } else if ($.connection.hub.state === $.connection.connectionState.reconnecting) {
                    chatHub.state.moduleid = moduleid;
                    chatHub.state.userid = userid;
                    chatHub.state.username = username;
                    //start the connection again -should handle this better
                    showStatus(sendMessageReconnecting);
                }
            }
        };

        this.disconnectRoom = function () {
            chatHub.server.leaveRoom(this.roomId, moduleid);
            userRoomModel.rooms.remove(this);
            userRoomModel.activeRoom(defaultRoomId);
        };

        this.joinRoom = function () {
            var foundRoom = findRoom(this.roomId);
            if (!foundRoom) {
                if (this.roomId != userRoomModel.activeRoom) {
                    chatHub.server.getRoomInfo(this.roomId, moduleid);
                    this.setActiveRoom();
                }
                $(".LobbyRoomList").dialog('close');
            } else {
                alert(alreadyInRoom);
            }
        };

        //TODO: create an observable for Unread messages and Mentions counts
    }

    function findRoom(rId) {
        return ko.utils.arrayFirst(userRoomModel.rooms(), function (room) {
            return room.roomId === rId;
        });
    }

    var chatHub = $.connection.chatHub;
    $.connection.hub.logging = false;

    //define the client state with information from DNN, this will get used after the connection starts
    chatHub.state.moduleid = moduleid;
    chatHub.state.userid = userid;
    chatHub.state.username = username;
    chatHub.state.startMessage = startmessage;

    // Declare a function to actually create a message on the chat hub so the server can invoke it
    chatHub.client.newMessage = function (data) {
        var m = new Message(data);

        //lookup the proper ROOM in the array and push a message to it
        var curRoom = findRoom(m.roomId);
        if (curRoom) {
            curRoom.addMessage(m);
        } else {
            //If the room isn't found display an alert
            //TODO: localize this
            alert('Message received for a room you aren\'t connected to');
        }   
        //Original messageModel pushing
        //messageModel.messages.push(replaceMessage(m));

        if (focus === false) {
            //handle new messages if window isn't in focus
            updateUnread(checkMention(m.messageText, chatHub.state.username));
            if ($("#messages").scrollTop() + $("#messages").height() < $("#messages")[0].scrollHeight - 250) {
                //pause

            } else {
                $("#messages").scrollTop($("#messages")[0].scrollHeight);
            }
        } else {

            //if we are in focus, and we are currently within 10% of the bottom
            //we want to make sure to scroll to the bottom of the DIV when a new message posts
            //check if the current scroll position + the height of the div (default 500) is less than the overall height of the div minus 100
            //randomly picked -100 here
            //TODO: figure out a better way to check what 100 should be 
            if ($("#messages").scrollTop() + $("#messages").height() < $("#messages")[0].scrollHeight - 250) {
                //pause

            } else {
                $("#messages").scrollTop($("#messages")[0].scrollHeight);
            }
        }
    };

    chatHub.client.newMessageNoParse = function (data) {

        var m = new Message(data);

//        alert('Message RoomId:' + m.roomId);
        var curRoom = findRoom(m.roomId);
        if (curRoom) {
            curRoom.addSystemMessage(m);
        } else {
            //If the room isn't found display an alert
            //TODO: localize this
            alert('Message received for a room you aren\'t connected to: newMessageNoParse');
            alert('RoomId:' + m.roomId + ' Message: ' + m.messageText);
        }

        //messageModel.messages.push(m);
        //we want to make sure to scroll to the bottom of the DIV when a new message posts
        //check if the current scroll position + the height of the div (default 500) is less than the overall height of the div minus 100
        //randomly picked -100 here
        //TODO: figure out a better way to check what 100 should be 

        if ($(".chatMessages").scrollTop() + $(".chatMmessages").height() < $(".chatMessages")[0].scrollHeight - 100) {
            //pause the scroll

        } else {
            $(".chatMessages").scrollTop($(".chatMessages")[0].scrollHeight);
        }
    };

    //TODO: handle state better if a connection is lost

    //wire up the click handler for the button after the connection starts
    this.init = function (element) {
        $.connection.hub.start().done(function () {
            //set the default room?
            //usersViewModel.activeRoom(settings.defaultRoomId);
            //TODO: do anything here?
            //btnSubmit.click();
        });
    };

    //logic below based on code from Jabbr (http://jabbr.net)
    $.connection.hub.stateChanged(function (change) {
        if (change.newState === $.connection.connectionState.reconnecting) {
            //do something on reconnect   
            showStatus(stateReconnecting);
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
        });

        chatHub.state.startMessage = "";
    };

    chatHub.client.joinRoom = function (item) {
        var r = new Room(item);
        userRoomModel.rooms.push(r);
        chatHub.server.joinRoom(r.roomId, moduleid);
    };

    //this method get's called from the Hub when you update your name using the /name SOMETHING call in the text window
    chatHub.client.updateName = function (newName) {
        chatHub.state.username = newName;
    };

    //handle the return/enter key press within the module (but not within other modules)
    //TODO: handle keypress for Enter/Return
    $(".msg").keypress(function (e) {
        if (e.which == 13) {
            e.preventDefault();
            //btnSubmit.click();
        }
    });

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
                //TODO: figure out how to push to a specific Room's connection records
                var cr = new ConnectionRecord(item);

                //lookup the proper ROOM in the array and push the connection to it
                curRoom.connectionRecords.push(cr);

                //usersViewModel.connectionRecords.push(new ConnectionRecord(item));
            });
        }

        //sort the list of users
        usersViewModel.connectionRecords.sort(function (left, right) { return left.authorName == right.authorName ? 0 : (left.authorName.toLowerCase() < right.authorName.toLowerCase() ? -1 : 1); });

        //TODO: configure the Count for a specific room
        //update the online user count
        $('#currentCount').text(data.length);
    };

    //TODO: handle these click events with knockout

    //TODO: userlist should be per room
    $('#userList').on('click', '.UserListUser', function () {
        $('#msg').val($('#msg').val() + ' @' + $(this).text() + ' ').focus();
    });

    //TODO: messages need to be per room
    $(".chatMessages").on('click', '.MessageAuthor', function () {
        $('#msg').val($('#msg').val() + ' @' + $(this).text() + ' ').focus();
    });

    function updateUnread(mentioned) {
        if (focus === false) {
            if (mentioned === true)
                mentions = mentions + 1;
            unread = unread + 1;
            SetTitle();
        }
    }

    //TODO: modify the titles of the tabs for each room with notifications
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
        return false;
    }

    //for autocomplete of usernames look at 
    //http://stackoverflow.com/questions/7537002/autocomplete-combobox-with-knockout-js-template-jquery 


    ko.applyBindings(userRoomModel, document.getElementById('userRoomList'));
    ko.applyBindings(userRoomModel, document.getElementById('roomView'));

    ko.applyBindings(roomModel, document.getElementById('roomList'));


}


function showStatus(message) {
    $('#ChatStatus').html(message);
}
