﻿using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using Chocopoi.DressingTools.Logging;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default.Hooks
{
    public class NoMissingScriptsHookTest : DTTestBase
    {
        private bool EvaluateHook(GameObject avatarRoot, GameObject wearableRoot, out DTReport report)
        {
            report = new DTReport();
            var settings = new DTDefaultDresserSettings();
            var boneMappings = new List<DTBoneMapping>();
            var objectMappings = new List<DTObjectMapping>();
            var hook = new NoMissingScriptsHook();

            settings.targetAvatar = avatarRoot;
            settings.targetWearable = wearableRoot;
            settings.avatarArmatureName = "Armature";
            settings.wearableArmatureName = "Armature";
            settings.dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;

            return hook.Evaluate(report, settings, boneMappings, objectMappings);
        }

        [Test]
        public void AvatarMissingScripts_ReturnsCorrectErrorCode()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_MissingScriptsObject.prefab");

            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar));
        }

        [Test]
        public void WearableMissingScripts_ReturnsCorrectErrorCode()
        {
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_MissingScriptsObject.prefab");

            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable));
        }
    }
}
