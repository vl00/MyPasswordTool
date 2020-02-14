using Common;
using MyPasswordTool.Models;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
    public class MainViewModel : NotifyObject, IMvvmAware, IClear
    {
        public MainViewModel()
        {
            OnInit();
        }

        private string _current_db;
        private DalRepository repository { get; } = IoC.Resolve<DalRepository>();

        public NotifyBag ViewModelData { get; } = new NotifyBag();

        bool _Lazyloaded;
        public bool Lazyloaded
        {
            get => _Lazyloaded;
            set => this.SetPropertyValue(ref _Lazyloaded, value);
        }

        protected void OnInit()
        {
        }

        public void Activate(object parameter)
        {
            if (Consts.TestCanLoadLazy != false) Lazyloaded = true;

            var canAct = true;
            if (parameter is LockActionMessage lockActionMessage && !lockActionMessage.IsLock)
            {
                if (lockActionMessage.DbInfo.DB != _current_db) _current_db = lockActionMessage.DbInfo.DB;
                else canAct = false;
            }
            if (canAct)
            {
                @dispatch(Consts.DyActType.main_Activate, parameter);
            }
        }

        public void Deactivate(object parameter)
        {
            @dispatch(Consts.DyActType.main_Deactivate, parameter);
        }

        public void Clear()
        {
            @dispatch(Consts.DyActType.main_Clear, Consts.Null);
        }

        private IDelegateCommand _AddNewPaInfoCmd;
        public IDelegateCommand AddNewPaInfoCmd => _AddNewPaInfoCmd = _AddNewPaInfoCmd ?? new DelegateCommand<int>(_exec_AddNewPaInfoCmd, _canexec_AddNewPaInfoCmd);

        private bool _canexec_AddNewPaInfoCmd(int type)
        {
            var time = ViewModelData["__AddNewPaInfoCmd_time"].CastTo<DateTime?>(false);
            return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1.5);
        }

        private async void _exec_AddNewPaInfoCmd(int type)
        {
            ViewModelData["__AddNewPaInfoCmd_time"] = DateTime.Now;

            var pa = new PaInfo
            {
                Type = type,
                Title = Consts.NewTagName,
            };
            pa.CreateTime = pa.UpdateTime = DateTime.Now;
            await repository.AddPaInfoAsync(pa);

            await @dispatchAsync("TagTreeViewModel__AddNewPaInfo", (id: 0, select: false));
            await @dispatchAsync(new RefreshPaInfosMessage { Type = "added" });

            ViewModelData.Release("__AddNewPaInfoCmd_time");
        }

        private IDelegateCommand _SearchCmd;
        public IDelegateCommand SearchCmd => _SearchCmd = _SearchCmd ?? new DelegateCommand<EventParamter>(_exec_SearchCmd, _canexec_SearchCmd);

        private bool _canexec_SearchCmd(EventParamter o)
        {
            var time = ViewModelData["__SearchCmd_time"].CastTo<DateTime?>(false);
            return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1.5);
        }

        private void _exec_SearchCmd(EventParamter o)
        {
            var text = o.Sender.Text as string;
            if ((text?.Trim() ?? null).IsNullOrEmpty()) return;

            @dispatch("on_searchtext", text);

            var msg = new FindPasMessage { Token = "search", Data = new NotifyBag() };
            msg.Data["text"] = text;
            @dispatch(msg);

            ViewModelData.Release("__SearchCmd_time");
        }
    }
}
