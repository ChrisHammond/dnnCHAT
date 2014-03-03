/*
' Copyright (c) 2014 Christoc.com Software Solutions
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


using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Christoc.Modules.DnnChat.Components;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace Christoc.Modules.DnnChat
{
    using System;
    using DotNetNuke.Services.Exceptions;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Archive class displays old Chat messages
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Archive : DnnChatModuleBase
    {

        override protected void OnInit(EventArgs e)
        {
            jQuery.RequestUIRegistration();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                //TODO: be sure to check for permissions when enabled
                if (!Page.IsPostBack)
                {
                    var sd = Request.QueryString["sd"];
                    var ed = Request.QueryString["ed"];

                    var startDate = DateTime.UtcNow.Date;
                    var endDate = DateTime.UtcNow;
                    if (sd != null)
                    {
                        startDate = Convert.ToDateTime(sd);
                    }
                    if (ed != null)
                    {
                        endDate = Convert.ToDateTime(ed);
                    }

                    txtStartDate.Text = startDate.ToString(CultureInfo.InvariantCulture);
                    txtEndDate.Text = endDate.ToString(CultureInfo.InvariantCulture);
                    var mc = new MessageController();
                    rptMessages.DataSource = mc.GetMessagesByDate(ModuleId, startDate, endDate, RoomId);
                    rptMessages.DataBind();

                    //if we have any items, don't display the "no results found" message
                    if (rptMessages.Items.Count > 0)
                    {
                        lblNoResults.Visible = false;
                    }
                    BuildArchiveLinks(RoomId);

                    var rc = new RoomController();
                    var r = rc.GetRoom(RoomId, ModuleId);
                    var tp = (CDefault)Page;
                    var t = new TabController().GetTab(TabId, PortalId, false);
                    tp.Title += string.Format(Localization.GetString("ChatArchiveTitle.Text", LocalResourceFile), t.TabName, r.RoomName, startDate, endDate);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void BuildArchiveLinks(Guid roomId)
        {
            var curTime = DateTime.UtcNow;
            var today = curTime.Date;
            var month = new DateTime(today.Year, today.Month, 1);

            var ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            DayOfWeek fdow = ci.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek todayDow = curTime.DayOfWeek;
            DateTime sow = curTime.AddDays(-(todayDow - fdow)).Date;

            lbToday.NavigateUrl = BuildArchiveLink(roomId, curTime.Date, curTime);
            lbYesterday.NavigateUrl = BuildArchiveLink(roomId, curTime.Date.AddDays(-1), curTime.Date);
            lbThisWeek.NavigateUrl = BuildArchiveLink(roomId, sow, curTime);
            lbLastWeek.NavigateUrl = BuildArchiveLink(roomId, sow.AddDays(-7), sow);
            lbThisMonth.NavigateUrl = BuildArchiveLink(roomId, month, curTime);
            lbLastMonth.NavigateUrl = BuildArchiveLink(roomId, month.AddMonths(-1), month);
        }

        protected string BuildArchiveLink(Guid roomId, DateTime startDate, DateTime endDate)
        {
            return EditUrl(string.Empty, string.Empty, "Archive", "&roomid=" + roomId + "&sd=" + startDate + "&ed=" + endDate);
        }

        protected void lbGo_Click(object sender, EventArgs e)
        {
            var roomId = Request.QueryString["roomid"];

            if (roomId != null)
            {
                var guidRoomId = new Guid(roomId);
                Response.Redirect(EditUrl(string.Empty, string.Empty, "Archive", "&roomid=" + roomId + "&sd=" + txtStartDate.Text + "&ed=" + txtEndDate.Text));
            }
        }

        public static string ActivateLinksInText(string source)
        {
            source = " " + source + " ";
            // easier to convert BR's to something more neutral for now.
            source = Regex.Replace(source, "<", "&lt;");
            source = Regex.Replace(source, ">", "&gt;");
            source = Regex.Replace(source, "<br>|<br />|<br/>", "\n");
            source = Regex.Replace(source, @"([\s])(www\..*?|http://.*?)([\s])", "$1<a href=\"$2\" target=\"_blank\">$2</a>$3");
            source = Regex.Replace(source, @"href=""www\.", "href=\"http://www.");
            //source = Regex.Replace(source, "\n", "<br />");
            return source.Trim();
        }

        protected void rptMessages_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (rptMessages.Items.Count > 0)
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    var lblNoResults = (Label)e.Item.FindControl("lblNoResults");
                    lblNoResults.Visible = false;
                }
            }
        }
    }
}