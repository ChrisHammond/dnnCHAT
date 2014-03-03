<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Archive.ascx.cs" Inherits="Christoc.Modules.DnnChat.Archive" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<div class="ChatWindow ChatArchive">
    <asp:Repeater ID="rptMessages" runat="server" OnItemDataBound="rptMessages_OnItemDataBound">
        <ItemTemplate>
            <div class="ChatMessage dnnClear">
                <div class="MessageAuthor"><%# DataBinder.Eval(Container.DataItem,"authorName") %></div>
                <div class="MessageText"><%# ActivateLinksInText(DataBinder.Eval(Container.DataItem,"MessageText").ToString()) %></div>
                <div class="MessageTime"><%# DataBinder.Eval(Container.DataItem,"messageDate") %></div>
            </div>
        </ItemTemplate>
               
    </asp:Repeater>
    <asp:Label runat="server" ID="lblNoResults" resourcekey="lblNoResults"></asp:Label>
</div>

<div class="ArchiveDates">
    <asp:HyperLink runat="server" ID="lbToday" resourcekey="lbToday" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:HyperLink runat="server" ID="lbYesterday" resourcekey="lbYesterday" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:HyperLink runat="server" ID="lbThisWeek" resourcekey="lbThisWeek" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:HyperLink runat="server" ID="lbLastWeek" resourcekey="lbLastWeek" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:HyperLink runat="server" ID="lbThisMonth" resourcekey="lbThisMonth" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:HyperLink runat="server" ID="lbLastMonth" resourcekey="lbLastMonth" CssClass="archiveLink dnnClear"></asp:HyperLink>
    <asp:Label runat="server" ID="lblStartDate" resourcekey="lblStartDate"></asp:Label>
    <asp:TextBox id="txtStartDate" runat="server" class="startDate" />
    <asp:Label runat="server" ID="lblEndDate" resourcekey="lblEndDate"></asp:Label>
    <asp:TextBox id="txtEndDate" runat="server" class="endDate"/>
    <asp:LinkButton runat="server" ID="lbGo" OnClick="lbGo_Click" resourcekey="lbGo"></asp:LinkButton>
</div>

<script>
    $(function () {
        $(".startDate").datepicker();
        $(".endDate").datepicker();
    });

</script>

<div class="projectMessage"><%= Localization.GetString("ProjectMessage.Text",Localization.LocalSharedResourceFile)%></div>
