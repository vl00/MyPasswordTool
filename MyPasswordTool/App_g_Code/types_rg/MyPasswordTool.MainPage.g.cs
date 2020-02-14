//2019/10/7 12:14:06

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;
using MyPasswordTool.Models;
using MyPasswordTool.ViewModels;
using SilverEx;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using static rg.SimpleRedux;

namespace MyPasswordTool
{
	partial class MainPage : __IObj__
	{
		int __iC__;

	    #region __undefined__
	    #endregion __undefined__
	    #region rg.SimpleRedux.@sstobv
	    static partial void @sstobv<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg); 
	    static partial void @sstobv_<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg); 
	    
	    SimpleRedux.UnSubscribe _0003un_0;
	    
	    MyPasswordTool.Models.LockActionMessage _0003_when_lock_29256072(MyPasswordTool.Models.AppState _){return _.Action as LockActionMessage;}
	    SimpleRedux.UnSubscribe _0003un_1;
	    
	    MyPasswordTool.Models.NavigationMessage _0003_on_nav_3a856073(MyPasswordTool.Models.AppState _){return _.Action as NavigationMessage;}
	    #endregion rg.SimpleRedux.@sstobv
	    #region rg.SimpleRedux.AutoSubscribe
	    SimpleRedux.UnSubscribe _0004un_0;
	    
	    #endregion rg.SimpleRedux.AutoSubscribe
	    
	    void __IObj__.__init__() { __init__(); }
	    void __IObj__.__cleanup__() { __cleanup__(); }

	    protected virtual void __init__()
	    {
			var _ic_ = Interlocked.CompareExchange(ref __iC__, 1, 0);
			if (_ic_ != 0) return;

	        #region __undefined__
	        #endregion __undefined__
	        #region rg.SimpleRedux.@sstobv
	        { 
	            var _0003ov__0003_when_lock_29256072 = default(MyPasswordTool.Models.LockActionMessage); 
	            _0003un_0 = @store.Subscribe(() => 
	            { 
	                var _0003nv__0003_when_lock_29256072 = _0003_when_lock_29256072(@store.GetState()); 
	                var _0003b = false; 
	                if (!(ReferenceEquals(_0003nv__0003_when_lock_29256072, _0003ov__0003_when_lock_29256072) || Equals(_0003nv__0003_when_lock_29256072, _0003ov__0003_when_lock_29256072))) 
	                { 
	                      _0003b = true; 
	                } 
	                if (_0003b)
	                { 
	                    _when_lock(act: _0003nv__0003_when_lock_29256072); 
	                } 
	            }); 
	        } 
	        { 
	            var _0003ov__0003_on_nav_3a856073 = default(MyPasswordTool.Models.NavigationMessage); 
	            _0003un_1 = @store.Subscribe(() => 
	            { 
	                var _0003nv__0003_on_nav_3a856073 = _0003_on_nav_3a856073(@store.GetState()); 
	                var _0003b = false; 
	                if (!(ReferenceEquals(_0003nv__0003_on_nav_3a856073, _0003ov__0003_on_nav_3a856073) || Equals(_0003nv__0003_on_nav_3a856073, _0003ov__0003_on_nav_3a856073))) 
	                { 
	                      _0003b = true; 
	                } 
	                if (_0003b)
	                { 
	                    var _0003t = _on_nav(act: _0003nv__0003_on_nav_3a856073);
	                    if (_0003t != null) @tasks?.Add(_0003t); 
	                } 
	            }); 
	        } 
	        #endregion rg.SimpleRedux.@sstobv
	        #region rg.SimpleRedux.AutoSubscribe
	        _0004un_0 = @subscribe("before_unlock_ok", delegate((bool is_first_unlock, string new_conn, string old_conn) _0004pi0)
	        { 
	            at_before_unlock_ok(_0004pi0);
	            return null;
	        }); 
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }

	    protected virtual void __cleanup__()
	    {
			var _ic_ = Interlocked.CompareExchange(ref __iC__, 0, 1);
			if (_ic_ != 1) return;

	        #region __undefined__
	        #endregion __undefined__
	        #region rg.SimpleRedux.@sstobv
	        _0003un_0?.Invoke();
	        _0003un_0 = null;
	        _0003un_1?.Invoke();
	        _0003un_1 = null;
	        #endregion rg.SimpleRedux.@sstobv
	        #region rg.SimpleRedux.AutoSubscribe
	        _0004un_0?.Invoke();
	        _0004un_0 = null;
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }
	}
}

