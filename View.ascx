<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Christoc.Modules.DnnChat.View" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<dnn:DnnJsInclude runat="server" FilePath="~/desktopmodules/DnnChat/Scripts/jquery.signalR-2.4.1.min.js" Priority="10" />
<dnn:DnnJsInclude runat="server" FilePath="~/signalr/hubs" Priority="100" />


<script type="text/javascript">
    /*knockout setup for user*/
    jQuery(document).ready(function ($) {
        
        var md = new DnnChat($, ko, {
            moduleId:<% = ModuleId %>,
            userId:<%=UserId%>,
            userName:'<%=UserInfo.DisplayName%>',
            startMessage:'<%=StartMessage%>',
            defaultAvatarUrl:'<%=DefaultAvatarUrl%>',
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
            messagePasswordEntry:'<%=Localization.GetString("MessagePasswordEntry.Text",LocalResourceFile)%>',
            errorSendingMessage:'<%=Localization.GetString("ErrorSendingMessage.Text",LocalResourceFile)%>',
            roomArchiveLink: '<%=EditUrl(string.Empty,string.Empty,"Archive","&roomid=0") %>',
            defaultRoomId:'<%=DefaultRoomId %>',
            roles:'<%=EncryptedRoles%>',
            //todo: we should populate a different messagedeleteconfirm if you don't have permissions to delete
            messageDeleteConfirmation: '<%=Localization.GetString("MessageDeleteConfirm.Text",LocalResourceFile)%>',
            allUsersNotification: '<%=Localization.GetString("AllUsersNotification.Text",LocalResourceFile)%>',
        });
        md.init('#messages');
    });
    
</script>

<div class="LobbyArea dnnClear" id="roomList">

    <div class="ShowRoomListButton dnnPrimaryAction" data-toggle="modal" data-target="#RoomListModal">
        <%=Localization.GetString("showRoomList.text",LocalResourceFile) %>
    </div>
    <div class="modal RoomList" style="display: none;" id="RoomListModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="myModalLabel"><%=Localization.GetString("joinARoom.Text",LocalResourceFile) %></h4>
                </div>
                <div class="modal-body">
                    <ul class="list-group">
                        <!-- ko foreach: rooms -->
                        <li data-bind="html:roomName,click:joinRoom" class="list-group-item RoomListRoom" data-dismiss="modal"></li>
                        <!-- /ko -->
                    </ul>
                    
                </div>
            </div>
        </div>
    </div>
</div>

<ul class="nav nav-tabs col-lg-10" id="userRoomList">
    <!-- ko foreach: rooms -->
    <li class="ChatRooms" data-bind="id:roomName,click:setActiveRoom,css:{'active':roomId == $parent.activeRoom()}" role="presentation">
        <a href="#">
            <div data-bind="html:roomName" class="ConnectedRoom">
            </div>
            <div data-bind="html:formatCount(awayMessageCount())" class="roomAwayMessageCount"></div>
            <div data-bind="html:formatCount(awayMentionCount())" class="roomAwayMentionCount"></div>
            <div data-bind="click:disconnectRoom" class="RoomClose glyphicon glyphicon-remove"></div>
            &nbsp;
            
        </a>
    </li>
    <!-- /ko -->
</ul>

<div class="RoomContainer container" id="roomView">
    <!-- ko foreach: rooms -->
    <!-- the display of the rooms that a user is connected -->
    <div class="row" data-bind="visible:showRoom">
        <div class="col-lg-10 container chatWrap">
            <div class="ChatWindow" data-bind="attr:{id: roomNameId}">
                <ul class="list-group">
                    <!-- ko foreach: messages -->
                    <li class="list-group-item row">
                        <div data-bind="attr:{class:cssName}">
                            <div class="col-lg-2 MessageAuthor smallPad dnnClear ">
                                <!-- ko if: authorUserId>0 -->
                                <img data-bind="attr: {src:photoUrl,alt:authorName},click:targetMessageAuthor" class="MessageAuthorPhoto" />
                                <!-- /ko -->
                                <!-- ko if: authorUserId<1 -->
                                <img data-bind="attr: {src:defaultAvatarUrl,alt:authorName},click:targetMessageAuthor" class="MessageAuthorPhoto" />
                                <!-- /ko -->
                                <div data-bind="html:authorName,click:targetMessageAuthor" class="MessageAuthorText"></div>
                            </div>
                            <div data-bind="html:messageText" class="col-lg-9 MessageText smallPad "></div>
                            <div data-bind="dateString: messageDate, click:deleteMessage" class=" col-lg-1 MessageTime smallPad"></div>
                        </div>
                    </li>
                    <!-- /ko -->
                </ul>
            </div>
            <input type="text" data-bind="value:newMessageText, hasfocus: textFocus, enterKey: sendMessage" class="msg" />
            <input class="dnnPrimaryAction" type="button" value="<%= Localization.GetString("btnSubmit.Text",LocalResourceFile)%>" data-bind="click:sendMessage" />
        </div>

        <div class="UsersList col-lg-2 container" id="userList">
            <div class="row usersOnline">
                <div class="col-xs-12">
                    <%= Localization.GetString("usersOnline.Text",LocalResourceFile)%><div data-bind="html:userCount" class="dnnRight"></div>
                </div>
            </div>
            <ul class="list-group chatUsers">
                <!-- ko foreach: connectionRecords -->

                <!-- ko if: userId>0 -->
                <li class="list-group-item smallPad">
                    <img data-bind="attr: {src:photoUrl},click:targetMessageAuthor" class="UserListPhoto" />
                    <div data-bind="html:authorName,click:targetMessageAuthor" class="UserListUser UserLoggedIn"></div>
                </li>
                <!-- /ko -->
                <!-- ko if: userId<1 -->
                <li data-bind="html:authorName,click:targetMessageAuthor" class="list-group-item UserListUser UserNotLoggedIn smallPad"></li>
                <!-- /ko -->
                <!-- /ko -->
            </ul>
        </div>
    </div>
    <div><a data-bind="attr:{href: roomArchiveLink},visible:showRoom" target="_blank"><%=Localization.GetString("Archives.Text",LocalResourceFile) %></a></div>
    <!-- /ko -->
</div>
<div class="container">
    <div class="row">
        <div id="ChatStatus" class="chatStatus col-lg-12">
        </div>
    </div>
    <div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",LocalResourceFile)%></div>
</div>

