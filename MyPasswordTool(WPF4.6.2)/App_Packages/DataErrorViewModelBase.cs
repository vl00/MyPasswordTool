using SilverEx;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace MyPasswordTool
{
    public abstract class DataErrorViewModelBase : NotifyObject, IClear, IDataErrorInfo
    {
        private Dictionary<string, string> __errors { get; } = new Dictionary<string, string>();

        public void SetDataError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearDataError(propertyName);
                return;
            }
            __errors[propertyName] = error;
            this.RaisePropertyChanged(propertyName);
        }

        public void ClearDataError(string propertyName)
        {
            if (__errors.Remove(propertyName))
                this.RaisePropertyChanged(propertyName);
        }

        public virtual void Clear() { }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string o;
                __errors.TryGetValue(propertyName, out o);
                return o;
            }
        }
    }
}
