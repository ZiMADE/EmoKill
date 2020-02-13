/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System.ServiceProcess;

namespace ZiMADE.EmoKill
{
    partial class WinService : ServiceBase
    {
        public WinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.Start();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
            base.OnStop();
        }
    }
}
