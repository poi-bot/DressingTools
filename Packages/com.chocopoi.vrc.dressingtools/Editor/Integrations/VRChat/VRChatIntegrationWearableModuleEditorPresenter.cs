﻿/*
 * File: VRChatIntegrationModuleEditorPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 23th 2023, 7:56:36 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

#if VRC_SDK_VRCSDK3
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingFramework.UI;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class VRChatIntegrationWearableModuleEditorPresenter
    {
        private IVRChatIntegrationWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private VRChatIntegrationWearableModuleConfig _module;

        public VRChatIntegrationWearableModuleEditorPresenter(IVRChatIntegrationWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, VRChatIntegrationWearableModuleConfig module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.ConfigChange += OnConfigChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.ConfigChange -= OnConfigChange;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnConfigChange()
        {
            _module.customCabinetToggleName = _view.UseCustomCabinetToggleName ? _view.CustomCabinetToggleName : null;
            _module.cabinetThumbnails = _view.UseCabinetThumbnails;
        }

        public bool IsValid()
        {
            return _module.customCabinetToggleName != null ? _module.customCabinetToggleName.Trim() != "" : true;
        }

        private void UpdateView()
        {
            if (_module.customCabinetToggleName != null)
            {
                _view.UseCustomCabinetToggleName = true;
                _view.CustomCabinetToggleName = _module.customCabinetToggleName;
            }
            else
            {
                _view.UseCustomCabinetToggleName = false;
                _view.CustomCabinetToggleName = "";
            }
            _view.UseCabinetThumbnails = _module.cabinetThumbnails;
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
#endif
