using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    [ServiceContract]
    public interface IBackgroundService
    {
        [OperationContract(IsOneWay = true)]
        void DoWork(string args);

        [OperationContract]
        Task<string> DoWorkWithResultAsync(string args);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class BackgroundService : IBackgroundService
    {
    }
}