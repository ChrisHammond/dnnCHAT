/*
' Copyright (c) 2018 Christoc.com Software Solutions
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
using Christoc.Modules.DnnChat.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace Christoc.Modules.DnnChat
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// 
    /// Typically your settings control would be used to manage settings for your module.
    /// There are two types of settings, ModuleSettings, and TabModuleSettings.
    /// 
    /// ModuleSettings apply to all "copies" of a module on a site, no matter which page the module is on. 
    /// 
    /// TabModuleSettings apply only to the current module on the current page, if you copy that module to
    /// another page the settings are not transferred.
    /// 
    /// If you happen to save both TabModuleSettings and ModuleSettings, TabModuleSettings overrides ModuleSettings.
    /// 
    /// Below we have some examples of how to access these settings but you will need to uncomment to use.
    /// 
    /// Because the control inherits from DnnChatSettingsBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : DnnChatModuleSettingsBase
    {
        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    //Check for existing settings and use those on this page
                    //Settings["SettingName"]
                    
                    txtStartMessage.Text = Settings.Contains("StartMessage") ? Settings["StartMessage"].ToString() : Localization.GetString("DefaultStartMessage", "/DesktopModules/DnnChat/App_LocalResources/SharedResources.resx");

                    txtDefaultAvatarUrl.Text = Settings.Contains("DefaultAvatarUrl") ? Settings["DefaultAvatarUrl"].ToString() : Localization.GetString("DefaultAvatarUrl", "/DesktopModules/DnnChat/App_LocalResources/SharedResources.resx");

                    var rc = new RoomController();

                    ddlDefaultRoom.DataSource = rc.GetRooms(ModuleId);
                    ddlDefaultRoom.DataBind();

                    if (Settings.Contains("DefaultRoomId"))
                    {
                        ddlDefaultRoom.Items.FindByValue(Settings["DefaultRoomId"].ToString()).Selected = true;
                    }

                    
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                var modules = new ModuleController();
                modules.UpdateModuleSetting(ModuleId, "StartMessage", txtStartMessage.Text);

                modules.UpdateModuleSetting(ModuleId, "DefaultRoomId", ddlDefaultRoom.SelectedValue);
                modules.UpdateModuleSetting(ModuleId, "DefaultAvatarUrl", txtDefaultAvatarUrl.Text);
                
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
    }
}