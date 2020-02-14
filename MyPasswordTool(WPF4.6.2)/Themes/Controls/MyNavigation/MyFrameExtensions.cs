using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public static class MyFrameExtensions
    {
        public static async Task<bool> GoBackAsync(this MyFrame frame, object parameter)
        {
            var i = frame.Journals.BackStack.Count - 1;
            if (i < 0) return false;
            var j = frame.Journals.BackStack[i];
            frame.Journals.BackStack[i] = new JournalEntry(j.PageType, parameter);
            return await frame.GoBackAsync();
        }

        public static async Task<bool> GoForwardAsync(this MyFrame frame, object parameter)
        {
            if (frame.Journals.ForwardStack.Count <= 0) return false;
            var j = frame.Journals.ForwardStack[0];
            frame.Journals.ForwardStack[0] = new JournalEntry(j.PageType, parameter);
            return await frame.GoForwardAsync();
        }
    }
}