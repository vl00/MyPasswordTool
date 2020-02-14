namespace Common
{
    //public interface IView { }

    public interface IView<TViewModel> //: IView
    {
        TViewModel ViewModel { get; }
    }
}