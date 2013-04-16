<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Christoc.Modules.DnnChat.View" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<script src="/desktopmodules/DnnChat/Scripts/jquery.signalR-1.0.1.min.js" type="text/javascript"></script>
<script type="text/javascript" src='<%= ResolveClientUrl("~/signalr/hubs") %>'></script>

<script type="text/javascript">
    /*knockout setup for user*/
    /*currently we are getting the list of initial users, but no updates for adding/removing*/

    $(function () {

        var focus = true;
        var pageTitle = document.title;
        var unread = 0;
        var mentions = 0;
        var firstConnection = true;

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

        //message mapping function
        function Message(m) {
            this.messageId = m.MessageId;
            this.connectionId = m.ConnectionId;
            this.messageText = m.MessageText;
            this.messageDate = m.MessageDate;
            this.authorName = m.AuthorName;

            //this.cssName = m.MessageText.toLowerCase().indexOf(chatHub.state.username.toLowerCase()) !== -1 ? "ChatMessage ChatMentioned dnnClear" : "ChatMessage dnnClear";
            //patch from @briandukes to highlight your own posts
            this.cssName = "ChatMessage dnnClear";
            if (checkMention(m.MessageText,chatHub.state.username)) {
                this.cssName += " ChatMentioned";
            }
            if (m.AuthorName === chatHub.state.username) {
                this.cssName += " ChatSelf";
            }
        }


        var messageModel = {
            messages: ko.observableArray([])
        };

        var btnSubmit = $("#btnSubmit");
        var chatHub = $.connection.chatHub;
        $.connection.hub.logging = false;

        //define the client state with information from DNN, this will get used after the connection starts
        chatHub.state.moduleid = '<%=ModuleId%>';
        chatHub.state.userid = '<%=UserId%>';
        chatHub.state.username = '<%=ChatNick%>';
        chatHub.state.startMessage = '<%=StartMessage%>';


        // Declare a function to actually create a message on the chat hub so the server can invoke it
        chatHub.client.newMessage = function (data) {
            var m = new Message(data);
            
            messageModel.messages.push(replaceMessage(m));
            //we want to make sure to scroll to the bottom of the DIV when a new message posts
            $("#messages").scrollTop($("#messages")[0].scrollHeight);

            if (focus === false) {
                //handle new messages if window isn't in focus
                updateUnread(checkMention(m.messageText, chatHub.state.username));
            }
        };

        chatHub.client.newMessageNoParse = function (data) {
            var m = new Message(data);
            messageModel.messages.push(m);
            //we want to make sure to scroll to the bottom of the DIV when a new message posts
            $("#messages").scrollTop($("#messages")[0].scrollHeight);
        };

        //TODO: handle state better if a connection is lost

        //wire up the click handler for the button after the connection starts
        $.connection.hub.start().done(function () {

            btnSubmit.click(function () {
                //remove all HTML tags first for safety
                var msgSend = $.trim($('#msg').val().replace(/(<([^>]+)>)/ig, ""));
                
                //make sure the chat string isn't empty
                if (msgSend != '') {
                    // Call the chat method on the server
                    if ($.connection.hub.state === $.connection.connectionState.connected) {
                        //console.log("connected");
                        chatHub.server.send(msgSend);
                        //clear the textbox for the next message
                        $('#msg').val('');
                    } else if ($.connection.hub.state === $.connection.connectionState.reconnecting) {

                        chatHub.state.moduleid = '<%=ModuleId%>';
                        chatHub.state.userid = '<%=UserId%>';
                        chatHub.state.username = '<%=ChatNick%>';
                        
                        //start the connection again -should handle this better
                        showStatus('<%=Localization.GetString("SendMessageReconnecting.Text",LocalResourceFile)%>'); 

                        //clear the textbox for the next message
                        //$('#msg').val('');
                    }
                
                }
            });
        });
        
        //logic below based on code from Jabbr (http://jabbr.net)
        $.connection.hub.stateChanged(function (change) {
            if (change.newState === $.connection.connectionState.reconnecting) {
                //do something on reconnect   
                showStatus('<%=Localization.GetString("StateReconnecting.Text",LocalResourceFile)%>');
            }
            else if (change.newState === $.connection.connectionState.connected) {
                if (!firstConnection) {
                    //do something on subsequent connections
                    
                    showStatus('<%=Localization.GetString("StateReconnected.Text",LocalResourceFile)%>');

                } else {
                    
                    //do something else on first connection
                    showStatus('<%=Localization.GetString("StateConnected.Text",LocalResourceFile)%>');
                }
            }
        });
        $.connection.hub.disconnected(function () {
            
            showStatus('<%=Localization.GetString("StateDisconnected.Text",LocalResourceFile)%>');
            
            // Restart the connection
            setTimeout(function () {
               $.connection.hub.start();
            }, 5000);
        });

        //when a connection starts we can't use the "state", the properties defined above, so we have to fire this method after that connection starts
        chatHub.client.populateUser = function () {
            chatHub.server.populateUser();
            chatHub.state.startMessage = "";
        };

        //this method get's called from the Hub when you update your name using the /name SOMETHING call in the text window
        chatHub.client.updateName = function (newName) {
            chatHub.state.username = newName;
        };

        //handle the return/enter key press within the module
        $(document).keypress(function (e) {
            if (e.which == 13) {
                e.preventDefault();
                btnSubmit.click();
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
        }, url = '<%= ResolveUrl(ControlPath + "images/emoticons/simple/") %>', patterns = [],
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

            return message;
        }
        
        //take all the users and put them in the view model
        chatHub.client.updateUserList = function (data) {
            usersViewModel.connectionRecords.removeAll();
            $.each(data, function (i, item) {
                usersViewModel.connectionRecords.push(new ConnectionRecord(item));
            });

            //sort the list of users
            usersViewModel.connectionRecords.sort(function (left, right) { return left.authorName == right.authorName ? 0 : (left.authorName.toLowerCase() < right.authorName.toLowerCase() ? -1 : 1); });

            //update the online user count
            $('#currentCount').text(data.length);
        };

        $('#userList').on('click', '.UserListUser', function () {
            $('#msg').val($('#msg').val() + ' @' + $(this).text() + ' ').focus();
        });

        $("#messages").on('click', '.MessageAuthor', function () {
            $('#msg').val($('#msg').val() + ' @' + $(this).text() + ' ').focus();
        });

        ko.applyBindings(messageModel, document.getElementById('messages'));
        ko.applyBindings(usersViewModel, document.getElementById('userList'));

        function updateUnread(mentioned) {
            if (focus === false) {
                if (mentioned===true)
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
        
        function checkMention(messageText, username) {
            if (String(messageText).toLowerCase().indexOf(String(username).toLowerCase()) !== -1) {
                {
                    return true;
                }
            }
            return false;
        }

        //for autocomplete of usernames look at 
        //http://stackoverflow.com/questions/7537002/autocomplete-combobox-with-knockout-js-template-jquery 
    });


    function showStatus(message) {
        $('#ChatStatus').html(message);
    }


</script>
<div class="srcWindow">
    <div id="messages" class="ChatWindow">
        <!-- ko foreach: messages -->
        <div data-bind="attr:{class:cssName}">
            <div data-bind="html:authorName" class="MessageAuthor"></div>
            <div data-bind="html:messageText" class="MessageText"></div>
            <div data-bind="dateString: messageDate" class="MessageTime"></div>
        </div>
        <!-- /ko -->
    </div>
    <div class="UsersList" id="userList">
        <!-- ko foreach: connectionRecords -->
        <div class="ChatUsers">
            <!-- ko if: userId>0 -->
            <div data-bind="html:authorName" class="UserListUser UserLoggedIn">
            </div>
            <!-- /ko -->
            <!-- ko if: userId<1 -->
            <div data-bind="html:authorName" class="UserListUser UserNotLoggedIn">
            </div>
            <!-- /ko -->

        </div>
        <!-- /ko -->
    </div>
    <input type="text" id="msg" />
    <input id="btnSubmit" class="dnnPrimaryAction" type="button" value="<%= Localization.GetString("btnSubmit.Text",LocalResourceFile)%>" />
    <div class="dnnRight usersOnline">
        <%= Localization.GetString("usersOnline.Text",LocalResourceFile)%><div id="currentCount" class="dnnRight">1</div>
    </div>
    <div id="ChatStatus" class="chatStatus dnnClear">
    </div>
</div>

<div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",LocalResourceFile)%></div>
