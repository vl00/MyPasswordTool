using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Common.ObjectHelper;
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
    public partial class TagTreeViewModel : NotifyObject
    {
        public TagTreeViewModel()
        {
            this.TryInit();
            ThreadUtil.CallOnUIThread(OnInit);
        }

        private const string _notstorekeyprev = "__$";
        private int _cannot_up_to_appstate = 0;
        private readonly TagTreeStoreData SData = new TagTreeStoreData();
        private readonly DalRepository repository = IoC.Resolve<DalRepository>();

        public NotifyBag ViewModelData { get; } = new NotifyBag();
        public ObservableCollection<TreeNode> TreeNodes { get; } = new ObservableCollection<TreeNode>();

        void OnInit()
        {
            {
                var cm = @state.JX?["TagTree"];
                SData.Expanded = (cm?["Expanded"] as JArray) ?? new JArray { -1 };
                SData.Selected = ((int?)cm?["Selected"]) ?? -1;
            } 
            {
                dynamic node = new TreeNode();
                node.Name = "回收站";
                node.IsExpanded = false;
                node.TagICO = Consts.ico_trash;
                node.PNode = null;
                node.Nodes = new ObservableCollection<TreeNode>();
                node._inited = true;
                TreeNodes.Add(set_treeitem_Expanded_and_Selected(node));
            } 
            {
                dynamic node = new TreeNode();
                node.ID = -1;
                node.Name = "All";
                node.PNode = null;
                node.Nodes = new ObservableCollection<TreeNode>();
                node.IsExpanded = true;
                node.TagICO = Consts.ico_all;
                node.HasChild = true;
                node._inited = true;
                TreeNodes.Add(set_treeitem_Expanded_and_Selected(node));
                _get_tree(node);
            } 
        }

        //public void Activate(object parameter) { }
        //public void Deactivate(object parameter) { }

        private async Task _get_tree(dynamic node)
        {
            var pid = (int)node.ID;
            var tqs = await repository.GetChildrenPaTags(pid);
            if (!node.Has("Nodes")) node.Nodes = new ObservableCollection<TreeNode>();
            if (pid == -1)
            {
                dynamic dy = new TreeNode();
                dy.ID = 0;
                dy.Name = Consts.NonTag;
                dy.PID = -1;
                dy.Order = 0;
                dy.PNode = node;
                dy.Nodes = new ObservableCollection<TreeNode>();
                dy.TagICO = Consts.ico_notag;
                dy.HasChild = false;
                dy._inited = true;
                node.Nodes.Add(set_treeitem_Expanded_and_Selected(dy));
            }
            foreach (var tag in tqs)
            {
                dynamic q = tag.ModelToBag<TreeNode>();
                q.PNode = node;
                q.Nodes = new ObservableCollection<TreeNode>();
                q.TagICO = Consts.ico_tag;
                node.Nodes.Add(set_treeitem_Expanded_and_Selected(q));
                q._inited = true;
                if (q.IsExpanded == true || q.IsSelected == true)
                {
                    q.IsExpanded = true;
                    _ = _get_tree(q);
                }
            }
        }

        private ICommand _SelCmd;
        public ICommand SelCmd
        {
            get
            {
                return _SelCmd = _SelCmd ?? new DelegateCommand<dynamic>(async node =>
                {
                    using (lazy_up_to_appstate())
                        node.IsExpanded = node.IsSelected = true;

                    @dispatch(new FindPasMessage
                    {
                        Token = "TreeTag",
                        Data = new NotifyBag()
                        {
                            ["ID"] = node.ID,
                            //["TreeNode"] = node,
                        }
                    });

                    if (node.HasChild == true && node.Nodes.Count <= 0)
                    {
                        using (lazy_up_to_appstate())
                        {
                            node.IsExpanded = false;
                            await _get_tree(node);
                            node.IsExpanded = true;
                        }
                    }
                }, node => node.IsSelected != true);
            }
        }

        private ICommand _ToggleCmd;
        public ICommand ToggleCmd
        {
            get
            {
                return _ToggleCmd = _ToggleCmd ?? new DelegateCommand<dynamic>(async node => 
                {
                    using (lazy_up_to_appstate())
                    {
                        node.IsExpanded = true;
                        if (node.HasChild == true && node.Nodes.Count <= 0)
                        {
                            node.IsExpanded = false;
                            await _get_tree(node);
                            node.IsExpanded = true;
                        }
                    }
                }, node => node.IsExpanded == true);
            }
        }

        private ICommand _TrashCmd;
        public ICommand TrashCmd
        {
            get
            {
                return _TrashCmd = _TrashCmd ?? new DelegateCommand<dynamic>(async node => 
                {
                    if (await repository.TrashDeletedPaInfo() && node.IsSelected)
                    {
                        //can_up_to_appstate = false;
                        node.IsSelected = false;
                        SelCmd.TryInvoke(node as object);
                    }
                });
            }
        }

        private ICommand _RenameCmd;
        public ICommand RenameCmd
        {
            get
            {
                return _RenameCmd = _RenameCmd ?? new DelegateCommand<dynamic>(node => 
                {
                    @dispatch(new NavigationMessage()
                    {
                        Type = Consts.NS.ChildWin,
                        ViewKey = Consts.ViewKey.Rename,
                        Parameter = node,
                    });
                });
            }
        }

        private ICommand _AddTagCmd;
        public ICommand AddTagCmd
        {
            get
            {
                return _AddTagCmd = _AddTagCmd ?? new DelegateCommand<dynamic>(async node =>
                {
                    using (lazy_up_to_appstate())
                    {
                        node.IsExpanded = true;
                        if (node.HasChild == true && node.Nodes.Count <= 0)
                        {
                            await _get_tree(node);
                        }

                        var tag = new PaTag { PID = node.ID, Name = "新标签" };
                        await repository.AddPaTagAsync(tag);

                        dynamic n = tag.ModelToBag<TreeNode>();
                        n.TagICO = Consts.ico_tag;
                        n.PNode = node;
                        n.Nodes = new ObservableCollection<TreeNode>();
                        set_treeitem_Expanded_and_Selected(n);
                        n._inited = true;
                        node.IsExpanded = true;
                        node.Nodes.Add(n);
                        node.HasChild = true;
                    }
                });
            }
        }

        private ICommand _DelTagCmd;
        public ICommand DelTagCmd
        {
            get
            {
                return _DelTagCmd = _DelTagCmd ?? new DelegateCommand<dynamic>(async node =>
                {
                    var phaschild = ((int)node.PNode.Nodes.Count) > 1;
                    await repository.DeletePaTagAsync(node.ID, node.PID);
                    SelCmd.TryInvoke(node.PNode as object);
                    node.PNode.HasChild = phaschild;
                    node.PNode.Nodes.Remove(node);
                    node.Release("PNode");

                    using (lazy_up_to_appstate())
                        node.IsSelected = node.IsExpanded = false;

                    @dispatch(new BackgroundMessage
                    {
                        Token = Consts.BackToken_deltag,
                        Message = new
                        {
                            node.ID,
                            node.PID,
                            db = IoC.Resolve<DbConnInfo>().DB,
                            pwd = IoC.Resolve<DbConnInfo>().Pwd,
                        },
                    });
                });
            }
        }

        [AutoSubscribe("TagTreeViewModel__AddNewPaInfo")]
        void on_TagTreeViewModel__AddNewPaInfo((int id, bool select) _)
        {
            (var id, var select) = _;
            dynamic node = this.FindNodeByID(0);
            ///node.IsExpanded = true;
            ///node.IsSelected = true;
            ///var msg = new FindPasMessage { Token = "afterAddNewPaInfo", Data = new NotifyBag() };
            ///msg.Data["PaID"] = pa.ID;
            ///msg.Data["PaType"] = pa.Type;
            ///Store.Dispatch(msg);
            node.IsSelected = false;
            this.SelCmd.TryInvoke(node as TreeNode);
        }

        [AutoSubscribe("on_searchtext")]
        void on_searchtext(string text)
        {
            FindNodeByID(-1)["IsSelected"] = true;
        }

        public TreeNode FindNodeByID(int? id)
        {
            if (id == null) return TreeNodes[0];
            else if (id == -1) return TreeNodes[1];
            else if (id == 0) return TreeNodes[1].Get<ObservableCollection<TreeNode>>("Nodes")[0];
            else return FindNodes(TreeNodes[1].Get<ObservableCollection<TreeNode>>("Nodes"), n => Equals(n.Get("ID"), id)).FirstOrDefault();
        }

        public IEnumerable<TreeNode> FindNodes(ObservableCollection<TreeNode> nodes, Func<TreeNode, bool> func)
        {
            foreach (var node in nodes)
            {
                if (func(node))
                    yield return node;

                var _nodes = node.Get("Nodes") as ObservableCollection<TreeNode>;
                if (_nodes == null) continue;

                foreach (var _node in FindNodes(_nodes, func))
                    yield return _node;
            }
        }

        [AutoSubscribe(Consts.DyActType.main_Clear)]
        void Clear(object _)
        {
            this.TryClearup();
            TreeNodes?.Clear();
        }

        private static bool in_PNode(dynamic node, int pid)
        {
            var pn = node.PNode;
            while (pn != null)
            {
                if (pn.ID == pid) return true;
                pn = pn.PNode;
            }
            return false;
        }

        private TreeNode set_treeitem_Expanded_and_Selected(dynamic node)
        {
            (node as TreeNode).PropertyChanged += _node_Expanded_and_Selected_Changed;
            do
            {
                var list = SData.Expanded;
                if (list.Any(j => (int?)j == (int?)node.ID)) node.IsExpanded = true;
                else node.IsExpanded = false;
            } while (false);
            do
            {
                if (AreEquals(SData.Selected, node.ID))
                {
                    //SelCmd.TryInvoke(node as object);
                    node.IsSelected = true;
                }
                else node.IsSelected = false;
            } while (false);
            return node;
        }

        private void _node_Expanded_and_Selected_Changed(object sender, PropertyChangedEventArgs e)
        {
            dynamic node = sender;
            var hasChanged = false;
            switch (e.PropertyName)
            {
                case "IsExpanded":
                    {
                        if (node._inited != true) break;
                        var list = SData.Expanded;
                        //if (node.IsExpanded == true && node.Nodes != null && node.HasChild == true) list.Add(node.ID);
                        if (node.IsExpanded == true && node.Nodes?.Count > 0) list.Add(node.ID);
                        else if (node.IsExpanded == false)
                        {
                            int i = -1, ii = i;
                            foreach (var j in list)
                            {
                                ii++;
                                if ((int?)j == (int?)node.ID)
                                {
                                    i = ii;
                                    break;
                                }
                            }
                            if (i > -1) list.RemoveAt(i);
                        }
                        hasChanged = true;
                    }
                    break;
                case "IsSelected":
                    {
                        if (node.IsSelected == true)
                        {
                            var old = ViewModelData[_notstorekeyprev + "sel_Node"] as TreeNode;
                            old?.Set("IsSelected", false);
                            ViewModelData[_notstorekeyprev + "sel_Node"] = node;
                            SData.Selected = node.ID;
                            hasChanged = true;
                        }
                    }
                    break;
                default: break;
            }

            if (hasChanged)
                using (lazy_up_to_appstate()) { }
        }

        private IDisposable lazy_up_to_appstate()
        {
            ++_cannot_up_to_appstate;
            return new Disposable(() =>
            {
                if ((--_cannot_up_to_appstate) == 0)
                    @dispatch(SData);
            });
        }
    }
}
