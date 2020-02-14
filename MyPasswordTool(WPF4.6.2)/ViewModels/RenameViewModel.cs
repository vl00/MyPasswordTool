using Common;
using MyPasswordTool.Models;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool.ViewModels
{
    public class RenameViewModel : NotifyObject, IMvvmAware, IClear
    {
        private dynamic node;
        private readonly DalRepository repository = IoC.Resolve<DalRepository>();

        public NotifyBag ViewModelData { get; } = new NotifyBag();

        private bool _IsUpName;
        public bool IsUpName
        {
            get { return _IsUpName; }
            set { this.SetPropertyValue(ref _IsUpName, value); }
        }

        private string _Name;
        public string Name 
        {
            get { return _Name; }
            set 
            {
                if (this.SetPropertyValue(ref _Name, value))
                    RenameCmd.RaiseCanExecuteChanged();
            }
        }

        public void Activate(object parameter = null)
        {
            node = parameter;
            Name = node.Name;
            RenameCmd.RaiseCanExecuteChanged();
        }

        public void Deactivate(object parameter = null)
        {
            Clear();
        }

        private IDelegateCommand _RenameCmd;
        public IDelegateCommand RenameCmd => _RenameCmd = _RenameCmd ?? new DelegateCommand<EventParamter>(async o =>
        {
            var tag = (node as TreeNode).BagToModel<PaTag>();
            tag.Name = this.Name;
            ViewModelData.Set("busy_RenameCmd", true);
            await repository.UpdatePaTagAsync(tag);
            node.Name = this.Name;
            ViewModelData.Release("busy_RenameCmd");
            IsUpName = true;
        }, o =>
        {
            if (ViewModelData.Has("busy_RenameCmd")) return false;
            return node != null && !string.IsNullOrEmpty(Name) && !(Name == node.Name);
        });

        public void Clear()
        {
            node = null;
        }
    }
}
