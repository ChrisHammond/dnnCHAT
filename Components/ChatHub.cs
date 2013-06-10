/*
' Copyright (c) 2013  Christoc.com Software Solutions
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
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Services.Localization;
using Microsoft.AspNet.SignalR;

namespace Christoc.Modules.DnnChat.Components
{
    public class ChatHub : Hub
    {
        //cjh - 2/28/2013 revamping to use knockout with guidance of http://www.slideshare.net/aegirth1/knockout-presentation-slides 

        //TODO: modify Hub to support rooms

        //a list of connectionrecords to keep track of users connected
        private static readonly List<ConnectionRecord> Users = new List<ConnectionRecord>();

        private static readonly Guid DefaultRoomId = new Guid("78fbeba0-cc57-4cd4-9dde-8611c91f7b9c"); //TODO: set the default room based on? ModuleId setting?

        /*
         * This method is used to send messages to all connected clients.
         */

        //for clients that may call the old method, send to the default room
        public void Send(string message)
        {
            //TODO: figure out the default room (module setting?)

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
                //TODO: make sure that the user can send to that Room

                // parse message before use
                if (Clients.Caller.username != null && Clients.Caller.username.Trim() != "phantom")
                {
                    var parsedMessage = ParseMessage(message);
                    if (parsedMessage != string.Empty)
                    {

                        int moduleId;
                        //int.TryParse(Clients.Caller.moduleid, out moduleId);
                        moduleId = Convert.ToInt32(Clients.Caller.moduleid);

                        var outputMessage = ParseMessage(message.Trim());
                        var m = new Message
                                    {
                                        ConnectionId = Context.ConnectionId,
                                        MessageDate = DateTime.UtcNow,
                                        MessageText = outputMessage,
                                        AuthorName = Clients.Caller.username,
                                        ModuleId = moduleId,
                                        RoomId = roomId
                                    };

                        new MessageController().CreateMessage(m);
                        Clients.Group(roomId.ToString()).newMessage(m);
                    }
                }
                else
                {
                    //TODO: handle anon users for non-default rooms

                    // if there is no username for the user don't let them post
                    var m = new Message
                                {
                                    ConnectionId = Context.ConnectionId,
                                    MessageDate = DateTime.UtcNow,
                                    MessageText = Localization.GetString("FailedUnknown.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile),
                                    AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile),
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
                    MessageText = Localization.GetString("FailedUnknown.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile),
                    AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile),
                    RoomId = DefaultRoomId
                };
                Clients.Caller.newMessage(m);
            }
        }

        //TODO: on connection, reload rooms for user?
        public override Task OnConnected()
        {
            Clients.Caller.Join();
            return base.OnConnected();
        }

        //TODO: on reconnection reload rooms for user
        public override Task OnReconnected()
        {
            Clients.Caller.PopulateUser();
            return base.OnReconnected();
        }

        //TODO: remove user from all rooms

        //lookup who just disconnected, and store the disconnect/time, remove them from the count for each room
        public override Task OnDisconnected()
        {
            if (Context.ConnectionId != null) DisconnectUser(Context.ConnectionId);
            return base.OnDisconnected();
        }

        //TODO: remove user from all rooms
        private void DisconnectUser(string connectionId)
        {
            var id = connectionId;
            if (id == null)
                return;
            var removeCrr = Users.Find(c => (c.ConnectionId == id));
            if (removeCrr != null)
            {
                Users.Remove(removeCrr);
            }

            var crc = new ConnectionRecordController();
            var cr = crc.GetConnectionRecordByConnectionId(id);
            if (cr != null)
            {
                Clients.Others.removeUserFromList(cr);
                cr.DisConnectedDate = DateTime.UtcNow;
                crc.UpdateConnectionRecord(cr);

                Clients.All.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = string.Format(Localization.GetString("Disconnected.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), cr.UserName) });
                Clients.All.updateUserList(Users);
            }
        }


        private ConnectionRecord SetupConnectionRecord()
        {
            string username = Clients.Caller.username;

            //if (string.IsNullOrEmpty(username))
            //{
            //    Clients.Caller.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = string.Format(Localization.GetString("BadConnection.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), "phantom") });
            //    return new ConnectionRecord();
            //}

            if (username.Trim() == "phantom" || username.Trim() == string.Empty)
            {
                username = string.Format(Localization.GetString("AnonymousUser.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), (Users.Count + 1));
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
                Users.Add(c);
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

                Users.Add(c);
                crc.CreateConnectionRecord(c);
            }
            //store the record for the connection
            return c;
        }

        //TODO: on connection, reload rooms for user?
        public Task Join()
        {
            //connect user to the default room
            Groups.Add(Context.ConnectionId, DefaultRoomId.ToString());

            //TODO: reconnect to all previous rooms
            //get list of previously connected (not departed) rooms
            var crrc = new ConnectionRecordRoomController();
            var rc = new RoomController();
            int moduleId = Convert.ToInt32(Clients.Caller.moduleid);

            var myRooms = crrc.GetConnectionRecordRoomsByUserId((int)Clients.Caller.userid);

            //if myRooms is empty, what to do (pass default room)
            if (myRooms == null)
            {
                //load the default room
                var r = rc.GetRoom(DefaultRoomId, moduleId);
                myRooms = new List<Room>();
                myRooms = myRooms.Concat(new[] { r });
            }

            var allRooms = rc.GetRooms(moduleId);

            //we are passing in a list of All rooms, and the current user's rooms
            Clients.Caller.PopulateUser(allRooms, myRooms);
            return base.OnConnected();
        }


        /*
         * When a user connects we need to populate their user information, we default the username to be Anonymous + a #
         */

        //This method is to populate/join room
        public Task JoinRoom(Guid roomId, int moduleId)
        {

            var crc = new ConnectionRecordController();
            var crrc = new ConnectionRecordRoomController();
            var rc = new RoomController();

            var r = rc.GetRoom(roomId, moduleId);

            var c = crc.GetConnectionRecordByConnectionId(Context.ConnectionId) ?? SetupConnectionRecord();

            //if the startMessage is empty, that means the user is a reconnection
            if (Clients.Caller.startMessage != string.Empty)
            {
                //lookup client room connection record, if there don't add

                var cr = crrc.GetConnectionRecordRoom(c.ConnectionRecordId, roomId);

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
                }

                //TODO: populate history for all previous rooms
                RestoreHistory(roomId);

                //TODO: target a room here
                Clients.Caller.newMessageNoParse(new Message
                {
                    AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile),
                    ConnectionId = "0",
                    MessageDate = DateTime.UtcNow,
                    MessageId = -1,
                    MessageText = Clients.Caller.startMessage,
                    RoomId = roomId
                });
                Clients.All.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = string.Format(Localization.GetString("Connected.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), c.UserName) });
            }

            return Clients.Group(roomId.ToString()).updateUserList(Users);
        }


        /*
         * 	We need to grab the latest 50 chat messages for the channel, should make this configurable.
         */
        //TODO: pull in history for specific room
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
                        Clients.Caller.newMessageNoParse(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        //TODO: update name in all rooms

        /*
         * This method gets called when someone updates their name. We need to store the change in the ConnectionRecord
         * We also want to send that info back to the client's state to update it there.
         */
        public string UpdateName(string userName)
        {
            var id = Context.ConnectionId;
            //we need to remove the original CR before updating it in the users list
            var removeCrr = Users.Find(c => (c.ConnectionId == id));
            if (removeCrr != null)
            {
                //handle removing the users
                Users.Remove(removeCrr);
            }

            var crc = new ConnectionRecordController();
            var cr = crc.GetConnectionRecordByConnectionId(id);
            if (cr != null)
            {
                var originalName = cr.UserName;
                //set the new name and save
                cr.UserName = userName;
                crc.UpdateConnectionRecord(cr);
                Users.Add(cr);
                Clients.Caller.UpdateName(userName);
                var nameChange = String.Format(Localization.GetString("NameChange.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), originalName,
                           cr.UserName);

                Clients.All.updateUserList(Users);
                return nameChange;
            }
            //else return nothing
            return string.Empty;
        }

        //TODO: Create method for leaving one room

        //check to see if there is a string in the message that is too many characters put together
        private string ParseMessage(string message)
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

            message = message.Replace("&nbsp;", " ").Replace("&nbsp", " ").Trim();

            //for name change, using starts with to see if they typed /nick in
            if (message.ToLower().StartsWith("/nick"))
            {
                string newName = message.Remove(0, 5);
                if (newName.Length > 25)
                {
                    Clients.Caller.newMessageNoParse(new Message { AuthorName = Localization.GetString("SystemName.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile), ConnectionId = "0", MessageDate = DateTime.UtcNow, MessageId = -1, MessageText = Localization.GetString("NameToolong.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile) });
                    newName = newName.Remove(25);
                }


                if (newName.Trim().Length > 0)
                    message = UpdateName(newName.Trim());
            }

            if (message.ToLower().Trim() == Localization.GetString("Test.Text", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile))
            {
                message = Localization.GetString("Test.Response", "/desktopmodules/DnnChat/app_localresources/ " + Localization.LocalSharedResourceFile);
            }

            return message;
        }

        //get IP address of the client
        //from: http://stackoverflow.com/questions/13889463/get-client-ip-address-in-self-hosted-signalr-hub

        protected string GetIpAddress()
        {
            var env = Get<IDictionary<string, object>>(Context.Request.Items, "owin.environment");
            if (env == null)
            {
                return null;
            }
            var ipAddress = Get<string>(env, "server.RemoteIpAddress");
            return ipAddress;
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}