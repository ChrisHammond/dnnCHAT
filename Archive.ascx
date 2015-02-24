<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Archive.ascx.cs" Inherits="Christoc.Modules.DnnChat.Archive" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<div class="container row">


    <div class="ChatWindow ChatArchive col-md-9 container">
        <h2><asp:Label runat="server" ID="lblArchiveTitle" runat="server"></asp:Label></h2>
        <asp:Repeater ID="rptMessages" runat="server" OnItemDataBound="rptMessages_OnItemDataBound">
            <ItemTemplate>
                <div class="ChatMessage dnnClear row">
                    <div class="MessageAuthor col-md-2">
                        
                        <img src="<%# GetPhotoUrl(DataBinder.Eval(Container.DataItem,"authorUserId")) %>" class="MessageAuthorPhoto" />
                        <div class="MessageAuthorText"><%# DataBinder.Eval(Container.DataItem,"authorName") %></div>
                    </div>
                    <div class="MessageText col-md-9"><%# ActivateLinksInText(DataBinder.Eval(Container.DataItem,"MessageText").ToString()) %></div>
                    <div class="MessageTime col-md-1"><%# DataBinder.Eval(Container.DataItem,"messageDate") %></div>
                </div>
            </ItemTemplate>

        </asp:Repeater>
        <asp:Label runat="server" ID="lblNoResults" resourcekey="lblNoResults"></asp:Label>
    </div>

    <div class="ArchiveDates col-md-3">
        <asp:HyperLink runat="server" ID="lbToday" resourcekey="lbToday" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:HyperLink runat="server" ID="lbYesterday" resourcekey="lbYesterday" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:HyperLink runat="server" ID="lbThisWeek" resourcekey="lbThisWeek" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:HyperLink runat="server" ID="lbLastWeek" resourcekey="lbLastWeek" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:HyperLink runat="server" ID="lbThisMonth" resourcekey="lbThisMonth" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:HyperLink runat="server" ID="lbLastMonth" resourcekey="lbLastMonth" CssClass="archiveLink dnnClear"></asp:HyperLink>
        <asp:Label runat="server" ID="lblStartDate" resourcekey="lblStartDate"></asp:Label>

        <asp:TextBox ID="txtStartDate" runat="server" class="startDate" />
        <div class="lblEndDate">
            <asp:Label runat="server" ID="lblEndDate" resourcekey="lblEndDate" CssClass="lblEndDate"></asp:Label>

            <asp:TextBox ID="txtEndDate" runat="server" class="endDate" />
        </div>
        <asp:LinkButton runat="server" ID="lbGo" OnClick="lbGo_Click" resourcekey="lbGo" CssClass="dnnClear dnnPrimaryAction"></asp:LinkButton>
    </div>
</div>
<script>
    $(function () {
        $(".startDate").datepicker();
        $(".endDate").datepicker();
    });

</script>

<div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",Localization.LocalSharedResourceFile)%></div>
