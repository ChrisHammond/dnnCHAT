/*
' Copyright (c) 2018 Christoc.com Software Solutions
'  All rights reserved.
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
' and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT 
' SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
' ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
' OR OTHER DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using Microsoft.AspNet.SignalR;

namespace Christoc.Modules.DnnChat.Components
{
    public class ChatHub : Hub
    {
        //a list of connectionrecords to keep track of users connected
        private static readonly List<UserListRecords> Users = new List<UserListRecords>();

        //set the default room based on name (probably should change this somehow for languages)
        //TODO: this only works for a single instance of the module, another instance and all hell might break loose
        private static Guid DefaultRoomId = new Guid(new RoomController().GetRoom("Lobby").RoomId.ToString());

        /*
         * This method is used to send messages to all connected clients.
         */

        //for clients that may call the old method, send to the default room
        public void Send(string message)
        {
            Send(message, DefaultRoomId);
        }

        public void Send(string message, Guid roomId)
        {
            //if no valid connectionrecord don't let the message go through
            var crc = new ConnectionRecordController();
            var cr = crc.GetConnectionRecordByConnectionId(Context.ConnectionId);

            //if the user (connectionrecord) isn't in a room don't let message go through

            var rc = new RoomController();

            if (cr != null && rc.UserInRoom(roomId, cr))
            {
                // parse message before use
                if (Clients.Caller.username != null && Clients.Caller.username.Trim() != "phantom")
                {
                    var parsedMessage = ParseMessage(message, roomId);
                    if (parsedMessage != string.Empty)
                    {
                        int moduleId;
                        int authorUserId;
                        //int.TryParse(Clients.Caller.moduleid, out moduleId);
                        moduleId = Convert.ToInt32(Clients.Caller.moduleid);
                        authorUserId = Convert.ToInt32(Clients.Caller.userid);

                        var m = new Message
                        {
                            ConnectionId = Context.ConnectionId,
                            MessageDate = DateTime.UtcNow,
                            MessageText = parsedMessage,
                            AuthorName = Clients.Caller.username,
                            AuthorUserId = authorUserId,
                            ModuleId = moduleId,
                            RoomId = roomId
                        };

                        new MessageController().CreateMessage(m);
                        Clients.Group(roomId.ToString()).newMessage(m);
                    }
                }
                else
                {
                    // if there is no username for the user don't let them post
                    var m = new Message
                    {
                        ConnectionId = Context.ConnectionId,
                        MessageDate = DateTime.UtcNow,
                        MessageText = Localization.GetString("FailedUnknown.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                        AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                        AuthorUserId = -1,
                        RoomId = DefaultRoomId
                    };
                    Clients.Caller.newMessage(m);
                }
            }
            else
            {
                // if there is no username for the user don't let them post
                var m = new Message
                {
                    ConnectionId = Context.ConnectionId,
                    MessageDate = DateTime.UtcNow,
                    MessageText = Localization.GetString("FailedUnknown.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                    AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                    AuthorUserId = -1,
                    RoomId = DefaultRoomId
                };
                Clients.Caller.newMessage(m);
            }
        }

        public override Task OnConnected()
        {
            Clients.Caller.Join();
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Clients.Caller.PopulateUser();
            return base.OnReconnected();
        }

        //lookup who just disconnected, and store the disconnect/time from the ConnectionRecord but not each room, remove them from the count for each room
        public override Task OnDisconnected(bool stopCalled)
        {
            if (Context.ConnectionId != null) DisconnectUser(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        private void DisconnectUser(string connectionId)
        {
            if (connectionId == null)
                return;
            //TODO: remove user from all rooms
            var crc = new ConnectionRecordController();
            var crrc = new ConnectionRecordRoomController();
            var id = connectionId;
            var cr = crc.GetConnectionRecordByConnectionId(id);
            if (cr != null)
            {
                var roomList = Users.FindAll(c => (c.ConnectionId == connectionId));
                //disconnect from each room the user was connected to
                foreach (UserListRecords rr in roomList)
                {
                    Users.Remove(rr);

                    Clients.Group(rr.RoomId.ToString()).newMessageNoParse(new Message
                    {
                        AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile)
                        ,
                        AuthorUserId = -1
                        ,
                        ConnectionId = "0",
                        MessageDate = DateTime.UtcNow
                        ,
                        MessageId = -1
                        ,
                        MessageText = string.Format(Localization.GetString("Disconnected.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile)

                            , cr.UserName)
                        ,
                        RoomId = rr.RoomId
                    });

                    //Clients.Group(rr.RoomId.ToString()).updateUserList(Users.FindAll(c => (c.RoomId == rr.RoomId)));
                    Clients.Group(rr.RoomId.ToString()).updateUserList(Users.FindAll(uc => (uc.RoomId == rr.RoomId)), rr.RoomId);
                }
                //disconnect the connectionrecord
                cr.DisConnectedDate = DateTime.UtcNow;
                crc.UpdateConnectionRecord(cr);
            }
        }

        private ConnectionRecord SetupConnectionRecord()
        {
            string username = Clients.Caller.username;


            //if (string.IsNullOrEmpty(username))
            //{
            //    Clients.Caller.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = string.Format(Localization.GetString("BadConnection.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), "phantom") });
            //    return new ConnectionRecord();
            //}

            if (username.Trim() == "phantom" || username.Trim() == string.Empty)
            {
                username = string.Format(Localization.GetString("AnonymousUser.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), (Users.Count + 1));
            }

            Clients.Caller.username = username;
            var userId = -1;


            if (Convert.ToInt32(Clients.Caller.userid) > 0)
            {
                userId = Convert.ToInt32(Clients.Caller.userid);
            }

            //check if the connectionrecord is already in the DB

            var crc = new ConnectionRecordController();
            var c = crc.GetConnectionRecordByConnectionId(Context.ConnectionId);
            if (c != null)
            {
                c.UserName = username;
                crc.UpdateConnectionRecord(c);
            }
            else
            {
                c = new ConnectionRecord
                {
                    ConnectionId = Context.ConnectionId,
                    ConnectedDate = DateTime.UtcNow,
                    ModuleId = Convert.ToInt32(Clients.Caller.moduleid),
                    UserName = username,
                    UserId = userId,
                    IpAddress = GetIpAddress()
                };

                //Users.Add(c);
                crc.CreateConnectionRecord(c);
            }
            //store the record for the connection
            return c;
        }

        //TODO: on connection, reload rooms for user?
        public Task Join()
        {
            int moduleId = Convert.ToInt32(Clients.Caller.moduleid);

            var settingsDefault = new Guid(Clients.Caller.defaultRoomId);
            if (DefaultRoomId != settingsDefault)
            {
                DefaultRoomId = settingsDefault;
            }

            //get list of previously connected (not departed) rooms
            var crrc = new ConnectionRecordRoomController();
            var rc = new RoomController();

            IEnumerable<Room> myRooms = null;

            //don't do this if we've got a private room loaded.
            if (Convert.ToInt32(Clients.Caller.userid) > 0)
            {
                myRooms = crrc.GetConnectionRecordRoomsByUserId((int)Clients.Caller.userid);
            }

            //TODO: the default room doesn't have a moduleid associated with it
            //if myRooms is empty, what to do (pass default room)
            if (myRooms == null)
            {
                //load the default room
                var r = rc.GetRoom(DefaultRoomId, moduleId);
                myRooms = new List<Room>();
                myRooms = myRooms.Concat(new[] { r });
            }
            else
            {
                //load the current default room to see if it is in the queue
                var r = rc.GetRoom(DefaultRoomId, moduleId);
                if (!myRooms.Contains(r))
                {
                    myRooms = myRooms.Concat(new[] { r });
                }
            }

            //get all the active rooms and send it back for the Lobby
            var allRooms = rc.GetRooms(moduleId);

            //we are passing in a list of All rooms, and the current user's rooms
            Clients.Caller.PopulateUser(allRooms, myRooms);
            return base.OnConnected();
        }

        public void GetRoomList()
        {
            int moduleId = Convert.ToInt32(Clients.Caller.moduleid);
            var rc = new RoomController();

            var allRooms = rc.GetRooms(moduleId);
            Clients.Caller.FillLobby(allRooms);
        }

        /*
         * When a user connects we need to populate their user information, we default the username to be Anonymous + a #
         */

        //This method is to populate/join room
        public Task JoinRoom(Guid roomId, int moduleId)
        {
            //TODO: don't allow connecting to the same room twice
            var crc = new ConnectionRecordController();
            var crrc = new ConnectionRecordRoomController();
            var rc = new RoomController();

            var r = rc.GetRoom(roomId, moduleId);

            if (r.Enabled)
            {
                if (r.Private)
                {
                    //check the password

                }

                var c = crc.GetConnectionRecordByConnectionId(Context.ConnectionId) ?? SetupConnectionRecord();
                var cr = crrc.GetConnectionRecordRoomByConnectionRecordId(c.ConnectionRecordId, roomId);



                if (cr == null)
                {
                    var crr = new ConnectionRecordRoom
                    {
                        ConnectionRecordId = c.ConnectionRecordId,
                        JoinDate = DateTime.UtcNow,
                        DepartedDate = null,
                        RoomId = roomId
                    };

                    //join the room
                    crrc.CreateConnectionRecordRoom(crr);

                    var ulr = new UserListRecords(c, crr);

                    //add the user to the List of users that will be later filtered by RoomId
                    Users.Add(ulr);
                }

                Groups.Add(Context.ConnectionId, roomId.ToString());

                //populate history for all previous rooms
                RestoreHistory(roomId);

                //lookup the Room to get the Welcome Message
                Clients.Caller.newMessageNoParse(new Message
                {
                    AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                    ConnectionId = "0",
                    MessageDate = DateTime.UtcNow,
                    MessageId = -1,
                    MessageText = r.RoomWelcome,
                    AuthorUserId = -1,
                    RoomId = roomId
                });
                Clients.Group(roomId.ToString()).newMessageNoParse(new Message
                {
                    AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile)
                    ,
                    AuthorUserId = -1
                    ,
                    ConnectionId = "0",
                    MessageDate = DateTime.UtcNow,
                    MessageId = -1,
                    MessageText = string.Format(Localization.GetString("Connected.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), c.UserName),
                    RoomId = roomId
                });

                Clients.Caller.scrollBottom(r.RoomId);

                return Clients.Group(roomId.ToString()).updateUserList(Users.FindAll(uc => (uc.RoomId == r.RoomId)), roomId);
            }
            else
            {
                //if the room was no longer enabled, return nothing
                return null;
            }
        }

        //This method is to populate/join room
        public Task LeaveRoom(Guid roomId, int moduleId)
        {
            var crc = new ConnectionRecordController();
            var crrc = new ConnectionRecordRoomController();
            var rc = new RoomController();

            var c = crc.GetConnectionRecordByConnectionId(Context.ConnectionId) ?? SetupConnectionRecord();

            //lookup client room connection record, if there don't add
            var connectionRoom = crrc.GetConnectionRecordRoomByConnectionRecordId(c.ConnectionRecordId, roomId);

            if (connectionRoom != null)
            {
                connectionRoom.DepartedDate = DateTime.UtcNow;
                crrc.UpdateConnectionRecordRoom(connectionRoom);

                var removeUser = Users.Find(conRec => (conRec.Id == connectionRoom.Id));
                Users.Remove(removeUser);
            }

            //Remove the user from the SignalR Group (broadcast)
            Groups.Remove(Context.ConnectionId, roomId.ToString());

            return Clients.Group(roomId.ToString()).updateUserList(Users.FindAll(cc => (cc.RoomId == roomId)), roomId);

        }


        public Task GetRoomInfo(Guid roomId, int moduleId)
        {
            var rc = new RoomController();
            //Lookup existing Rooms
            var r = rc.GetRoom(roomId, moduleId);
            return Clients.Caller.joinRoom(r);
        }

        public Task GetRoomInfo(Guid roomId, int moduleId, string password)
        {
            var rc = new RoomController();
            //Lookup existing Rooms
            var r = rc.GetRoom(roomId, moduleId);
            if (r.RoomPassword == password)
            { return Clients.Caller.joinRoom(r); }

            var badPasswordResponse = Localization.GetString("BadPassword.Text",
                "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile);
            return Clients.Caller.badPassword(badPasswordResponse);
        }


        /*
         * 	We need to grab the latest 50 chat messages for the channel, should make this configurable.
         */
        public void RestoreHistory(Guid roomId)
        {
            //TODO: make sure the user has access to this room
            try
            {
                int moduleId;
                //int.TryParse(Clients.Caller.moduleid, out moduleId);
                moduleId = Convert.ToInt32(Clients.Caller.moduleid);

                var messages = new MessageController().GetRecentMessages(moduleId, 2, 50, roomId);

                if (messages != null)
                {
                    foreach (var msg in messages)
                    {
                        //TODO: we need to figure out how to make sure it goes to the right room

                        //msg.PhotoUrl = GetPhotoUrl(msg.AuthorUserId);

                        Clients.Caller.newMessageNoParse(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }


        /*
         * This method gets called when someone updates their name. We need to store the change in the ConnectionRecord
         * We also want to send that info back to the client's state to update it there.
         */
        public string UpdateName(string userName)
        {
            var crc = new ConnectionRecordController();
            var id = Context.ConnectionId;
            var cr = crc.GetConnectionRecordByConnectionId(id);
            var crId = cr.ConnectionRecordId;

            //we need to remove the original CR before updating it in the users list

            //TODO: this isn't the right search
            var roomList = Users.FindAll(c => (c.ConnectionRecordId == crId));

            foreach (UserListRecords rr in roomList)
            {
                //todo: it doesn't look like the Remove happens on the client side
                Users.Remove(rr);
                var originalName = rr.UserName;
                //set the new name on both record objects
                rr.UserName = cr.UserName = userName;

                //we need to update with the connectionrecord not UserListRecord
                crc.UpdateConnectionRecord(cr);

                Users.Add(rr);

                var nameChange = String.Format(Localization.GetString("NameChange.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), originalName,
           rr.UserName);
                Clients.Group(rr.RoomId.ToString()).updateUserList(Users.FindAll(uc => (uc.RoomId == rr.RoomId)), rr.RoomId);


                var m = new Message
                {
                    ConnectionId = Context.ConnectionId,
                    MessageDate = DateTime.UtcNow,
                    MessageText = nameChange,
                    AuthorName = originalName,
                    AuthorUserId = -1,
                    ModuleId = rr.ModuleId,
                    RoomId = rr.RoomId
                };
                new MessageController().CreateMessage(m);
                Clients.Group(rr.RoomId.ToString()).newMessage(m);

                //update message for all rooms 
            }

            Clients.Caller.updateName(userName);


            //else return nothing
            return string.Empty;
        }

        //check to see if there is a string in the message that is too many characters put together
        private string ParseMessage(string message, Guid roomId)
        {
            //not using REGEX for now, maybe in the future
            //Regex nameChangeRegex = new Regex(@"/nick[^/]+", RegexOptions.IgnoreCase);

            //var nameChangeMatch = nameChangeRegex.Match(message);
            ////clean out all "scope" parameters from DNN Forum urls
            //if (nameChangeMatch.Success)
            //{
            //    //change the name
            //    string newName = message.Split(':')[1];
            //    message = UpdateName(newName.Trim());
            //}

            //TODO: allow command for Updating room description/properties
            //http://www.irchelp.org/irchelp/changuide.html

            //allow for Room creation/joining
            if (message.ToLower().StartsWith("/join"))
            {
                var userId = -1;
                if (Convert.ToInt32(Clients.Caller.userid) > 0)
                {
                    userId = Convert.ToInt32(Clients.Caller.userid);
                }

                if (userId > 0)
                {
                    string roomName = message.Remove(0, 5).Trim();
                    if (roomName.Length > 25)
                    {
                        Clients.Caller.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = Localization.GetString("RoomNameTooLong.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), AuthorUserId = -1, RoomId = roomId });
                        return string.Empty;
                    }
                    //create room
                    else
                    {
                        var rc = new RoomController();
                        //Lookup existing Rooms
                        var r = rc.GetRoom(roomName);
                        //int.TryParse(Clients.Caller.moduleid, out moduleId);
                        int moduleId = Convert.ToInt32(Clients.Caller.moduleid);

                        if (r != null)
                        {
                            //todo: anything to do here? the room exists already
                            if (r.Enabled == false)
                            {
                                //what if the room has been disabled?
                            }

                        }
                        else
                        {
                            r = new Room
                            {
                                RoomId = Guid.NewGuid(),
                                RoomName = roomName,
                                RoomWelcome = Localization.GetString("DefaultRoomWelcome.Text", "~/desktopmodules/DnnChat/app_localresources/" +
                                                                         Localization.LocalSharedResourceFile),
                                RoomDescription = Localization.GetString("DefaultRoomDescription.Text",
                                                                             "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                                ModuleId = moduleId,
                                CreatedDate = DateTime.UtcNow,
                                CreatedByUserId = userId,
                                LastUpdatedByUserId = userId,
                                LastUpdatedDate = DateTime.UtcNow,
                                Enabled = true

                            };
                            rc.CreateRoom(r);
                        }

                        //make a call to the client to add the room to their model, and join
                        Clients.Caller.messageJoin(r);
                    }
                }
                else
                {
                    // if there is no username for the user don't let them post
                    var m = new Message
                    {
                        ConnectionId = Context.ConnectionId,
                        MessageDate = DateTime.UtcNow,
                        MessageText = Localization.GetString("AnonymousJoinDenied.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                        AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile),
                        AuthorUserId = -1,
                        RoomId = roomId
                    };
                    Clients.Caller.newMessage(m);
                }
                message = string.Empty;
            }

            message = message.Replace("&nbsp;", " ").Replace("&nbsp", " ").Trim();

            //for name change, using starts with to see if they typed /nick in
            if (message.ToLower().StartsWith("/nick"))
            {
                string newName = message.Remove(0, 5);
                if (newName.Length > 25)
                {
                    Clients.Caller.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = Localization.GetString("NameToolong.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile), AuthorUserId = -1, RoomId = roomId });
                    newName = newName.Remove(25);
                }

                if (newName.Trim().Length > 0)
                    message = UpdateName(newName.Trim());
            }

            if (message.ToLower().Trim() == Localization.GetString("Test.Text", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile))
            {
                message = Localization.GetString("Test.Response", "~/desktopmodules/DnnChat/app_localresources/" + Localization.LocalSharedResourceFile);
            }

            return message;
        }

        //get IP address of the client
        //from: http://stackoverflow.com/questions/13889463/get-client-ip-address-in-self-hosted-signalr-hub

        //delete message, need to control 
        public void DeleteMessage(int messageId, int moduleId)
        {
            //todo: look to see if we should get user information from DNN somehow - CJH 3/5/2014
            var section = (MachineKeySection)ConfigurationManager.GetSection("system.web/machineKey");
            var validationKey = section.ValidationKey;

            var listOfRoles = (string)Clients.Caller.userroles;

            if (listOfRoles != null)
            {
                var roles = listOfRoles.Split(',');

                var pc = new PortalSecurity();

                foreach (var r in roles)
                {
                    var thisRole = pc.Decrypt(validationKey, r);
                    //TODO: need to remove the hard coded administrators role here, make this a module setting - CJH 3/6/2014
                    if (thisRole == "Administrators" || thisRole == "SuperUser")
                    {
                        var mc = new MessageController();
                        mc.DeleteMessage(messageId, moduleId);

                        //get the message and send it back so that we can remove it from the proper room
                        var m = mc.GetMessage(messageId, moduleId);
                        Clients.Group(m.RoomId.ToString()).deleteMessage(m);
                    }
                }
            }
        }


        protected string GetIpAddress()
        {
            //var env = Get<IDictionary<string, object>>(Context.Request.Environment, "owin.environment");
            var env = Context.Request.Environment;
            if (env == null)
            {
                return null;
            }
            var ipAddress = Get<string>(env, "server.RemoteIpAddress");
            return ipAddress;
        }

        public static string GetPhotoUrl(int userId)
        {
            if (userId > 0)
            {

                return UserController.Instance.GetUserProfilePictureUrl(userId, 32, 32);

            }
            return "";
        }


        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}