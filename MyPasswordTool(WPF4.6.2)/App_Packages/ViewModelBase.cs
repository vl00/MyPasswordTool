
namespace SilverEx
{
    public interface IClear
    {
        void Clear();
    }

    public abstract class ViewModelBase : NotifyObject, IClear
    {
        private bool _isinit;

        protected virtual void OnInit() { }
        public virtual void Clear() { }

        public void Init()
        {
            if (_isinit) return;
            _isinit = true;
            OnInit();
        }
    }
}