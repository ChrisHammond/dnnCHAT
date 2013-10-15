<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Christoc.Modules.DnnChat.View" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<dnn:DnnJsInclude runat="server" FilePath="~/desktopmodules/DnnChat/Scripts/jquery.signalR-1.1.3.min.js" Priority="10" />
<dnn:DnnJsInclude runat="server" FilePath="~/signalr/hubs" Priority="100" />


<script type="text/javascript">
    /*knockout setup for user*/
    /*currently we are getting the list of initial users, but no updates for adding/removing*/

    jQuery(document).ready(function ($) {
        var md = new DnnChat($, ko, {
            moduleId:<% = ModuleId %>,
            userId:<%=UserId%>,
            userName:'<%=UserInfo.DisplayName%>',
            startMessage:'<%=StartMessage%>',
            sendMessageReconnecting:'<%=Localization.GetString("SendMessageReconnecting.Text",LocalResourceFile)%>',
            stateReconnecting:'<%=Localization.GetString("StateReconnecting.Text",LocalResourceFile)%>',
            stateReconnected:'<%=Localization.GetString("StateReconnected.Text",LocalResourceFile)%>',
            stateConnected:'<%=Localization.GetString("StateConnected.Text",LocalResourceFile)%>',
            stateDisconnected:'<%=Localization.GetString("StateDisconnected.Text",LocalResourceFile)%>',
            stateConnectionSlow:'<%=Localization.GetString("StateConnectionSlow.Text",LocalResourceFile)%>',
            emoticonsUrl:'<%= ResolveUrl(ControlPath + "images/emoticons/simple/") %>',
            alreadyInRoom:'<%=Localization.GetString("AlreadyInRoom.Text",LocalResourceFile)%>',
            anonUsersRooms:'<%=Localization.GetString("AnonymousJoinDenied.Text",LocalResourceFile)%>',
            messageMissingRoom: '<%=Localization.GetString("MessageMissingRoom.Text",LocalResourceFile)%>',
            errorSendingMessage:'<%=Localization.GetString("ErrorSendingMessage.Text",LocalResourceFile)%>',
            defaultRoomId:'<%=DefaultRoomId %>'
            
        });
        md.init('#messages');
    });
    
</script>

<div class="LobbyArea dnnClear" id="roomList">

    <div class="ShowRoomListButton dnnPrimaryAction" data-bind="click:$root.ShowRoomList">
        <%=Localization.GetString("showRoomList.text",LocalResourceFile) %>
    </div>
    <div class="RoomList" style="display: none;" title="<%=Localization.GetString("joinARoom.Text",LocalResourceFile) %>">
        <!-- ko foreach: rooms -->
        <div data-bind="html:roomName,click:joinRoom" class="RoomListRoom">
        </div>
        <!-- /ko -->
    </div>
</div>

<div class="ConnectedRoomList" id="userRoomList">
    <!-- ko foreach: rooms -->
    <div class="ChatRooms">
        <div class="ConnectedRoomTab" data-bind="id:roomName,click:setActiveRoom,css:{activeRoom:roomId == $parent.activeRoom()}">
            <div data-bind="html:roomName" class="ConnectedRoom">
            </div>
            <div data-bind="html:formatCount(awayMessageCount())" class="roomAwayMessageCount"></div>
            <div data-bind="html:formatCount(awayMentionCount())" class="roomAwayMentionCount"></div>

            <div data-bind="click:disconnectRoom" class="RoomClose"></div>
        </div>
    </div>
    <!-- /ko -->
</div>

<div class="RoomContainer dnnClear" id="roomView">
    <!-- ko foreach: rooms -->
        <!-- the display of the rooms that a user is connected -->
        <div class="srcWindow" data-bind="visible:showRoom">
            <div class="ChatWindow" data-bind="attr:{id: roomNameId}">
                <!-- ko foreach: messages -->
                <div data-bind="attr:{class:cssName}">
                    <div data-bind="html:authorName,click:targetMessageAuthor" class="MessageAuthor"></div>
                    <div data-bind="html:messageText" class="MessageText"></div>
                    <div data-bind="dateString: messageDate" class="MessageTime"></div>
                </div>
                <!-- /ko -->
            </div>
            <div class="UsersList" id="userList">
                <!-- ko foreach: connectionRecords -->
                <div class="ChatUsers">
                    <!-- ko if: userId>0 -->
                    <div data-bind="html:authorName,click:targetMessageAuthor" class="UserListUser UserLoggedIn">
                    </div>
                    <!-- /ko -->
                    <!-- ko if: userId<1 -->
                    <div data-bind="html:authorName,click:targetMessageAuthor" class="UserListUser UserNotLoggedIn">
                    </div>
                    <!-- /ko -->
                </div>
                <!-- /ko -->
            </div>

            <input type="text" data-bind="value:newMessageText, hasfocus: textFocus, enterKey: sendMessage" class="msg" />
            <input class="dnnPrimaryAction" type="button" value="<%= Localization.GetString("btnSubmit.Text",LocalResourceFile)%>" data-bind="click:sendMessage" />

            <div class="dnnRight usersOnline">
                <%= Localization.GetString("usersOnline.Text",LocalResourceFile)%><div data-bind="html:userCount" class="dnnRight"></div>
            </div>
        </div>
    <!-- /ko -->
</div>
            <div id="ChatStatus" class="chatStatus dnnClear">
            </div>

<div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",LocalResourceFile)%></div>
