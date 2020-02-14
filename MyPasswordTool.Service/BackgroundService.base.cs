using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    public partial class BackgroundService
    {
        public void DoWork(string args) 
        {
            AsyncHelper.RunInBatch(async () =>
            {
                var id = Guid.NewGuid();
                var o = args.ToObject<BackgroundMessage>();
                try
                {
                    LogManager.GetLogger(this.GetType()).Info($"dowork start with at={o.Token} gid={id}");
                    await HandleBackgroundMessageWithResultAsync(o);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(this.GetType()).Error(ex);
                    throw ex;
                }
                finally
                {
                    LogManager.GetLogger(this.GetType()).Info($"dowork end {id}");
                }
            });
        }

        public async Task<string> DoWorkWithResultAsync(string args)
        {
            var id = Guid.NewGuid();
            var o = args.ToObject<BackgroundMessage>();
            try
            {
                LogManager.GetLogger(this.GetType()).Info($"dowork start with at={o.Token} gid={id}");
                var r = HandleBackgroundMessageWithResultAsync(o);
                return (await r).ToJson();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex);
                throw ex;
            }
            finally
            {
                LogManager.GetLogger(this.GetType()).Info($"dowork end {id}");
            }
        }
    }
}
