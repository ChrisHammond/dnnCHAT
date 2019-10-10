/*
' Copyright (c) 2019 Christoc.com Software Solutions
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

namespace Christoc.Modules.DnnChat.Components
{

    [TableName("DnnChat_Rooms")]
    //setup the primary key for table
    [PrimaryKey("RoomId", AutoIncrement = false)]
    //configure caching using PetaPoco
    [Cacheable("RoomId", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleId")]
    public class Room
    {
        ///<summary>
        /// The ID of the Room
        ///</summary>
        public Guid RoomId { get; set; }

        ///<summary>
        /// The name of the room
        ///</summary>
        public string RoomName { get; set; }

        ///<summary>
        /// A boolean field for enable/disable the room
        ///</summary>
        public bool Enabled { get; set; }

        ///<summary>
        /// A string with description of the room
        ///</summary>
        public string RoomDescription { get; set; }

        ///<summary>
        /// A string with description of the room
        ///</summary>
        public string RoomWelcome { get; set; }

        ///<summary>
        /// A boolean field for if the room is private or not
        ///</summary>
        public bool Private { get; set; }

        ///<summary>
        /// A string with Password for the room
        ///</summary>
        public string RoomPassword { get; set; }

        ///<summary>
        /// A boolean field for if the room should be shown in the Room list or not (instant message)
        ///</summary>
        public bool ShowRoom { get; set; }
        
        ///<summary>
        /// The date the connection started
        ///</summary>
        public DateTime CreatedDate { get; set; }

        ///<summary>
        /// The ID of the user who created the record
        ///</summary>
        public int CreatedByUserId { get; set; }

        ///<summary>
        /// The date the connection started
        ///</summary>
        public DateTime LastUpdatedDate { get; set; }


        ///<summary>
        /// The ID of the user who last updated the room
        ///</summary>
        public int LastUpdatedByUserId { get; set; }

        ///<summary>
        /// The ModuleId of where the connect was created
        ///</summary>
        public int ModuleId { get; set; }


    }

}