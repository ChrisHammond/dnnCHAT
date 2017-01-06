/*
' Copyright (c) 2017 Christoc.com Software Solutions
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
using System.Web;

namespace Christoc.Modules.DnnChat.Components
{
    public class UserListRecords : ConnectionRecord
    {
        ConnectionRecordRoom crr;

        ///<summary>
        /// The ID of the ConnectionRecordRoom
        ///</summary>
        public int Id
        {
            get { return crr.Id; }
            set { crr.Id = value; }
        }

        ///<summary>
        /// The ID of the Room
        ///</summary>
        public Guid RoomId
        {
            get { return crr.RoomId; }
            set { crr.RoomId = value; }
        }

        ///<summary>
        /// The date the user joined the room
        ///</summary>
        public DateTime JoinDate
        {
            get { return crr.JoinDate; }
            set { crr.JoinDate = value; }
        }

        ///<summary>
        /// The date the user departed the room
        ///</summary>
        public DateTime? DepartedDate
        {
            get { return crr.DepartedDate; }
            set { crr.DepartedDate = value; }
        }

        public UserListRecords(ConnectionRecord cr, ConnectionRecordRoom cRr):base(cr)
        {
            crr = cRr;
        }
    }
}