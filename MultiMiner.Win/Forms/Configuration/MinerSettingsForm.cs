﻿using MultiMiner.Utility.Forms;
using MultiMiner.Utility.OS;
using MultiMiner.Utility.Serialization;
using MultiMiner.Win.Data.Configuration;
using MultiMiner.Xgminer.Data;
using System;
using System.Diagnostics;
using MultiMiner.Win.Extensions;

namespace MultiMiner.Win.Forms.Configuration
{
    public partial class MinerSettingsForm : MessageBoxFontForm
    {
        private readonly MultiMiner.Engine.Data.Configuration.Xgminer minerConfiguration;
        private readonly MultiMiner.Engine.Data.Configuration.Xgminer workingMinerConfiguration;

        private readonly Application applicationConfiguration;
        private readonly Application workingApplicationConfiguration;

        public MinerSettingsForm(MultiMiner.Engine.Data.Configuration.Xgminer minerConfiguration, Application applicationConfiguration)
        {
            InitializeComponent();
            this.minerConfiguration = minerConfiguration;
            this.workingMinerConfiguration = ObjectCopier.CloneObject<MultiMiner.Engine.Data.Configuration.Xgminer, MultiMiner.Engine.Data.Configuration.Xgminer>(minerConfiguration);

            this.applicationConfiguration = applicationConfiguration;
            this.workingApplicationConfiguration = ObjectCopier.CloneObject<Application, Application>(applicationConfiguration);
        }

        private void AdvancedSettingsForm_Load(object sender, EventArgs e)
        {
            xgminerConfigurationBindingSource.DataSource = workingMinerConfiguration;
            applicationConfigurationBindingSource.DataSource = workingApplicationConfiguration;
            autoDesktopCheckBox.Enabled = OSVersionPlatform.GetGenericPlatform() != PlatformID.Unix;
            PopulateIntervalCombo();
            PopulateAlgorithmCombo();
            LoadSettings();
        }

        private void PopulateAlgorithmCombo()
        {
            algoArgCombo.Items.Clear();
            foreach (CoinAlgorithm algorithm in (CoinAlgorithm[])Enum.GetValues(typeof(CoinAlgorithm)))
                algoArgCombo.Items.Add(algorithm.ToString().ToSpaceDelimitedWords());
        }

        private void PopulateIntervalCombo()
        {
            intervalCombo.Items.Clear();
            foreach (Application.TimerInterval interval in (Application.TimerInterval[])Enum.GetValues(typeof(Application.TimerInterval)))
                intervalCombo.Items.Add(interval.ToString().ToSpaceDelimitedWords());
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            ObjectCopier.CopyObject(workingMinerConfiguration, minerConfiguration);
            ObjectCopier.CopyObject(workingApplicationConfiguration, applicationConfiguration);
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void LoadSettings()
        {
            intervalCombo.SelectedIndex = (int)workingApplicationConfiguration.ScheduledRestartMiningInterval;

            algoArgCombo.SelectedIndex = 0;
            
            autoDesktopCheckBox.Enabled = !disableGpuCheckbox.Checked;
        }

        private void SaveSettings()
        {
            //if the user has disabled Auto-Set Dynamic Intensity, disable Dynamic Intensity as well
            if (!workingApplicationConfiguration.AutoSetDesktopMode &&
                (workingApplicationConfiguration.AutoSetDesktopMode != applicationConfiguration.AutoSetDesktopMode))
                workingMinerConfiguration.DesktopMode = false;

            workingApplicationConfiguration.ScheduledRestartMiningInterval = (Application.TimerInterval)intervalCombo.SelectedIndex;
        }

        private void disableGpuCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            autoDesktopCheckBox.Enabled = !disableGpuCheckbox.Checked;
        }

        private void scryptConfigLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://litecoin.info/Mining_hardware_comparison#GPU");
        }

        private void argAlgoCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CoinAlgorithm algorithm = (CoinAlgorithm)algoArgCombo.SelectedIndex;
            if (workingMinerConfiguration.AlgorithmFlags.ContainsKey(algorithm))
                algoArgEdit.Text = workingMinerConfiguration.AlgorithmFlags[algorithm];
            else
                algoArgEdit.Text = String.Empty;
        }

        private void algoArgEdit_Validated(object sender, EventArgs e)
        {
            CoinAlgorithm algorithm = (CoinAlgorithm)algoArgCombo.SelectedIndex;
            workingMinerConfiguration.AlgorithmFlags[algorithm] = algoArgEdit.Text;
        }
    }
}
