using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json.Linq;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Common.ObjectHelper;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
    public class PalockViewModel : DataErrorViewModelBase, IMvvmAware
    {
        public PalockViewModel()
        {
            OnInit();
        }

        private CancellationTokenSource cancel_db_vacuum;
        private DalRepository dal = IoC.Resolve<DalRepository>();

        private bool is_first_unlock => cancel_db_vacuum == null;

        public NotifyBag ViewModelData { get; } = new NotifyBag();

        private string _ConStr;
        public string ConStr
        {
            get { return _ConStr; }
            set { this.SetPropertyValue(ref _ConStr, value); }
        }

        private string _Pwd = "";
        public string Pwd
        {
            get { return _Pwd; }
            set { this.SetPropertyValue(ref _Pwd, value); }
        }

        private IDelegateCommand _OkCmd;
        public IDelegateCommand OkCmd
        {
            get
            {
                if (_OkCmd == null) ViewModelData[nameof(OkCmd)] = _OkCmd = new DelegateCommand<EventParamter>(_exec_OkCmd, _canexec_OkCmd);
                return _OkCmd;
            }
        }

        private bool _canexec_OkCmd(EventParamter o)
        {
            return ViewModelData["busy_OkCmd"] == null;
        }

        private void _exec_OkCmd(EventParamter o)
        {
            LogManager.GetLogger(this.GetType()).Info("PalockViewModel _exec_OkCmd");
            @dispatch(new Func<Dispatcher, Dispatcher, Func<AppState>, Task>(
                async (next, dispatch, getState) => 
                {
                    if (string.IsNullOrWhiteSpace(ConStr))
                    {
                        SetDataError("ConStr", "ConStr must be not empty");
                        return;
                    }
                    var c = (o.CommandParameter as object).CastTo<string>();
                    ViewModelData["busy_OkCmd"] = true;
                    try
                    {
                        if (c == "openDB") await openDB();
                        else if (c == "newDB") await newDB();
                    }
                    catch (Exception ex)
                    {
                        SetDataError("ConStr", ex.Message);
                        ViewModelData.Release("busy_OkCmd");
                        return;
                    }
                    await _after_ok();
                    ViewModelData.Release("busy_OkCmd");
                }
            ));
        }

        protected void OnInit()
        {
            ViewModelData.ListenPropertyChanged(e => e.PropertyName.StartsWith("busy_"), e => 
            {
                var prop_cmd = ViewModelData[e.PropertyName.TrimStart("busy_")];
                (prop_cmd as IDelegateCommand)?.RaiseCanExecuteChanged();
            });
        }

        public void Activate(object parameter = null)
        {
            var dbinfo = IoC.Resolve<DbConnInfo>();
            ConStr = dbinfo.DB;
            Pwd = "";
            ClearDataError(nameof(ConStr));
            ClearDataError(nameof(Pwd));

            if ((parameter as LockActionMessage)?.IsLock == true)
            {
                AsyncHelper.RunInBatch(async () =>
                {
                    //GC.Collect(2, GCCollectionMode.Forced, false);
                    if (AppPlatform.IsDebuggerAttached) GC.Collect();

                    cancel_db_vacuum = new CancellationTokenSource();
                    try { await Task.Delay(20 * 1000, cancel_db_vacuum.Token); } catch { }
                    if (cancel_db_vacuum?.IsCancellationRequested == true) return;
                    if (cancel_db_vacuum != null || (cancel_db_vacuum == null && ConStr != dbinfo.DB))
                    {
                        @dispatch(new BackgroundMessage
                        {
                            Token = Consts.db_vacuum,
                            Message = new
                            {
                                db = dbinfo.DB,
                                pwd = dbinfo.Pwd,
                            },
                        });
                    }
                });
            }
        }

        public void Deactivate(object parameter = null) { }

        private async Task openDB()
        {
            await dal.CheckDB(this.ConStr, this.Pwd);
            ClearDataError("ConStr");
        }

        private async Task newDB()
        {
            if (File.Exists(ConStr))
            {
                await openDB();
                return;
            }
            await dal.NewDB(ConStr, Pwd);
            ClearDataError("ConStr");
        }

        private async Task _after_ok()
        {
            await ThreadUtil.VoidTask;
            var olddbinfo = @state.DbConnInfo;
            if (!is_first_unlock && ConStr != olddbinfo.DB && olddbinfo.DB != null)
            {
                @dispatch(new BackgroundMessage
                {
                    Token = Consts.clean_tree,
                    Message = new
                    {
                        db = olddbinfo.DB,
                        pwd = olddbinfo.Pwd,
                    }
                });
                olddbinfo.Dispose();
            }
            await @dispatchAsync(DyAction.Create(Consts.DyActType.before_unlock_ok, (is_first_unlock, ConStr, olddbinfo.DB)));
            await @dispatchAsync(new LockActionMessage
            {
                IsLock = false,
                DbInfo = !is_first_unlock && ConStr == olddbinfo.DB ? olddbinfo : new DbConnInfo
                {
                    DB = ConStr,
                    Pwd = Pwd,
                }
            });
        }

        public override void Clear()
        {
            var c = cancel_db_vacuum;
            cancel_db_vacuum = null;
            c?.Cancel();
            c?.Dispose();
        }

        
    }
}
