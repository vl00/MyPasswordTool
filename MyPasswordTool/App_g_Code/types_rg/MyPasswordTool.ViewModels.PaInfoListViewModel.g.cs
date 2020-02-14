//2019/10/7 12:14:06

using Common;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.ViewModels
{
	partial class PaInfoListViewModel : __IObj__
	{
		int __iC__;

	    #region __undefined__
	    #endregion __undefined__
	    #region rg.SimpleRedux.AutoSubscribe
	    SimpleRedux.UnSubscribe _0004un_0;
	    
	    SimpleRedux.UnSubscribe _0004un_1;
	    
	    SimpleRedux.UnSubscribe _0004un_2;
	    
	    SimpleRedux.UnSubscribe _0004un_3;
	    
	    SimpleRedux.UnSubscribe _0004un_4;
	    
	    SimpleRedux.UnSubscribe _0004un_5;
	    
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
	        _0004un_2 = @subscribe(delegate(MyPasswordTool.Models.FindPasMessage _0004pi2)
	        { 
	            OnHandleMessage(_0004pi2);
	            return null;
	        }); 
	        _0004un_3 = @subscribe(delegate(MyPasswordTool.Models.RefreshPaInfosMessage _0004pi3)
	        { 
	            OnHandleMessage(_0004pi3);
	            return null;
	        }); 
	        _0004un_4 = @subscribe("palsOrderChanged", delegate(string _0004pi4)
	        { 
	            OnHandleMessage(_0004pi4);
	            return null;
	        }); 
	        _0004un_5 = @subscribe(delegate(MyPasswordTool.Models.DropPaInfoToTagMessage _0004pi5)
	        { 
	            OnHandleMessage(_0004pi5);
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
	        _0004un_3?.Invoke();
	        _0004un_3 = null;
	        _0004un_4?.Invoke();
	        _0004un_4 = null;
	        _0004un_5?.Invoke();
	        _0004un_5 = null;
	        #endregion rg.SimpleRedux.AutoSubscribe
	        
	    }
	}
}

