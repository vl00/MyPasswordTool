//2020/1/4 14:27:34

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
	partial class TagTreeViewModel : __IObj__
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
	        _0004un_0 = @subscribe("TagTreeViewModel__AddNewPaInfo", delegate((int id, bool select) _0004pi0)
	        { 
	            on_TagTreeViewModel__AddNewPaInfo(_0004pi0);
	            return null;
	        }); 
	        _0004un_1 = @subscribe("on_searchtext", delegate(string _0004pi1)
	        { 
	            on_searchtext(_0004pi1);
	            return null;
	        }); 
	        _0004un_2 = @subscribe("main_Clear", delegate(object _0004pi2)
	        { 
	            Clear(_0004pi2);
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
	        _0004un_1?.Invoke();
	        _0004un_1 = null;
	        _0004un_2?.Invoke();
	        _0004un_2 = null;
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }
	}
}

