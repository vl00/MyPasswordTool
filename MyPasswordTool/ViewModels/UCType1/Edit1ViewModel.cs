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
    //[ExportAsViewModel("edit1")]
    public class Edit1ViewModel : NotifyObject, IMvvmAware, IClear
    {
        //private DucPaInfoModel ducPaInfoModel;
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
            if (parameter == null) throw new ArgumentException("error=Edit1ViewModel--OnActivate");

            OnActivate_by_ducpa_displaytoedit(parameter);
        }

        private async void OnActivate_by_ducpa_displaytoedit(object data)
        {
            paNode = data;

            var pa = await repository.GetAsync<PaInfo>((int)paNode.ID);
            PaInfo = pa.ModelToBag<NotifyBag>();
            DataModel = pa.Data.ToObject<NotifyBag>() ?? new NotifyBag();
            Tags = await repository.FindTagsByPaInfoID(pa.ID);

            dynamic dfm = new NotifyBag();
            var pf = await repository.GetPaInfoFile(pa.ID);
            dfm.GetFile = new Func<Task<byte[]>>(() => repository.GetPaInfoFileData(pa.ID));
            dfm.IsUpdated = false;
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

        private IDelegateCommand _SaveCmd;
        public IDelegateCommand SaveCmd => _SaveCmd = _SaveCmd ?? new DelegateCommand<EventParamter>(async o =>
        {
            ViewModelData["saving"] = true;

            var pa = PaInfo.BagToModel<PaInfo>();
            pa.UpdateTime = DateTime.Now;
            pa.Data = DataModel.ToJson();

            PaInfoFile pfm = null;
            dynamic dfm = FileModel;
            if (dfm.IsUpdated == true)
            {
                pfm = new PaInfoFile
                {
                    ID = pa.ID,
                    File = dfm.File as byte[],
                    FileExtname = dfm.FileExtname as string,
                };
                if (dfm.FileSize != null) pfm.FileSize = (dfm.FileSize as object).CastTo<double?>();
            }

            await repository.SavePaInfoAsync(pa, Tags, pfm);

            ViewModelData.Release("saving");

            Store.Dispatch(new RefreshPaInfosMessage { Type = "updated", Payload = Tags.Select(t => t.ID).ToArray() });
            RouteToDisplayCmd.TryInvoke(new EventParamter { CommandParameter = true }).TryThrow();
        }, o =>
        {
            return !ViewModelData.Has("saving");
        });

        private IDelegateCommand _RouteToDisplayCmd;
        public IDelegateCommand RouteToDisplayCmd => _RouteToDisplayCmd = _RouteToDisplayCmd ?? new DelegateCommand<EventParamter>(o =>
        {
            var is_new = (o.CommandParameter as object).CastTo<bool>();
            var nav = new NavigationMessage { Parameter = paNode };
            if (is_new)
            {
                nav.Type = Consts.NS.Duc;
                nav.ViewKey = string.Format(Consts.ViewKey.Duc, 1);
            }
            else
            {
                nav.Type = Consts.NS.Edit_BackTo_Duc;
            }
            Store.Dispatch(nav);
        });
    }
}
