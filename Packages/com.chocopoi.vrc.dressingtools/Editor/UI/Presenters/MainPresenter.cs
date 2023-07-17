﻿using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter : IMainPresenter
    {
        private IMainView mainView;

        public MainPresenter(IMainView mainView)
        {
            this.mainView = mainView;
        }

        public void StartDressingWizard()
        {
            // TODO: reset dressing tab?
            mainView.SwitchTab(1);
        }

        public void AddToCabinet(DTCabinet cabinet, DTWearableConfig config, GameObject wearableGameObject)
        {
            cabinet.AddWearable(config, wearableGameObject);

            EditorUtility.DisplayProgressBar("DressingTools", "Refreshing cabinet...", 0);
            cabinet.RefreshCabinet();
            EditorUtility.ClearProgressBar();

            // TODO: reset dressing tab?
            // return to cabinet page
            mainView.SwitchTab(0);
        }
    }
}
