//2019/10/7 12:14:06

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;
using MyPasswordTool.Models;
using SilverEx;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static rg.SimpleRedux;

namespace MyPasswordTool
{
	partial class MainWindow : __IObj__
	{
		int __iC__;

	    #region __undefined__
	    #endregion __undefined__
	    #region rg.SimpleRedux.@sstobv
	    static partial void @sstobv<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg); 
	    static partial void @sstobv_<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg); 
	    
	    SimpleRedux.UnSubscribe _0003un_0;
	    
	    MyPasswordTool.Models.NavigationMessage _0003on_ChildWin_f7be6025(MyPasswordTool.Models.AppState _){return _.Action as NavigationMessage;}
	    #endregion rg.SimpleRedux.@sstobv
	    
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
	            var _0003ov__0003on_ChildWin_f7be6025 = default(MyPasswordTool.Models.NavigationMessage); 
	            _0003un_0 = @store.Subscribe(() => 
	            { 
	                var _0003nv__0003on_ChildWin_f7be6025 = _0003on_ChildWin_f7be6025(@store.GetState()); 
	                var _0003b = false; 
	                if (!(ReferenceEquals(_0003nv__0003on_ChildWin_f7be6025, _0003ov__0003on_ChildWin_f7be6025) || Equals(_0003nv__0003on_ChildWin_f7be6025, _0003ov__0003on_ChildWin_f7be6025))) 
	                { 
	                      _0003b = true; 
	                } 
	                if (_0003b)
	                { 
	                    on_ChildWin(msg: _0003nv__0003on_ChildWin_f7be6025); 
	                } 
	            }); 
	        } 
	        #endregion rg.SimpleRedux.@sstobv
	        
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
	        #endregion rg.SimpleRedux.@sstobv
	        
	    }
	}
}

