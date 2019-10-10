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

namespace Christoc.Modules.DnnChat.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using DotNetNuke.Data;
    using DotNetNuke.Framework.Providers;

    /*
     * This class provides the DAL2 access to the storing of Messages within the DnnChat module
     */
    public class MessageController
    {

        private const string ProviderType = "data";
        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly string _objectQualifier;
        private readonly string _databaseOwner;

        public void CreateMessage(Message t)
        {
            using (var ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Message>();
                rep.Insert(t);
            }
        }

        public void DeleteMessage(int messageId, int moduleId)
        {
            var t = this.GetMessage(messageId, moduleId);
            //this.DeleteMessage(t); //we aren't hard deleting messages anymore, only soft deletes
            t.IsDeleted = true;
            UpdateMessage(t);
        }

        public void DeleteMessage(Message t)
        {
            //not currently being used, but still available
            using (var ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Message>();

                rep.Delete(t);
            }
        }

        public IEnumerable<Message> GetMessages(int moduleId, Guid roomId)
        {
            IEnumerable<Message> t;
            using (var ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Message>();
                //filter by the Room id
                t = (from a in rep.Get(moduleId) where a.RoomId == roomId select a).OrderByDescending(x => x.MessageDate);

                //t = rep.Get(moduleId).OrderByDescending(x => x.MessageDate);
            }

            return t;
        }

        public IEnumerable<Message> GetRecentMessages(int moduleId, int hoursBackInTime, int maxRecords, Guid roomId)
        {
            //var messages = (from a in GetMessages(moduleId, roomId) where a.MessageDate.Subtract(DateTime.UtcNow).TotalHours <= hoursBackInTime && a.IsDeleted==false select a).Take(maxRecords).Reverse();
            using (var ctx = DataContext.Instance())
            {
                //var rep = ctx.GetRepository<Message>();
                //var messages = (from a in rep.Get(moduleId) where a.RoomId == roomId 
                //                && a.MessageDate.Subtract(DateTime.UtcNow).TotalHours <= hoursBackInTime 
                //                && a.IsDeleted == false
                //                select a).OrderByDescending(x => x.MessageDate).Take(maxRecords).Reverse();

                
                var messages = ctx.ExecuteQuery<Message>(CommandType.Text,
                                                          string.Format(
                                                              //"select top {4} * from {0}{1}DnnChat_Messages where ModuleId = '{2}' and DateDiff(hour,MessageDate,GetUtcDate())<={3} and RoomId = '{5}' and IsDeleted =false",
                                                              "select top {4} * from {0}{1}DnnChat_Messages where ModuleId = '{2}' and RoomId = '{5}' and IsDeleted ='false' order by MessageId Desc",
                                                              _databaseOwner,
                                                              _objectQualifier,
                                                             moduleId, hoursBackInTime, maxRecords, roomId)).Reverse().ToList();
                
                return messages.Any() ? messages : null;
            }
        }

        public IEnumerable<Message> GetMessagesByDate(int moduleId, DateTime startDate, DateTime endDate, Guid roomId)
        {
            //var messages = (from a in GetMessages(moduleId, roomId) where a.MessageDate <= endDate && a.MessageDate >= startDate && a.IsDeleted == false select a).Reverse();

            using (var ctx = DataContext.Instance())
            {
                //var rep = ctx.GetRepository<Message>();
                //var messages = (from a in rep.Get(moduleId)
                //                where a.RoomId == roomId
                //                && a.MessageDate <= endDate
                //                && a.MessageDate >= startDate
                //                && a.IsDeleted == false
                //                select a).OrderByDescending(x => x.MessageDate).Reverse();




                var messages = ctx.ExecuteQuery<Message>(CommandType.Text,
                                          string.Format(
                                              "select * from {0}{1}DnnChat_Messages where ModuleId = '{2}' and MessageDate between '{3}' and '{4}' and RoomId = '{5}' and IsDeleted ='false'",
                                              _databaseOwner,
                                              _objectQualifier,
                                             moduleId, startDate, endDate, roomId)).ToList();

                return messages.Any() ? messages : null;
            }

        }

        public Message GetMessage(int messageId, int moduleId)
        {
            Message t;
            using (var ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Message>();
                t = rep.GetById(messageId, moduleId);
            }

            return t;
        }

        public void UpdateMessage(Message t)
        {
            using (var ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Message>();
                rep.Update(t);
            }
        }
    }
}