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

namespace Christoc.Modules.DnnChat.Components
{
    using System;

    using DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("DnnChat_Messages")]
    
    // setup the primary key for table
    [PrimaryKey("MessageId", AutoIncrement = true)]
    
    // configure caching using PetaPoco
    // [Cacheable("Messages", CacheItemPriority.Default, 20)]
    
    // scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleId")]
    
    public class Message
    {
        /// <summary>
        /// Gets or sets the message record
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Gets or sets connectionid from SignalR
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the Message content
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the message was posted
        /// </summary>
        public DateTime MessageDate { get; set; }

        /// <summary>
        /// Gets or sets the author of the message
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the AuthorUserId.
        /// </summary>
        public int AuthorUserId { get; set; }

        /// <summary>
        /// Gets or sets the module id.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the RoomId for a message
        /// </summary>
        public Guid RoomId { get; set; }

        /// <summary>
        /// The IsDeleted flag for Messages
        /// </summary>
        public bool IsDeleted { get; set; }

        ///<summary>
        /// A string with user's avatar url
        ///</summary>
        [IgnoreColumn]
        //public string PhotoUrl { get; set; }
        public string PhotoUrl
        {
            get { return ChatHub.GetPhotoUrl(AuthorUserId); }
        }

    }
}