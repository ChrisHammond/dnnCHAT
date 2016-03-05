<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Edit.ascx.cs" Inherits="Christoc.Modules.DnnChat.Edit" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>

<!-- list of Rooms to edit -->
<h2 id="dnnSitePanel-RoomList" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("RoomList")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="lblRoomList" runat="server" />

        <asp:DropDownList runat="server" ID="ddlRooms" DataTextField="RoomName" DataValueField="RoomId" OnSelectedIndexChanged="ddlRooms_OnSelectedIndexChanged" AutoPostBack="True" />
    </div>
    <div class="dnnFormItem">
        <asp:LinkButton id="lbAddRoom" runat="server" resourcekey="lbAddRoom" OnClick="lbAddRoom_Click" CssClass="dnnSecondaryAction" />

    </div>

</fieldset>

<!-- edit interface for rooms -->
<div id="divRoomSettings" runat="server" visible="False">
    <h2 id="dnnSitePanel-RoomSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("RoomSettings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="lblRoomName" runat="server" />

            <asp:TextBox ID="txtRoomName" runat="server" TextMode="SingleLine" />
            <asp:TextBox runat="server" ID="txtRoomId" Visible="false"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblRoomDescription" runat="server" />

            <asp:TextBox runat="server" ID="txtRoomDescription" TextMode="MultiLine" />
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblRoomWelcome" runat="server" />

            <asp:TextBox runat="server" ID="txtRoomWelcome" TextMode="SingleLine" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblPrivateRoom" runat="server" />

            <asp:CheckBox runat="server" ID="chkPrivateRoom" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblRoomPassword" runat="server" />

            <asp:TextBox ID="txtRoomPassword" runat="server" TextMode="SingleLine" />
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblRoomEnabled" runat="server" />

            <asp:CheckBox runat="server" ID="chkEnabled" />
        </div>
    </fieldset>
    <asp:LinkButton runat="server" ID="lbSubmit" runat="server" resourcekey="btnSaveRoom" class="dnnPrimaryAction" OnClick="lbSubmit_Click" />


</div>
