/*
' Copyright (c) 2022 Christoc.com Software Solutions
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

//TODO: handle inputs for room fields, add requirements?


using System;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using Christoc.Modules.DnnChat.Components;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace Christoc.Modules.DnnChat
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditDnnChat class is used to manage content
    /// 
    /// Typically your edit control would be used to create new content, or edit existing content within your module.
    /// The ControlKey for this control is "Edit", and is defined in the manifest (.dnn) file.
    /// 
    /// Because the control inherits from DnnChatModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Edit : DnnChatModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    var rc = new RoomController();
                    ddlRooms.DataSource = rc.GetAllRooms(ModuleId);
                    ddlRooms.DataBind();

                    var li = new ListItem();
                    li.Value = "-1";
                    li.Text = Localization.GetString("ChooseRoom.Text", LocalResourceFile);
                    ddlRooms.Items.Insert(0, li);

                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ddlRooms_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            divRoomSettings.Visible = true;
            //TODO: load the room into the edit interface.
            var rc = new RoomController();
            if (ddlRooms.SelectedValue != "-1")
            {
                var r = rc.GetRoom(new Guid(ddlRooms.SelectedValue), ModuleId);
                if (r != null)
                {
                    txtRoomId.Text = r.RoomId.ToString();
                    txtRoomName.Text = r.RoomName;
                    txtRoomDescription.Text = r.RoomDescription;
                    txtRoomPassword.Text = r.RoomPassword;
                    txtRoomWelcome.Text = r.RoomWelcome;
                    chkPrivateRoom.Checked = r.Private;
                    chkEnabled.Checked = r.Enabled;
                    chkShowRoom.Checked = r.ShowRoom;
                }
            }
        }

        protected void lbSubmit_Click(object sender, EventArgs e)
        {
            //save the room
            var rc = new RoomController();
            if (txtRoomId.Text.Any())
            {
                var r = rc.GetRoom(new Guid(txtRoomId.Text), ModuleId);
                r.RoomName = txtRoomName.Text.Trim();
                r.RoomDescription = txtRoomDescription.Text.Trim();
                r.RoomPassword = txtRoomPassword.Text.Trim();
                r.RoomWelcome = txtRoomWelcome.Text.Trim();
                r.Private = chkPrivateRoom.Checked;
                r.Enabled = chkEnabled.Checked;
                r.ShowRoom = chkShowRoom.Checked;
                r.LastUpdatedByUserId = UserId;
                r.LastUpdatedDate = DateTime.UtcNow;

                rc.UpdateRoom(r);

            }
            else
            {
                var r = new Room
                {
                    RoomId = Guid.NewGuid(),
                    RoomName = txtRoomName.Text.Trim(),
                    RoomDescription = txtRoomDescription.Text.Trim(),
                    RoomPassword = txtRoomPassword.Text.Trim(),
                    RoomWelcome = txtRoomWelcome.Text.Trim(),
                    Private = chkPrivateRoom.Checked,
                    Enabled = chkEnabled.Checked,
                    ShowRoom = chkShowRoom.Checked,
                    CreatedByUserId = UserId,
                    CreatedDate =  DateTime.UtcNow,
                    LastUpdatedByUserId = UserId,
                    LastUpdatedDate =  DateTime.UtcNow,
                    ModuleId =  ModuleId
                    
                };
                rc.CreateRoom(r);
            }
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL());
        }

        protected void lbAddRoom_Click(object sender, EventArgs e)
        {
            divRoomSettings.Visible = true;
            txtRoomDescription.Text =
                txtRoomId.Text = txtRoomName.Text = txtRoomPassword.Text = txtRoomWelcome.Text = string.Empty;
            chkEnabled.Checked = chkPrivateRoom.Checked = false;
            chkShowRoom.Checked = true;
        }
    }
}