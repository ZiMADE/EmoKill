using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiMADE.EmoKill
{
    public enum EntityType
    {
        Unknown = 0,
        SysInfo = 1,
        EmoCheck = 2,
        EmoKill = 3,
        TestKill = 4,
    }

    public enum CheckInfoSource
    {
        Registry = 1,
        Keywords = 2,
        Services = 3,
        Test = 9999,
    }
}
