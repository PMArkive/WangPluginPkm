﻿using System;
using System.Windows.Forms;
using WangPluginPkm.GUI;

namespace WangPluginPkm.Plugins
{
    internal class AboutPlugin: WangPluginPkm
    {
        public override string Name => "关于/About";
        public override int Priority => 12;

        protected override void AddPluginControl(ToolStripDropDownItem modmenu)
        {
            var ctrl = new ToolStripMenuItem(Name)
            {
                Image = Properties.Resources.avatar
            };
            ctrl.Click += OpenForm;
            ctrl.Name = "关于/About";
            modmenu.DropDownItems.Add(ctrl);

        }

        private void OpenForm(object sender, EventArgs e)
        {

            var form = new AboutUI();
            form.Show();
        }
    }
}
