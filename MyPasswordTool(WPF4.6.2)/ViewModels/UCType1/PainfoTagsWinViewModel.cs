using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool.ViewModels
{
    public class PainfoTagsWinViewModel : NotifyObject, IMvvmAware, IClear
    {
        private dynamic Tags;
        private DalRepository repository { get; } = IoC.Resolve<DalRepository>();

        private bool _IsOK;
        public bool IsOK
        {
            get { return _IsOK; }
            set { this.SetPropertyValue(ref _IsOK, value); }
        }

        public ObservableCollection<TreeNode> TreeNodes { get; set; } = new ObservableCollection<TreeNode>();

        public void Activate(object parameter)
        {
            Tags = parameter;
            _get_tree(null, -1);
        }

        private async void _get_tree(dynamic node, int pid)
        {
            var tags = await Task.FromResult(Tags.GetTags() as PaTag[]);
            var q = (await repository.GetChildrenPaTags(pid)).Select(tag =>
            {
                dynamic dy = tag.ModelToBag<TreeNode>();
                dy.PNode = node;
                dy.Nodes = new ObservableCollection<TreeNode>();
                dy.TagICO = Consts.ico_tag;
                dy.IsExpanded = true;
                return dy;
            });
            foreach (dynamic _q in q)
            {
                if (tags.Any(t => t.ID == _q.ID)) _q.IsSelected = true;
                (node != null ? node.Nodes : TreeNodes).Add(_q);
                _get_tree(_q, _q.ID);
            }
        }

        public void Deactivate(object parameter)
        {
            var tags = findNodes(TreeNodes, n => n["IsSelected"].CastTo<bool?>() == true).Select(n => n.BagToModel<PaTag>());
            Tags.SetTags(tags.ToArray());
        }

        public void Clear()
        {
            Tags = null;
            TreeNodes?.Clear();
        }

        private IEnumerable<TreeNode> findNodes(ObservableCollection<TreeNode> nodes, Func<TreeNode, bool> func)
        {
            foreach (var node in nodes)
            {
                if (func(node))
                    yield return node;

                var _nodes = node.Get("Nodes") as ObservableCollection<TreeNode>;
                if (_nodes == null) continue;

                foreach (var _node in findNodes(_nodes, func))
                    yield return _node;
            }
        }
    }
}
