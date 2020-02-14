using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyPasswordTool.Models
{
    public class AppState
    {
        public JToken JX { get; set; }

        public DbConnInfo DbConnInfo { get; set; }

        //tmp

        public string prevdb { get; set; }
        
        public (object Action, IList<Task> Tasks) ActionContext
        {
            get
            {
                var c = ActionContexts;
                if (c.Count == 0) return default;
                return c.Peek();
            }
        }

        private ThreadLocal<Stack<(object, IList<Task>)>> _actionContexts = new ThreadLocal<Stack<(object, IList<Task>)>>();
        public Stack<(object Action, IList<Task> Tasks)> ActionContexts
        {
            get
            {
                var actions = _actionContexts.Value;
                if (actions == null) _actionContexts.Value = actions = new Stack<(object, IList<Task>)>();
                return actions;
            }
        }

        public void TryPopActionContext()
        {
            var actions = _actionContexts.Value;
            if (actions?.Count > 0) actions.Pop();
            if (actions?.Count == 0) _actionContexts.Value = null;
        }

        public object Action => ActionContext.Action;
        public IList<Task> Tasks => ActionContext.Tasks;
    }
}