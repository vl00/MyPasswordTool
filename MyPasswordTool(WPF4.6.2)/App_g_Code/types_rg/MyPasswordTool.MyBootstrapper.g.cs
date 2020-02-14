//2020/1/4 14:27:34

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool
{
	partial class MyBootstrapper : __IObj__
	{
		int __iC__;

	    #region __undefined__
	    #endregion __undefined__
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
	        #region rg.SimpleRedux.AutoSubscribe
	        _0004un_0?.Invoke();
	        _0004un_0 = null;
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }
	}
}

