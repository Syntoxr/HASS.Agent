﻿using System;
using System.Windows.Forms;
using HASSAgent.Functions;
using Syncfusion.Windows.Forms;

namespace HASSAgent.Forms
{
    public partial class Onboarding : MetroForm
    {
        private readonly OnboardingManager _onboardingManager;

        public Onboarding()
        {
            _onboardingManager = new OnboardingManager(this);
            InitializeComponent();
        }

        private void Onboarding_Load(object sender, EventArgs e)
        {
            // load the current onboarding control
            _onboardingManager.ShowCurrentOnboardingStatus();
        }

        /// <summary>
        /// Show the next control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNext_Click(object sender, EventArgs e) => _onboardingManager.ShowNext();

        /// <summary>
        /// Show the previous control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrevious_Click(object sender, EventArgs e) => _onboardingManager.ShowPrevious();

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (!_onboardingManager.ConfirmBeforeClose()) return;
            DialogResult = DialogResult.OK;
        }

        private void Onboarding_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_onboardingManager.ConfirmBeforeClose()) e.Cancel = true;
        }
    }
}
