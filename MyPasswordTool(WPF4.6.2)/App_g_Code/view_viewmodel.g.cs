///
/// auto generated with [Map_View_ViewModel] and IView<>
/// on 2020-01-05 14:45:50.8796
///

using System;
using System.Collections.Generic;

namespace MyPasswordTool
{
	partial class MyBootstrapper
	{
		partial void map_view_viewmodel(IDictionary<Type, Type> mapper)
		{
			mapper.Add(typeof(MyPasswordTool.HeaderView), 
			           typeof(MyPasswordTool.ViewModels.MainViewModel));

			mapper.Add(typeof(MyPasswordTool.MainPage), 
			           typeof(MyPasswordTool.ViewModels.MainViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PageDuc0), 
			           typeof(MyPasswordTool.ViewModels.Duc0ViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PageDuc1), 
			           typeof(MyPasswordTool.ViewModels.Duc1ViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PageEdit0), 
			           typeof(MyPasswordTool.ViewModels.Edit0ViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PageEdit1), 
			           typeof(MyPasswordTool.ViewModels.Edit1ViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PaInfoList), 
			           typeof(MyPasswordTool.ViewModels.PaInfoListViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PalockView), 
			           typeof(MyPasswordTool.ViewModels.PalockViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.PaTagsWin), 
			           typeof(MyPasswordTool.ViewModels.PainfoTagsWinViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.RenameWin), 
			           typeof(MyPasswordTool.ViewModels.RenameViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.TrashDisplayPaUC), 
			           typeof(MyPasswordTool.ViewModels.DucTrashViewModel));

			mapper.Add(typeof(MyPasswordTool.Views.TreepanelView), 
			           typeof(MyPasswordTool.ViewModels.TagTreeViewModel));

		}
	}
}
