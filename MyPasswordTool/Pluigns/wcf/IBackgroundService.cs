using System.ServiceModel;
using System.Threading.Tasks;

namespace MyPasswordTool.WCFService
{
    [ServiceContract]
    public interface IBackgroundService
    {
        [OperationContract(IsOneWay = true, Action = "http://tempuri.org/IBackgroundService/DoWork")]
        //[OperationContract(IsOneWay = true)]
        void DoWork(string args);

        [OperationContract(Action = "http://tempuri.org/IBackgroundService/DoWorkWithResult", ReplyAction = "http://tempuri.org/IBackgroundService/DoWorkWithResultResponse")]
        //[OperationContract]
        Task<string> DoWorkWithResultAsync(string args);
    }
}
