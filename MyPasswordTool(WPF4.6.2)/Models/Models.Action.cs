using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPasswordTool.Models
{
    public class LockActionMessage
    {
        public bool IsLock { get; set; }
        public DbConnInfo DbInfo { get; set; }
    }

    public class BackgroundMessage
    {
        public dynamic Message { get; set; }
        public string Token { get; set; }
        //public string DB { get; set; }
        //public string DBpwd { get; set; }
        public bool IsDirect { get; set; }
    }

    public class FindPasMessage
    {
        public string Token { get; set; }
        public NotifyBag Data { get; set; }
    }

    public class RefreshPaInfosMessage
    {
        public string Type { get; set; } // null | "updated" | "added"
        public object Payload { get; set; }
    }

    public class NavigationMessage
    {
        public string Type { get; set; }
        public string ViewKey { get; set; }
        public object Parameter { get; set; }
    }

    public class DropPaInfoToTagMessage
    {
        public dynamic Tag { get; set; }
        public dynamic SimplePaInfo { get; set; }
    }

    public struct ChangeSearchText
    {
        public string Text { get; set; }
    }
}
