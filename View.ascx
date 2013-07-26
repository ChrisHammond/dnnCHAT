<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Christoc.Modules.DnnChat.View" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<dnn:DnnJsInclude runat="server" FilePath="~/desktopmodules/DnnChat/Scripts/jquery.signalR-1.1.2.min.js" Priority="10" />
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
            emoticonsUrl:'<%= ResolveUrl(ControlPath + "images/emoticons/simple/") %>',
            alreadyInRoom:'<%=Localization.GetString("AlreadyInRoom.Text",LocalResourceFile)%>',
            anonUsersRooms:'<%=Localization.GetString("AnonymousJoinDenied.Text",LocalResourceFile)%>',
            messageMissingRoom: '<%=Localization.GetString("MessageMissingRoom.Text",LocalResourceFile)%>',
            defaultRoomId:'<%=DefaultRoomId %>'
            
        });
        md.init('#messages');
    });
    

</script>

<div class="RoomList" id="roomList">

    <div class="ChatRooms" data-bind="click:$root.ShowLobby">
        <%=Localization.GetString("lobbyName.Text",LocalResourceFile) %>
    </div>
    <div class="LobbyRoomList" style="display: none;" title="<%=Localization.GetString("lobbyTitle.Text",LocalResourceFile) %>">
        <!-- ko foreach: rooms -->
        <div data-bind="html:roomName,click:joinRoom" class="LobbyRoom">
        </div>
        <!-- /ko -->
    </div>
</div>

<div class="RoomList" id="userRoomList">
    <!-- ko foreach: rooms -->
    <div class="ChatRooms">
        <div class="RoomListTab" data-bind="id:roomName,click:setActiveRoom,css:{activeRoom:roomId == $parent.activeRoom()}">
            <div data-bind="html:roomName" class="RoomListRoom">
            </div>
            <div data-bind="html:formatCount(awayMessageCount()),visible:(showRoom()!=true && awayMessageCount()>0)" class="roomAwayMessageCount"></div>
            <div data-bind="html:formatCount(awayMessageCount()),visible:(showRoom()!=true && awayMentionCount()>0)" class="roomAwayMentionCount"></div>

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

            <input type="text" data-bind="value:newMessageText, enterKey: sendMessage" class="msg" />
            <input class="dnnPrimaryAction" type="button" value="<%= Localization.GetString("btnSubmit.Text",LocalResourceFile)%>" data-bind="click:sendMessage" />

            <div class="dnnRight usersOnline">
                <%= Localization.GetString("usersOnline.Text",LocalResourceFile)%><div data-bind="html:userCount" class="dnnRight"></div>
            </div>
            <div id="ChatStatus" class="chatStatus dnnClear">
            </div>
        </div>
    <!-- /ko -->
</div>

<div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",LocalResourceFile)%></div>
