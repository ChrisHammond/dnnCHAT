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
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Data.PetaPoco;

namespace Christoc.Modules.DnnChat.Components
{

    [TableName("DnnChat_ConnectionRecords")]
    //setup the primary key for table
    [PrimaryKey("ConnectionRecordId", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("ConnectionRecords", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleId")]
    
    public class ConnectionRecord
    {
        ///<summary>
        /// The ID of the connection record
        ///</summary>
        public int ConnectionRecordId { get; set; }
        ///<summary>
        /// A string with the connectionid from SignalR
        ///</summary>
        public string ConnectionId { get; set; }

        ///<summary>
        /// A string with user's entered name
        ///</summary>
        public string UserName { get; set; }

        ///<summary>
        /// An integer with the userid of the user
        ///</summary>
        public int UserId { get; set; }

        ///<summary>
        /// The ModuleId of where the connect was created
        ///</summary>
        public int ModuleId { get; set; }

        ///<summary>
        /// The date the connection started
        ///</summary>
        public DateTime ConnectedDate { get; set; }

        ///<summary>
        /// The date the connected ended
        ///</summary>
        public DateTime? DisConnectedDate { get; set; }

        ///<summary>
        /// A string with user's ipaddress
        ///</summary>
        public string IpAddress { get; set; }

        ///<summary>
        /// A string with user's avatar url
        ///</summary>
        [IgnoreColumn]
        //public string PhotoUrl { get { return string.Format("/profilepic.ashx?userId={0}&h=32&w=32", UserId); } }
        public string PhotoUrl {
            get { return ChatHub.GetPhotoUrl(UserId); }
        }


        public ConnectionRecord()
        { }

        public ConnectionRecord(ConnectionRecord cr)
        {
            ConnectionRecordId = cr.ConnectionRecordId;
            ConnectionId = cr.ConnectionId;
            UserName = cr.UserName;
            UserId = cr.UserId;
            ModuleId = cr.ModuleId;
            ConnectedDate = cr.ConnectedDate;
            DisConnectedDate = cr.DisConnectedDate;
            IpAddress = cr.IpAddress;
        }

    }

}