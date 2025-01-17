﻿/*
 * File: IArmatureMappingModuleEditorView.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 12:27:04 am
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

using System;
using System.Collections.Generic;
using Chocopoi.DressingFramework.Dresser;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingTools.UI;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ReportData
    {
        public List<string> errorMsgs;
        public List<string> warnMsgs;
        public List<string> infoMsgs;

        public ReportData()
        {
            errorMsgs = new List<string>();
            warnMsgs = new List<string>();
            infoMsgs = new List<string>();
        }
    }

    internal interface IArmatureMappingWearableModuleEditorView : IEditorView
    {
        event Action DresserChange;
        event Action ModuleSettingsChange;
        event Action DresserSettingsChange;
        event Action RegenerateMappingsButtonClick;
        event Action ViewEditMappingsButtonClick;
        event Action ViewReportButtonClick;

        ReportData DresserReportData { get; set; }
        DresserSettings DresserSettings { get; set; }
        string[] AvailableDresserKeys { get; set; }
        int SelectedDresserIndex { get; set; }
        bool IsAvatarAssociatedWithCabinet { get; set; }
        bool IsLoadCabinetConfigError { get; set; }
        string AvatarArmatureName { get; set; }
        bool RemoveExistingPrefixSuffix { get; set; }
        bool GroupBones { get; set; }

        void StartMappingEditor(DTMappingEditorContainer container);
    }
}
