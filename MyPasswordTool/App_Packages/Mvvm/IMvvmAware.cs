using System;

namespace Common
{
    public interface IMvvmAware
    {
        void Activate(object parameter);
        void Deactivate(object parameter);
    }
}