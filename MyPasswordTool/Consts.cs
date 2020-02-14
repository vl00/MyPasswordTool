using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyPasswordTool
{
    public static partial class Consts
    {
        public static readonly string str_newdb = @"
PRAGMA auto_vacuum = 0;
PRAGMA temp_store = 1;
------------------------------PaInfoTag----------------------------------------------------------
CREATE TABLE 'PaInfoTag' (
'TagID'  INTEGER NOT NULL,
'PaInfoID'  INTEGER NOT NULL
);
------------------------------PaInfo----------------------------------------------------------
CREATE TABLE 'PaInfo' (
'ID'  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
'Title'  TEXT(200) NOT NULL,
'Type'  INTEGER NOT NULL,
'ICO'  BLOB,
'TitleDesc'  TEXT,
'Data'  BLOB,
'CreateTime'  TEXT NOT NULL,
'UpdateTime'  TEXT NOT NULL,
'IsDeleted'  INTEGER NOT NULL DEFAULT 0
);
------------------------------PaInfoFile----------------------------------------------------------
CREATE TABLE 'PaInfoFile' (
'ID'  INTEGER NOT NULL,
'File'  BLOB COLLATE BINARY,
'FileExtname'  TEXT,
'FileSize'  REAL,
CONSTRAINT 'pk' UNIQUE ('ID')
);
------------------------------PaTag-------------------------------------------------------
CREATE TABLE 'PaTag' (
'ID'  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
'Name'  TEXT(50) NOT NULL,
'PID'  INTEGER NOT NULL DEFAULT -1,
'Order'  INTEGER,
'HasChild'  INTEGER NOT NULL DEFAULT 0
);
------------------trigger_SyncIDAndOrder_AfterInsert-----------------------------------
CREATE TRIGGER 'trigger_SyncIDAndOrder_AfterInsert' AFTER INSERT ON 'PaTag' BEGIN
  UPDATE PaTag SET 'Order'=ID WHERE ID=new.ID;
END
";

        public const string MyPart = "__my_Part";
        public const string app_exit = "app_exit";
        public const string db_vacuum = "db_vacuum";
        public const string clean_tree = "del_not_in_tree";
        public const string BackToken_deltag = "deltag";

        public static readonly int ExitWaitMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["exitWaitMilliseconds"]);
        public static readonly bool? TestCanLoadLazy = ConfigurationManager.AppSettings["testCanLoadLazy"].CastTo<bool?>();

        #region Consts.DrapDropGroup
        public static readonly DependencyProperty DrapDropGroupProperty = DependencyProperty.RegisterAttached("DrapDropGroup", typeof(string), typeof(Consts), null);

        public static string GetDrapDropGroup(UIElement target)
        {
            return (string)target.GetValue(DrapDropGroupProperty);
        }

        public static void SetDrapDropGroup(UIElement target, string value)
        {
            target.SetValue(DrapDropGroupProperty, value);
        }
        #endregion

        public delegate Type MapKeyToViewType(string key);
        public delegate string MapViewTypeToKey(Type type);

        public static class NS
        {
            public const string ChildWin = "_$ns:ChildWin";
            public const string Shell = "_$ns:shell";
            public const string Pa = "_$ns:pa";
            public const string EmptyDuc = "_$ns:EmptyDuc";
            public const string BeforeDuc = "_$ns:BeforeDuc";
            public const string DucTrash = "_$ns:DucTrash";
            public const string Duc = "_$ns:Duc";
            public const string Edit = "_$ns:Edit";
            public const string Edit_BackTo_Duc = "_$ns:Edit_BackTo_Duc";
        }

        public static class ViewKey
        {
            public static readonly string Main = $"_$view:{typeof(MainPage).AssemblyQualifiedName}";
            public static readonly string Palock = $"_$view:{typeof(Views.PalockView).AssemblyQualifiedName}";
            public static readonly string Rename = $"_$view:{typeof(Views.RenameWin).AssemblyQualifiedName}";
            public static readonly string DucTrash = $"_$view:{typeof(Views.TrashDisplayPaUC).AssemblyQualifiedName}";
            public const string Duc = "_$view:MyPasswordTool.Views.PageDuc{0}";
            public const string Edit = "_$view:MyPasswordTool.Views.PageEdit{0}";
            public const string patags_win = "_$view:MyPasswordTool.Views.PaTagsWin";
        }

        public const string NonTag = "未分类";
        public const string NewTagName = "..new painfo";
        public const string ico_trash = "/Themes/Resource/trash.png";
        public const string ico_all = "/Themes/Resource/alltag.png";
        public const string ico_notag = "/Themes/Resource/notag.png";
        public const string ico_tag = "/Themes/Resource/tag.png";
        public const string IcoFilter = "*(*.jpg;*.jpeg;*.png;*.bmp;*.ico)|*.jpg;*.jpeg;*.png;*.bmp;*.ico|jpg(*.jpg;*.jpeg)|*.jpg;*.jpeg|png(*.png)|*.png|bmp(*.bmp)|*.bmp|ico(*.ico)|*.ico";
        public const string ducpa_fromlist = "__ducpa_fromlist";
        public const string ducpa_afteredit = "__ducpa_afteredit";
        public const string ducpa_displaytoedit = "__ducpa_displaytoedit";

        public static class Error
        {
            public const string FindNoShell = "find no shell";
            public const string NotFindView = "not find view:{0}";
        }

        public static class DyActType
        {
            public const string palsOrderChanged = "palsOrderChanged"; //(string order)
            public const string before_unlock_ok = "before_unlock_ok"; //(bool is_first_unlock, string new_conn, string old_conn)
            public const string main_Activate = "main_Activate";
            public const string main_Deactivate = "main_Deactivate";
            public const string main_Clear = "main_Clear";
        }

        //public static class dev
        //{
        //    public static readonly Models.BackgroundMessage BackgroundMsg = new Models.BackgroundMessage()
        //    {
        //        Token = "test2",
        //        Message = "guhwuoghwohow"
        //    };
        //}
    }
}