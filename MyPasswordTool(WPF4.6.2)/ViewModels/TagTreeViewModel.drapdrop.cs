using Common;
using GongSolutions.Wpf.DragDrop;
using MyPasswordTool.Models;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GongSolutionsDragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
    public partial class TagTreeViewModel : IDropTarget, IDragSource
    {
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var sgroup = (Consts.GetDrapDropGroup(dropInfo.DragInfo.VisualSource) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var tgroup = (Consts.GetDrapDropGroup(dropInfo.VisualTarget) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            dropInfo.Effects = DragDropEffects.None;
            if ("tag".In(sgroup) && "tag".In(tgroup))
            {
                if (dropInfo.VisualTarget.GetType() == dropInfo.DragInfo.VisualSource.GetType())
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                else
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if ("painfo".In(sgroup) && "painfo".In(tgroup))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var sgroup = (Consts.GetDrapDropGroup(dropInfo.DragInfo.VisualSource) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var tgroup = (Consts.GetDrapDropGroup(dropInfo.VisualTarget) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if ("tag".In(sgroup) && "tag".In(tgroup))
            {
                drop_tag(dropInfo, sgroup, tgroup);
            }
            else if ("painfo".In(sgroup) && "painfo".In(tgroup)) 
            {
                drop_painfo_to_tag(dropInfo, sgroup, tgroup);
            }
        }

        bool IDragSource.CanStartDrag(IDragInfo dragInfo)
        {
            var group = (Consts.GetDrapDropGroup(dragInfo.VisualSource) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!"tag".In(group)) return false;
            var node = dragInfo.SourceItem as NotifyBag;
            return node != null && !node.Get("ID").In(null, 0, -1);
        }

        void IDragSource.StartDrag(IDragInfo dragInfo)
        {
            GongSolutionsDragDrop.DefaultDragHandler.StartDrag(dragInfo);
        }

        void IDragSource.Dropped(IDropInfo dropInfo) { }
        void IDragSource.DragCancelled() { }
        public bool TryCatchOccurredException(Exception exception) => false;
    }

    public partial class TagTreeViewModel
    {
        private void drop_tag(IDropInfo dropInfo, string[] sgroup, string[] tgroup)
        {
            using (lazy_up_to_appstate())
            {
                if ("tag_dropinto".In(tgroup)) drop_tag_into(dropInfo, sgroup, tgroup);
                else drop_tag_order(dropInfo, sgroup, tgroup);
            }
        }

        async void drop_tag_into(IDropInfo dropInfo, string[] sgroup, string[] tgroup)
        {
            dynamic snode = dropInfo.DragInfo.SourceItem;
            dynamic tnode = dropInfo.VisualTarget.AsTo<FrameworkElement>().DataContext;
            if (tnode.ID == 0 || snode.ID == tnode.ID || snode.PID == tnode.ID) return;
            if (tnode.ID == null)
            {
                DelTagCmd.TryInvoke(snode as object);
                return;
            }
            if (in_PNode(tnode, snode.ID))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var old_snPNode = snode.PNode;
            old_snPNode.Nodes.Remove(snode);
            old_snPNode.HasChild = old_snPNode.Nodes.Count > 0;
            tnode.Nodes.Add(snode);
            snode.PID = tnode.ID;
            snode.PNode = tnode;
            tnode.IsExpanded = true;
            tnode.HasChild = true;
            _orderbynodeschildren(tnode.Nodes, snode.ID);

            var sa = ((IEnumerable<TreeNode>)tnode.Nodes).Union(new TreeNode[] { old_snPNode, tnode, snode }).Select(n => n.BagToModel<PaTag>()).ToArray();
            await repository.UpdatePaTagsAsync(sa);
        }

        async void drop_tag_order(IDropInfo dropInfo, string[] sgroup, string[] tgroup)
        {
            dynamic snode = dropInfo.DragInfo.SourceItem;
            dynamic tnode = dropInfo.TargetItem;
            if (tnode == null || tnode.ID == null || tnode.ID == -1 || snode.ID == tnode.ID) return;
            if (tnode.ID == 0 && dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem) return;
            if (tnode.ID == 0 && dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem && tnode.PNode.Nodes[1] == snode) return;
            if (in_PNode(tnode, snode.ID))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var old_snPNode = snode.PNode;
            old_snPNode.Nodes.Remove(snode);
            old_snPNode.HasChild = old_snPNode.Nodes.Count > 0;
            snode.Release("PNode");
            int t_index = tnode.PNode.Nodes.IndexOf(tnode);
            var dindex = t_index + (dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem ? 0 :
                                    dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem ? 1 :
                                    0);
            dindex = dindex < 0 ? 0 : dindex;
            if (dindex < tnode.PNode.Nodes.Count) tnode.PNode.Nodes.Insert(dindex, snode);
            else tnode.PNode.Nodes.Add(snode);
            snode.PID = tnode.PNode.ID;
            snode.PNode = tnode.PNode;
            tnode.PNode.IsExpanded = true;
            tnode.PNode.HasChild = true;
            _orderbynodeschildren(tnode.PNode.Nodes, snode.ID);

            var sa = ((IEnumerable<TreeNode>)tnode.PNode.Nodes).Union(new TreeNode[] { old_snPNode, tnode.PNode, snode }).Select(n => n.BagToModel<PaTag>()).ToArray();
            await repository.UpdatePaTagsAsync(sa);
        }

        private void _orderbynodeschildren(ObservableCollection<TreeNode> tns, int snote_id)
        {
            var orders = tns.Select(tn => tn.Get<int>("Order")).OrderBy(i => i).ToArray();
            var ts = new List<PaTag>();
            tns.Foreach((tn, i) =>
            {
                var t = tn.BagToModel<PaTag>();
                var t_order = t.Order;
                if (t_order != orders[i]) tn.Set("Order", t.Order = orders[i]);
                if (t.ID == snote_id || t_order != orders[i]) ts.Add(t);
            });
        }
    }

    public partial class TagTreeViewModel
    {
        private void drop_painfo_to_tag(IDropInfo dropInfo, string[] sgroup, string[] tgroup)
        {
            dynamic source = dropInfo.DragInfo.Data;
            dynamic tnode = dropInfo.VisualTarget.AsTo<FrameworkElement>().DataContext;

            @dispatch(new DropPaInfoToTagMessage { SimplePaInfo = source, Tag = tnode });
        }
    }
}
