﻿using System;
using System.Windows.Forms;

namespace HASSAgent.Controls.Onboarding
{
    public partial class Updates : UserControl
    {
        public Updates()
        {
            InitializeComponent();
        }

        private void Updates_Load(object sender, EventArgs e)
        {
            CbNofityOnUpdate.Checked = Variables.AppSettings.CheckForUpdates;
        }

        internal bool Store()
        {
            Variables.AppSettings.CheckForUpdates = CbNofityOnUpdate.Checked;
            return true;
        }
    }
}
