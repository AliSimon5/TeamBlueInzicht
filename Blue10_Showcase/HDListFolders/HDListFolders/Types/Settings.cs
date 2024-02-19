using M.Core.Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDListFolders
{
    public class Settings : AutoSettings
    {
        public static string Path { get; set; }
        public static string FilterOne { get; set; }
        public static string FilterTwo { get; set; }
        public static string FilterThree { get; set; }
    }
}
