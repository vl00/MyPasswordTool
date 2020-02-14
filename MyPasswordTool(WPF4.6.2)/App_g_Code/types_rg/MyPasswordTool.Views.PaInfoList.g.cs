//2020/1/4 14:27:34

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.Views
{
	partial class PaInfoList : __IObj__
	{
		int __iC__;

	    #region __undefined__
	    #endregion __undefined__
	    #region rg.SimpleRedux.AutoSubscribe
	    SimpleRedux.UnSubscribe _0004un_0;
	    
	    SimpleRedux.UnSubscribe _0004un_1;
	    
	    SimpleRedux.UnSubscribe _0004un_2;
	    
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
	        _0004un_0 = @subscribe("main_Activate", delegate(object _0004pi0)
	        { 
	            Activate(_0004pi0);
	            return null;
	        }); 
	        _0004un_1 = @subscribe("main_Clear", delegate(object _0004pi1)
	        { 
	            Clear(_0004pi1);
	            return null;
	        }); 
	        _0004un_2 = @subscribe(delegate(MyPasswordTool.Models.PaListScrollData _0004pi2)
	        { 
	            return when_voff_noteq_svvoff(_0004pi2);
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
	        _0004un_1?.Invoke();
	        _0004un_1 = null;
	        _0004un_2?.Invoke();
	        _0004un_2 = null;
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }
	}
}

