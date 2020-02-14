using System;
using System.Dynamic;

namespace SilverEx
{
	public abstract partial class DynamicBag : DynamicObject
	{
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this.Get(binder.Name);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			this.Set(binder.Name, value);
			return true;
		}
	}
}