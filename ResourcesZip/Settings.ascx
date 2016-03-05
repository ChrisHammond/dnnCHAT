﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="Christoc.Modules.DnnChat.Settings" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>

<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="lblStartMessage" runat="server" />

        <asp:TextBox ID="txtStartMessage" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblDefaultRoom" runat="server" />

        <asp:DropDownList runat="server" ID="ddlDefaultRoom" DataTextField="RoomName" DataValueField="RoomId"/>
    </div>
    
    <div class="dnnFormItem">
        <dnn:Label ID="lblDefaultAvatarUrl" runat="server" />

        <asp:TextBox ID="txtDefaultAvatarUrl" runat="server" />
    </div>

</fieldset>


