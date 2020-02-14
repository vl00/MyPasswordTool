using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPasswordTool.Models
{
    public class TreeNode : NotifyBag { }
    public class SimPaModel : NotifyBag { }

    public class TagTreeStoreData
    {
        public int? Selected { get; set; }
        public JArray Expanded { get; set; }
    }

    public class PaListStoreData
    {
        public JObject Selected { get; set; }
        public FindPasMessage FindPasMessage { get; set; }
        public string RefeshActionInfo { get; set; }
    }

    public struct PaListScrollData
    {
        public double? Hoff { get; set; }
        public double? Voff { get; set; }
    }
}
