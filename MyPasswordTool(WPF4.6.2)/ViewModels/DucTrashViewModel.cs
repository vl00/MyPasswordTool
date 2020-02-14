using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.ObjectHelper;

namespace MyPasswordTool.ViewModels
{
    public class DucTrashViewModel : NotifyObject, IMvvmAware, IClear
    {
        public DucTrashViewModel()
        {
            OnInit();
        }

        private DalRepository repository { get; } = IoC.Resolve<DalRepository>();

        public IStore Store { get; } = IoC.Resolve<IStore>();
        public NotifyBag ViewModelData { get; } = new NotifyBag();

        private NotifyBag _PaInfo;
        public NotifyBag PaInfo
        {
            get { return _PaInfo; }
            set { this.SetPropertyValue(ref _PaInfo, value); }
        }

        private void OnInit()
        {
            ViewModelData["busying"] = false;
            ViewModelData.ListenPropertyChanged("busying", e => 
            {
                DelCmd.RaiseCanExecuteChanged();
                ResumeCmd.RaiseCanExecuteChanged();
            });
        }

        public async void Activate(object parameter)
        {
            if (parameter == null) throw new ArgumentException("error=DucTrashViewModel--Activate");
            dynamic simplePainfo = parameter;
           
            var pa = await repository.GetAsync<SimplePaInfo>((int)simplePainfo.ID);
            PaInfo = pa.ModelToBag<NotifyBag>();
        }

        public void Deactivate(object parameter)
        {
            Clear();
        }

        public void Clear()
        {
            //PaInfo?.Dispose();
            //PaInfo = null;
        }

        private IDelegateCommand _DelCmd;
        public IDelegateCommand DelCmd
        {
            get
            {
                return _DelCmd = _DelCmd ?? new DelegateCommand<EventParamter>(async o => 
                {
                    ViewModelData["_DelCmd_time"] = DateTime.Now;
                    ViewModelData["busying"] = true;

                    await repository.DelTruePaInfoAsync((int)PaInfo["ID"]);

                    ViewModelData["busying"] = false;
                    ViewModelData.Release("_DelCmd_time");

                    Store.Dispatch(new FindPasMessage { Token = "afterdelfromtrash" });
                    Store.Dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
                }, o => 
                {
                    if (AreEquals(ViewModelData["busying"], true)) return false;
                    var time = ViewModelData["_DelCmd_time"]?.CastTo<DateTime?>(false);
                    return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1.5);
                });
            }
        }

        private IDelegateCommand _ResumeCmd;
        public IDelegateCommand ResumeCmd
        {
            get
            {
                return _ResumeCmd = _ResumeCmd ?? new DelegateCommand<EventParamter>(async o =>
                {
                    ViewModelData["_ResumeCmd_time"] = DateTime.Now;
                    ViewModelData["busying"] = true;

                    PaInfo["UpdateTime"] = DateTime.Now;
                    PaInfo["IsDeleted"] = false;
                    await repository.UpdateSimplePaInfoAsync(PaInfo.BagToModel<SimplePaInfo>());

                    ViewModelData["busying"] = false;
                    ViewModelData.Release("_ResumeCmd_time");

                    Store.Dispatch(new FindPasMessage { Token = "afterdelfromtrash" });
                    Store.Dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
                }, o => 
                {
                    if (AreEquals(ViewModelData["busying"], true)) return false;
                    var time = ViewModelData["_ResumeCmd_time"]?.CastTo<DateTime?>();
                    return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1.5);
                });
            }
        }

    }
}
