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
using System.Data;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Framework.Providers;

namespace Christoc.Modules.DnnChat.Components
{

    /*
     * This class provides the DAL2 access to the storing of Connections within the DnnChat module
     */

    public class ConnectionRecordController
    {
        private const string ProviderType = "data";
        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly string _objectQualifier;
        private readonly string _databaseOwner;

        public ConnectionRecordController()
        {
            var objProvider = (Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
            {
                _objectQualifier += "_";
            }

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
            {
                _databaseOwner += ".";
            }
        }

        public void CreateConnectionRecord(ConnectionRecord t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecord>();
                rep.Insert(t);
            }
        }

        public void DeleteConnectionRecord(int connectionRecordId, int moduleId)
        {
            var t = GetConnectionRecord(connectionRecordId, moduleId);
            DeleteConnectionRecord(t);
        }

        public void DeleteConnectionRecord(ConnectionRecord t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecord>();
                rep.Delete(t);
            }
        }

        public IEnumerable<ConnectionRecord> GetConnectionRecords(int moduleId)
        {
            IEnumerable<ConnectionRecord> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecord>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public ConnectionRecord GetConnectionRecord(int connectionRecordId, int moduleId)
        {
            ConnectionRecord t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecord>();
                t = rep.GetById(connectionRecordId, moduleId);
            }
            return t;
        }

        public ConnectionRecord GetConnectionRecordByConnectionId(string connectionId)
        {
            ConnectionRecord t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var connections = ctx.ExecuteQuery<ConnectionRecord>(CommandType.Text,
                                                       string.Format(
                                                           "select top 1 * from {0}{1}DnnChat_ConnectionRecords where ConnectionId = '{2}'",
                                                           _databaseOwner,
                                                           _objectQualifier,
                                                          connectionId)).ToList();

                if (connections.Any())
                {
                    t = connections[0];
                }
                else
                    return null;
            }
            return t;
        }

        public void UpdateConnectionRecord(ConnectionRecord t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecord>();
                rep.Update(t);
            }
        }

    }
}