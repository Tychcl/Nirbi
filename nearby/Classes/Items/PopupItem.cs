using System.Windows.Input;

namespace nearby.Classes
{
    public class PopupItem : BaseItem
    {
        public ICommand Command;

        public PopupItem() { }

        public PopupItem(string icon, string text, ICommand command) : base(icon, text) { Command = command; }
    }

    public class PopupItem<T> : PopupItem
    {
        public T? Parameter;

        public PopupItem() { }

        public PopupItem(string icon, string text, ICommand command, T? parameter) : base(icon, text, command) { Parameter = parameter; }
    }
}
