using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;

namespace SilverEx.Xaml
{
    [ContentProperty("CommandParameter")]
    public sealed class EventToDelegateCommand : TriggerAction<FrameworkElement>
    {
        #region CommandParameter

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof (object), typeof (EventToDelegateCommand),
                                        new PropertyMetadata(OnCommandParameterPropertyChanged));

        private static void OnCommandParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as EventToDelegateCommand;
            if (o != null && o.AssociatedObject != null && e.OldValue != e.NewValue)
                o.EnableOrDisableElementByChanged();
        }

        [Category("Common Properties")]
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof (ICommand), typeof (EventToDelegateCommand),
                                        new PropertyMetadata(null, OnCommandPropertyChanged));

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as EventToDelegateCommand;
            if (e.OldValue != null) 
            {
                (e.OldValue as ICommand).CanExecuteChanged -= o.OnCommandCanExecuteChanged;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as ICommand).CanExecuteChanged += o.OnCommandCanExecuteChanged;
            }
            o.EnableOrDisableElementByChanged();
        }

        [Category("Common Properties")]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #region MustToggleIsEnabled

        public static readonly DependencyProperty MustToggleIsEnabledProperty =
            DependencyProperty.Register("MustToggleIsEnabled", typeof(bool), typeof(EventToDelegateCommand),
                                        new PropertyMetadata(false, OnMustToggleIsEnabledPropertyChanged));

        private static void OnMustToggleIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as EventToDelegateCommand;
            if (o != null && o.AssociatedObject != null && e.OldValue != e.NewValue)
                o.EnableOrDisableElementByChanged();
        }

        public bool MustToggleIsEnabled
        {
            get { return (bool) GetValue(MustToggleIsEnabledProperty); }
            set { SetValue(MustToggleIsEnabledProperty, value); }
        }

        #endregion

        #region IsDetachOnAssociatedObjectUnloaded

        public static readonly DependencyProperty IsDetachOnAssociatedObjectUnloadedProperty =
            DependencyProperty.Register("IsDetachOnAssociatedObjectUnloaded", typeof(bool), typeof(EventToDelegateCommand),
                                        new PropertyMetadata(true, null));

        public bool IsDetachOnAssociatedObjectUnloaded
        {
            get { return (bool)GetValue(IsDetachOnAssociatedObjectUnloadedProperty); }
            set { SetValue(IsDetachOnAssociatedObjectUnloadedProperty, value); }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            if (IsDetachOnAssociatedObjectUnloaded) AssociatedObject.Unloaded += _AssociatedObject_Unloaded;
            EnableOrDisableElementByChanged();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            SetValue(CommandProperty, null);
        }

        protected override void Invoke(object parameter)
        {
            if (!AssociatedElementIsDisabled())
            {
                var command = this.GetCommand();
                if (command != null)
                {
                    var e = new EventParamter { Sender = AssociatedObject, EventArgs = parameter, CommandParameter = this.CommandParameter };
                    if (command.CanExecute(e))
                        command.Execute(e);
                }
            }
        }

        private ICommand GetCommand()
        {
            return this.Command;
        }

        private bool AssociatedElementIsDisabled()
        {
#if SILVERLIGHT
            var element = this.AssociatedObject as Control;
#else
            var element = this.AssociatedObject as UIElement;
#endif
            return ((this.AssociatedObject == null) || ((element != null) && !element.IsEnabled));
        }

        private void EnableOrDisableElementByChanged()
        {
#if SILVERLIGHT
            var element = this.AssociatedObject as Control;
#else
            var element = this.AssociatedObject as UIElement;
#endif
            if (element != null && this.MustToggleIsEnabled)
            {
                var command = this.GetCommand();
                if (command != null)
                {
                    var e = new EventParamter { Sender = AssociatedObject, EventArgs = null, CommandParameter = this.CommandParameter };
                    element.IsEnabled = command.CanExecute(e);
                }
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.EnableOrDisableElementByChanged();
        }

        private void _AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Unloaded -= _AssociatedObject_Unloaded;
            OnDetaching();
        }
    }
}