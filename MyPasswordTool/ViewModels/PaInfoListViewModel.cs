using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilverEx;
using SilverEx.DataVirtualization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.ObjectHelper;
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
	public partial class PaInfoListViewModel : NotifyObject
	{
        public PaInfoListViewModel()
        {
            OnInit();
        }

        private const int _pas_pagesize = 15;
        private const int _pas_pagetimeout = 2500; //0
        private double? voff = null;
        private string order;
        private object _initd;
        private int _cannot_up_to_appstate = 0;
        private PaListStoreData StoreData = new PaListStoreData();

        private DalRepository repository { get; } = IoC.Resolve<DalRepository>();

        public NotifyBag ViewModelData { get; } = new NotifyBag();

        public event Action PasInitialized;

        private AsyncVirtualizingCollection<SimPaModel> _pas;
		public AsyncVirtualizingCollection<SimPaModel> Pas
		{
			get { return _pas; }
			set
            {
                if (this.SetPropertyValue(ref _pas, value) && _pas != null)
                {
                    IDisposable d = null;
                    d = Pas.ListenPropertyChanged(e => !Pas.IsInitializing, e =>
                    {
                        d?.Dispose();
                        PasInitialized?.Invoke();
                    });
                }
            }
		}

        public string Order => order;

        public int PasCount => Pas?.Count ?? 0;

        private void OnInit()
		{
            this.TryInit();
			{
                var cm = @state.JX?["PaList"];
                voff = ((double?)cm?["voff"]) ?? 0;
                order = ((string)cm?["Order"]) ?? Consts.Order.UpdateTime;
                StoreData.FindPasMessage = cm?["FindPasMessage"]?.ToObject<FindPasMessage>();
				StoreData.RefeshActionInfo = (string)cm?["RefeshActionInfo"];
                StoreData.Selected = new JObject();
                if (cm?["Selected"] != null)
                {
                    //StoreData.Selected["ID"] = cm["Selected"]?["ID"];
                    //StoreData.Selected["Type"] = ((int?)cm["Selected"]?["Type"]) ?? 0;
                    //StoreData.Selected["IsDeleted"] = ((bool?)cm["Selected"]?["IsDeleted"]) ?? false;
                    StoreData.Selected = JObject.FromObject(cm["Selected"]);
                }
            } 
			{
                _initd = this;
                this.PasInitialized += _sync_voff_on_PasInitialized;
            } 
		}

        [AutoSubscribe(Consts.DyActType.main_Activate)]
		void Activate(object parameter)
		{
            using (lazy_up_to_appstate())
            {
                refesh();
                if (_initd != null && StoreData.Selected?["ID"] != null)
                {
                    var o = StoreData.Selected;
                    _sel_nav(new
                    {
                        ID = (int)o["ID"],
                        Type = (int)o["Type"],
                        IsDeleted = (bool)o["IsDeleted"],
                    });
                }
            }
        }

        //[AutoSubscribe(Consts.DyActType.main_Deactivate)]
        //void Deactivate(object paramete) { }

        [AutoSubscribe(Consts.DyActType.main_Clear)]
		void Clear(object _)
        { 
			Pas = null;
            this.PasInitialized -= _sync_voff_on_PasInitialized;
            this.TryClearup();
        }

		private IDelegateCommand _SelCmd;
        public IDelegateCommand SelCmd => _SelCmd = _SelCmd ?? new DelegateCommand<SimPaModel>(_exec_SelCmd_, _can_exec_SelCmd_);

        bool _can_exec_SelCmd_(SimPaModel pa)
        {
            var time = ViewModelData["_SelCmd_time"].CastTo<DateTime?>(false);
            return time == null || ((time.Value - DateTime.Now).TotalSeconds >= 1);
        }

        void _exec_SelCmd_(SimPaModel pa)
        {
            ViewModelData["_SelCmd_time"] = DateTime.Now;
            pa["IsSelected"] = true;
            _sel_nav(pa);
            ViewModelData.Release("_SelCmd_time");
        }

        private void _sel_nav(dynamic pa)
        {
            @dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
            var nav = new NavigationMessage { Parameter = pa };
            if (pa.IsDeleted)
            {
                nav.Type = Consts.NS.DucTrash;
                nav.ViewKey = Consts.ViewKey.DucTrash;
            }
            else
            {
                nav.Type = Consts.NS.Duc;
                nav.ViewKey = string.Format(Consts.ViewKey.Duc, pa.Type);
            }
            @dispatch(nav);
        }

        [AutoSubscribe]
        void OnHandleMessage(FindPasMessage message)
        {
            @dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
            using (lazy_up_to_appstate())
            {
                StoreData = new PaListStoreData();
                StoreData.Selected = new JObject();
                StoreData.FindPasMessage = message;
                refesh(message);
            }
        }

        [AutoSubscribe]
        async void OnHandleMessage(RefreshPaInfosMessage message)
        {
            switch (message.Type)
            {
                case null:
                    refesh();
                    break;
                case "added":
                case "updated":
                    await @dispatchAsync(Consts.DyActType.palsOrderChanged, Consts.Order.UpdateTime);
                    refesh();
                    break;
                //case "updated":
                //    {
                //        var b = (message.Payload as int[]).Any(i => i == ((int?)@state.JX?["TagTree"]?["Selected"]));
                //        if (!b) refesh();
                //        else if (ViewModelData["__selected_item"] is SimPaModel m)
                //        {
                //            var pa = await repository.GetAsync<SimplePaInfo>(m.Get<int>("ID"));
                //            foreach (var kv in pa.ModelToBag<SimPaModel>().Dictionary())
                //                m.Set(kv.Key, kv.Value);
                //        }
                //    }
                //    break;
            }
        }

        private void refesh() => refesh(StoreData.FindPasMessage);

        private void refesh(FindPasMessage message)
		{
            if (message == null) return;
            using (lazy_up_to_appstate())
            {
                switch (message.Token)
                {
                    case "TreeTag":
                        {
                            dynamic data = message.Data;
                            if (data.ID == null)
                            {
                                StoreData.RefeshActionInfo = "TreeTag_trash";
                                _get_painfo_trash();
                            }
                            else if (data.ID == -1)
                            {
                                StoreData.RefeshActionInfo = "TreeTag_all";
                                _get_painfo_all();
                            }
                            else if (data.ID == 0)
                            {
                                StoreData.RefeshActionInfo = "TreeTag_notag";
                                _get_painfo_withnoTag();
                            }
                            else
                            {
                                StoreData.RefeshActionInfo = "TreeTag_tag";
                                _get_painfo_byTagID((int)data.ID);
                            }
                        }
                        break;
                    case "afterAddNewPaInfo":
                        {
                            dynamic data = message.Data;
                            _initd = StoreData.Selected["ID"] = data.PaID;
                            StoreData.Selected["Type"] = data.PaType;
                            StoreData.Selected["IsDeleted"] = false;
                            StoreData.RefeshActionInfo = "TreeTag_notag";
                            _get_painfo_withnoTag();
                        }
                        break;
                    case "afterdelfromtrash":
                        {
                            StoreData.Selected = new JObject();
                            _initd = StoreData.RefeshActionInfo = "TreeTag_trash";
                            _get_painfo_trash();
                        }
                        break;
                    case "search":
                        {
                            StoreData.RefeshActionInfo = "search";
                            _get_painfo_search((string)message.Data["text"]);
                            @dispatch(new ChangeSearchText { Text = (string)message.Data["text"] });
                        }
                        break;
                    default:
                        Pas = null;
                        break;
                }
                if (message.Token != "search")
                {
                    @dispatch(new ChangeSearchText { Text = null });
                }
            }
		}

        private async void _get_painfo_trash() => _set_Pas(await repository.GetSimplePainfoByTrash(order));
        private async void _get_painfo_all() => _set_Pas(await repository.GetSimplePainfoByAll(order));
        private async void _get_painfo_withnoTag() => _set_Pas(await repository.GetSimplePainfoByNoTag(order));
		private async void _get_painfo_byTagID(int tagId) => _set_Pas(await repository.GetSimplePainfoByTagID(tagId, order));
        private async void _get_painfo_search(string text) => _set_Pas(await repository.GetSimplePainfoBySearch(text, order));

        private void _set_Pas(FuncItemsProvider<SimplePaInfo> f)
        {
            //Pas = null;
            Pas = new AsyncVirtualizingCollection<SimPaModel>(async (i, j) =>
            {
                var ls = new List<SimPaModel>();
                using (lazy_up_to_appstate())
                {
                    foreach (var q in await f.FetchRange(i, j))
                    {
                        ls.Add(_setitem(q.ModelToBag<SimPaModel>()));
                    }
                }
                on_Pas_items_data_loaded(ls, ls.Count > 0 ? i : -1);
                return ls;
            }, async () =>
            {
                var i = await Task.Run(() => f.FetchCount());
                await ThreadUtil.CallOnUIThread(() => this.RaisePropertyChanged(nameof(PasCount)));
                return i;
            }, _pas_pagesize, _pas_pagetimeout);
        }

        [AutoSubscribe(Consts.DyActType.palsOrderChanged)]
        void OnHandleMessage(string ord)
        {
            if (this.order == ord) return;
            this.order = ord;
            StoreData.Selected = new JObject();
            @dispatch(new RefreshPaInfosMessage());
        }

        [AutoSubscribe]
        async void OnHandleMessage(DropPaInfoToTagMessage message)
        {
            DataWrapper<SimPaModel> sourceWrap = message.SimplePaInfo;
            dynamic source = message.SimplePaInfo.Data;
            dynamic tnode = message.Tag;

            if (source == null || tnode == null || tnode.ID == -1) return;
            if (tnode.ID == null && StoreData.RefeshActionInfo == "TreeTag_trash") return;
            if (tnode.ID == null)
            {
                if (tnode.IsSelected == true) return;
                await _painfo_to_trash((source as SimPaModel).BagToModel<SimplePaInfo>());
                //refesh();
                Pas.Refesh();
                this.RaisePropertyChanged(() => PasCount);
                @dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
                return;
            }
            if (tnode.ID == 0 && StoreData.RefeshActionInfo == "TreeTag_notag") return;
            if (tnode.ID == 0)
            {
                await repository.ClearPaInfoTagByPaInfoID((int)source.ID);
                Pas.Refesh();
                this.RaisePropertyChanged(() => PasCount);
                @dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
                return;
            }
            {
                await repository.AddPaInfoTag((int)source.ID, (int)tnode.ID);
                if (
                     StoreData.RefeshActionInfo == "TreeTag_trash" || 
                     StoreData.RefeshActionInfo == "TreeTag_notag"
                   )
                {
                    Pas.Refesh();
                    //this.RaisePropertyChanged(() => PasCount);
                    @dispatch(new NavigationMessage { Type = Consts.NS.EmptyDuc });
                }
                else
                {
                    SelCmd.TryInvoke(source as SimPaModel).TryThrow();
                }
            } 
        }

        private async Task _painfo_to_trash(SimplePaInfo simplePaInfo)
		{
            simplePaInfo.IsDeleted = true;
            simplePaInfo.UpdateTime = DateTime.Now;
            await repository.UpdateSimplePaInfoAsync(simplePaInfo);
        }

		private SimPaModel _setitem(SimPaModel bag)
		{
            bag.PropertyChanged -= _node_IsSelected_Changed;
            bag.PropertyChanged += _node_IsSelected_Changed;
            bag["IsSelected"] = Equals(bag["ID"].ToString(), StoreData.Selected?["ID"]?.ToString());
			return bag;
		}

        private void _node_IsSelected_Changed(object sender, PropertyChangedEventArgs e)
        {
            dynamic item = sender.AsDynamic();
            if (e.PropertyName != "IsSelected" || item.IsSelected != true) return;

            using (lazy_up_to_appstate())
            {
                StoreData.Selected["ID"] = item.ID;
                StoreData.Selected["Type"] = item.Type;
                StoreData.Selected["IsDeleted"] = item.IsDeleted;
            }

            try
            {
                if (ViewModelData["__selected_item"] is SimPaModel o)
                {
                    o.PropertyChanged -= _node_IsSelected_Changed;
                    o["IsSelected"] = false;
                    o.PropertyChanged += _node_IsSelected_Changed;
                }
            }
            catch { }
            ViewModelData["__selected_item"] = item;    //it won't be 2 Selected in top and bottom
        }

        private void on_Pas_items_data_loaded(List<SimPaModel> ls, int itemIndexForBag0)
        {
            if (_initd != null) //first loaded
            {
                _initd = null;  
            }
            else if (_initd == null && itemIndexForBag0 == 0)
            {
                ThreadUtil.CallOnUIThread(() => _exec_SelCmd_(ls[0]));
            }
        }

        private void _sync_voff_on_PasInitialized()
        {
            //if (voff == null) return;
            var _v = voff;
            voff = null;
            @dispatch(new PaListScrollData { Hoff = null, Voff = _v ?? 0 });
            //voff = null;
        }

        private IDisposable lazy_up_to_appstate()
        {
            ++_cannot_up_to_appstate;
            return new Disposable(() => 
            {
                if ((--_cannot_up_to_appstate) == 0)
                    @dispatch(StoreData);
            });
        }
    }
}
