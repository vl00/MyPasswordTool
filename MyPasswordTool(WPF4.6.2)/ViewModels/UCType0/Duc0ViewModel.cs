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

namespace MyPasswordTool.ViewModels
{
    public class Duc0ViewModel : NotifyObject, IMvvmAware, IClear
    {
        private dynamic paNode;
        private DalRepository repository { get; } = IoC.Resolve<DalRepository>();

        public NotifyBag ViewModelData { get; } = new NotifyBag();
        public IStore Store { get; } = IoC.Resolve<IStore>();

        private NotifyBag _PaInfo;
        public NotifyBag PaInfo
        {
            get { return _PaInfo; }
            set { this.SetPropertyValue(ref _PaInfo, value); }
        }

        private NotifyBag _DataModel;
        public NotifyBag DataModel
        {
            get { return _DataModel; }
            set { this.SetPropertyValue(ref _DataModel, value); }
        }

        private PaTag[] _Tags;
        public PaTag[] Tags
        {
            get { return _Tags; }
            set { this.SetPropertyValue(ref _Tags, value); }
        }

        private NotifyBag _FileModel;
        public NotifyBag FileModel
        {
            get { return _FileModel; }
            set { this.SetPropertyValue(ref _FileModel, value); }
        }

        public void Activate(object parameter)
        {
            if (parameter == null) throw new ArgumentException("error=Duc0ViewModel--Activate");

            OnActivate_by_ducpa_fromlist(parameter);
        }

        private async void OnActivate_by_ducpa_fromlist(object data)
        {
            dynamic simplePainfo = paNode = data;
            if (simplePainfo == null || simplePainfo.Type != 0) throw new ArgumentException("error=Duc0ViewModel--OnActivate_ducpa_fromlist");

            var pa = await repository.GetAsync<PaInfo>((int)simplePainfo.ID);
            PaInfo = pa.ModelToBag<NotifyBag>();
            DataModel = pa.Data.ToObject<NotifyBag>() ?? new NotifyBag();
            Tags = await repository.FindTagsByPaInfoID(pa.ID);

            dynamic dfm = new NotifyBag();
            var pf = await repository.GetPaInfoFile(pa.ID);
            dfm.GetFile = new Func<Task<byte[]>>(() => repository.GetPaInfoFileData(pa.ID));
            dfm.FileExtname = pf?.FileExtname;
            dfm.FileSize = pf?.FileSize;
            FileModel = dfm;
        }

        public void Deactivate(object parameter) { }

        public void Clear()
        {
            paNode = null;
            PaInfo = null;
            DataModel = null;
            FileModel = null;
        }

        private IDelegateCommand _RouteToEditCmd;
        public IDelegateCommand RouteToEditCmd => _RouteToEditCmd = _RouteToEditCmd ?? new DelegateCommand<EventParamter>(o =>
        {
            ViewModelData["_EditCmd_time"] = DateTime.Now;

            var nav = new NavigationMessage { Type = Consts.NS.Edit, Parameter = paNode };
            nav.ViewKey = string.Format(Consts.ViewKey.Edit, 0);
            Store.Dispatch(nav);

            ViewModelData.Release("_EditCmd_time");
        }, o =>
        {
            var time = ViewModelData["_EditCmd_time"].CastTo<DateTime?>(false);
            return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1.5);
        });
    }
}
