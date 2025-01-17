﻿/*
 * File: BuildDTCabinetCallback.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 11:42:41 pm
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    [InitializeOnLoad]
    public class BuildDTCabinetCallback : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private const string LogLabel = "BuildDTCabinetCallback";

        public const string GeneratedAssetsFolderName = "_DTGeneratedAssets";
        public const string GeneratedAssetsPath = "Assets/" + GeneratedAssetsFolderName;

        public int callbackOrder => -25;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(avatarGameObject);
            if (cabinet == null)
            {
                // avatar has no cabinet, skipping
                return true;
            }

            var report = new DTReport();

            try
            {
                // create hook instances
                EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.applyingCabinet"), 0);
                new CabinetApplier(report, cabinet).Execute();
            }
            catch (System.Exception ex)
            {
                DTReportUtils.LogExceptionLocalized(report, LogLabel, ex, "integrations.vrc.msgCode.error.exceptionProcessAvatar");
            }
            finally
            {
                // show report window if any errors
                if (report.HasLogType(DTReportLogType.Error))
                {
                    ReportWindow.ShowWindow(report);
                    EditorUtility.DisplayDialog(t._("tool.name"), t._("integrations.vrc.dialog.msg.errorPreprocessingReferReportWindow"), t._("common.dialog.btn.ok"));
                }

                AssetDatabase.SaveAssets();
                // hide the progress bar
                EditorUtility.ClearProgressBar();
            }

            return !report.HasLogType(DTReportLogType.Error);
        }

        public void OnPostprocessAvatar()
        {
        }
    }

#region IEditorOnly Workaround
    // Copyright (c) 2022 bd_ under MIT License
    // temporary workaround with VRCSDK to not remove IEditorOnly objects at early stage which causes problems
    // code referenced from MA: https://github.com/bdunderscore/modular-avatar/blob/main/Packages/nadena.dev.modular-avatar/Editor/PreventStripTagObjects.cs
    // https://feedback.vrchat.com/sdk-bug-reports/p/ieditoronly-components-should-be-destroyed-late-in-the-build-process

    [InitializeOnLoad]
    internal static class RemoveOriginalEditorOnlyCallback
    {
        static RemoveOriginalEditorOnlyCallback()
        {
            EditorApplication.delayCall += () =>
            {
                // obtain the private static field via reflection
                var callbackStaticField = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks", BindingFlags.Static | BindingFlags.NonPublic);
                var callbacks = (List<IVRCSDKPreprocessAvatarCallback>)callbackStaticField.GetValue(null);

                // remove RemoveAvatarEditorOnly
                var modifiedCallbacks = callbacks.Where(c => !(c is RemoveAvatarEditorOnly)).ToList();
                callbackStaticField.SetValue(null, modifiedCallbacks);
            };
        }
    }

    internal class ReplacementRemoveAvatarEditorOnly : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -1024;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // iterate the avatar transforms to see if has tag EditorOnly
            foreach (var transform in avatarGameObject.GetComponentsInChildren<Transform>(true))
            {
                // remove if has tag
                if (transform != null && transform.CompareTag("EditorOnly"))
                {
                    Object.DestroyImmediate(transform.gameObject);
                }
            }
            return true;
        }
    }

    internal class ReplacementRemoveIEditorOnly : IVRCSDKPreprocessAvatarCallback
    {
        // execute the callback at a very very late order
        public int callbackOrder => int.MaxValue;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // iterate all IEditorOnly objects
            foreach (var component in avatarGameObject.GetComponentsInChildren<IEditorOnly>(true))
            {
                Object.DestroyImmediate(component as Object);
            }
            return true;
        }
    }
#endregion IEditorOnly Workaround

}
#endif
