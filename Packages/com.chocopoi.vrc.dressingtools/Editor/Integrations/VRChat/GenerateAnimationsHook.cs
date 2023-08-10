﻿#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.AvatarLib.Expressions;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class GenerateAnimationsHook : IBuildDTCabinetHook
    {
        private const string LogLabel = "GenerateAnimationHook";

        private DTReport _report;

        private DTCabinet _cabinet;

        public GenerateAnimationsHook(DTReport report, DTCabinet cabinet)
        {
            _report = report;
            _cabinet = cabinet;
        }

        private AnimationGenerationModule FindAnimationGenerationModule(DTWearableConfig config)
        {
            foreach (var module in config.modules)
            {
                if (module is AnimationGenerationModule agm)
                {
                    return agm;
                }
            }
            return null;
        }

        public bool OnPreprocessAvatar()
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Generating animations...", 0);

            // get the avatar descriptor
            var avatarDescriptor = _cabinet.avatarGameObject.GetComponent<VRCAvatarDescriptor>();

            // obtain FX layer
            var fxController = CopyAndReplaceLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            // TODO: these operations are just temporary for testing, need rework to do exception catching etc

            EditorUtility.DisplayProgressBar("DressingTools", "Removing old animator layers and parameters...", 0);
            AnimationUtils.RemoveAnimatorLayers(fxController, "^cpDT_Cabinet");
            AnimationUtils.RemoveAnimatorParameters(fxController, "^cpDT_Cabinet");

            EditorUtility.DisplayProgressBar("DressingTools", "Writing animator...", 0);
            AnimationUtils.AddAnimatorParameter(fxController, "cpDT_Cabinet", 0);
            var refTransition = new AnimatorStateTransition
            {
                canTransitionToSelf = false,
                duration = 0,
                exitTime = 0,
                hasExitTime = false,
                hasFixedDuration = true,
                isExit = false,
                mute = false,
                offset = 0,
                orderedInterruption = true,
                solo = false,
                conditions = new AnimatorCondition[] { }
            };

            var exParams = CopyAndReplaceExpressionParameters(avatarDescriptor);
            var exMenu = CopyAndReplaceExpressionMenu(avatarDescriptor);

            ExpressionMenuUtils.RemoveExpressionParameters(exParams, "^cpDT_Cabinet");

            try
            {
                ExpressionMenuUtils.AddExpressionParameters(exParams, new VRCExpressionParameters.Parameter[]
                {
                new VRCExpressionParameters.Parameter()
                {
                    name = "cpDT_Cabinet",
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = 0,
                    networkSynced = true,
                    saved = true
                }
                });
            }
            catch (ParameterOverflowException ex)
            {
                _report.LogExceptionLocalized(LogLabel, ex, "integrations.vrc.msgCode.error.parameterOverFlow");
                return false;
            }

            ExpressionMenuUtils.RemoveExpressionMenuControls(exMenu, "DT Cabinet");

            var subMenu = new ExpressionMenuBuilder(exMenu)
                .BeginNewSubMenu("DT Cabinet");

            subMenu.AddToggle("Original", "cpDT_Cabinet", 0);

            // create an empty clip
            var emptyClip = new AnimationClip();
            AssetDatabase.CreateAsset(emptyClip, BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_EmptyClip.anim");
            var pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };

            // get wearables
            var wearables = _cabinet.GetWearables();

            for (var i = 0; i < wearables.Length; i++)
            {
                var config = DTWearableConfig.Deserialize(wearables[i].configJson);
                if (config == null)
                {
                    if (!EditorUtility.DisplayDialog("DressingTools", "Unable to load configuration for one of the wearables. It will not be dressed.\nDo you want to continue?", "Yes", "No"))
                    {
                        return false;
                    }
                    continue;
                }

                EditorUtility.DisplayProgressBar("DressingTools", "Generating animations for " + config.info.name + "...", i / (float)wearables.Length * 100);
                var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearables[i].wearableGameObject);

                // find the animation generation module
                var module = FindAnimationGenerationModule(config);
                if (module == null)
                {
                    Debug.Log("[DressingTools] [BuildDTCabinetCallback] [GenerateAnimationHook] " + config.info.name + " has no AnimationGenerationModule, skipping this wearable generation");
                    continue;
                }

                var animationGenerator = new AnimationGenerator(_report, _cabinet.avatarGameObject, module, wearables[i].wearableGameObject, wearableDynamics);

                // TODO: write defaults settings
                var wearAnimations = animationGenerator.GenerateWearAnimations(true);
                pairs.Add(i + 1, wearAnimations.Item1); // enable clip
                AssetDatabase.CreateAsset(wearAnimations.Item1, BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_" + wearables[i].name + ".anim");

                // generate expression menu
                subMenu.AddToggle(config.info.name, "cpDT_Cabinet", i + 1);
            }

            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, true, null, refTransition);

            EditorUtility.DisplayProgressBar("DressingTools", "Generating expression menu...", 0);
            subMenu.CreateAsset(BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_Cabinet.asset")
                .EndNewSubMenu();

            EditorUtility.SetDirty(fxController);
            EditorUtility.SetDirty(exMenu);
            EditorUtility.SetDirty(exParams);

            return true;
        }

        private AnimatorController GetDefaultLayerAnimator(VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            string defaultControllerName = null;
            switch (animLayerType)
            {
                case VRCAvatarDescriptor.AnimLayerType.Base:
                    defaultControllerName = "Locomotion";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Additive:
                    defaultControllerName = "Idle";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Action:
                    defaultControllerName = "Action";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Gesture:
                    defaultControllerName = "Hands";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.FX:
                    defaultControllerName = "Face";
                    break;
            }

            if (defaultControllerName == null)
            {
                return null;
            }

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + "Layer.controller");
            if (controller == null)
            {
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + "Layer.controller");
            }
            return controller;
        }

        private VRCExpressionParameters CopyAndReplaceExpressionParameters(VRCAvatarDescriptor avatarDescriptor)
        {
            var copiedPath = string.Format("{0}/cpDT_ExParams.asset", BuildDTCabinetCallback.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(copiedPath);
            avatarDescriptor.expressionParameters = copiedParams;
            return copiedParams;
        }

        private VRCExpressionsMenu CopyAndReplaceExpressionMenu(VRCAvatarDescriptor avatarDescriptor)
        {
            var copiedPath = string.Format("{0}/cpDT_ExMenu.asset", BuildDTCabinetCallback.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(avatarDescriptor.expressionsMenu), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(copiedPath);
            avatarDescriptor.expressionsMenu = copiedMenu;
            return copiedMenu;
        }


        private AnimatorController CopyAndReplaceLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            VRCAvatarDescriptor.CustomAnimLayer? nullableAnimLayer = null;

            foreach (var layer in avatarDescriptor.baseAnimationLayers)
            {
                if (layer.type == animLayerType)
                {
                    nullableAnimLayer = layer;
                }
            }

            if (nullableAnimLayer == null)
            {
                return null;
            }

            var animLayer = nullableAnimLayer.Value;
            var animator = !animLayer.isDefault && animLayer.animatorController != null ? (AnimatorController)animLayer.animatorController : GetDefaultLayerAnimator(animLayerType);

            // copy to our asset path
            var copiedPath = string.Format("{0}/cpDT_{1}.controller", BuildDTCabinetCallback.GeneratedAssetsPath, animLayerType.ToString());
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(animator), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);

            // get back here
            var copiedAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(copiedPath);
            animLayer.animatorController = copiedAnimator;

            return copiedAnimator;
        }
    }
}
#endif
