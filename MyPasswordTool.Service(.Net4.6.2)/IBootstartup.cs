using System;

namespace MyPasswordTool.Service
{
    public interface IBootstartup : IDisposable
    {
        void Init();
    }
}