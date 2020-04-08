using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.ComponentModel;

/// <summary>
/// this is currently a failed experiment exploring using DependencyObjects in addition to
/// NotificationObjects in Helios and supporting automatic undo.
/// It is largely a failure, primarily because any bindings in dependency properties would not
/// be recognized as undo batches and therefore would create a bunch of undo steps that actually
/// should not be separated.
/// </summary>
internal class HeliosDependencyObject : DependencyObject
{
    protected static DependencyProperty RegisterProperty<T>(
        Type ownerType,
        Expression<Func<T>> expression,
        T defaultValue,
        PropertyChangedCallback propertyChangedCallback = null)
    {
        MemberExpression body = (MemberExpression) expression.Body;
        PropertyInfo pi = body.Member as PropertyInfo;

        PropertyChangedCallback handler;
        if (propertyChangedCallback != null)
            handler = (d, e) =>
            {
                UndoPropertyChangeHandler(d, e);
                propertyChangedCallback(d, e);
            };
        else
            handler = UndoPropertyChangeHandler;
        return DependencyProperty.Register(pi.Name, typeof(T), ownerType, new PropertyMetadata(defaultValue, handler));
    }

    private static void UndoPropertyChangeHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine($"undo record {e.Property.Name} change from {e.OldValue} to {e.NewValue}");
        HeliosDependencyObject sourceObject = (HeliosDependencyObject) d;
        sourceObject.HeliosParent.OnPropertyChanged(
            e.Property.Name,
            new PropertyNotificationEventArgs(sourceObject, e.Property.Name, e.OldValue, e.NewValue));
    }

    public NotificationObject HeliosParent { get; set; }
}

namespace net.derammo.Helios.SampleProgram.Sample
{
    /// <summary>
    /// dependency object and therefore does not automatically
    /// create undo items via IPropertyNotification but rather
    /// via an adapted mechanism
    /// </summary>
    internal class SampleDependencyAutoUndo : HeliosDependencyObject
    {
        public static readonly DependencyProperty SampleValueProperty = RegisterProperty(
            typeof(SampleDependencyAutoUndo),
            () => new SampleDependencyAutoUndo().SampleValue,
            1);

        public static readonly DependencyProperty SampleValue2Property = RegisterProperty(
            typeof(SampleDependencyAutoUndo),
            () => new SampleDependencyAutoUndo().SampleValue2,
            1);

        public SampleDependencyAutoUndo()
        {
            // test direct binding
            // NOTE: this shows this code is nonsense, because dependent properties would be considered
            // a separate undo batch, which makes no sense.  It seems we have no clean way of knowing which interactions
            // with our bindable properties are from UI interaction and therefore we can't really do a good job on
            // undo batching?
            // the information might be in EffectiveValueEntry but we should not access it
            // https://referencesource.microsoft.com/#windowsbase/Base/System/Windows/EffectiveValueEntry.cs

            Binding testBinding = new Binding("SampleValue");
            testBinding.Source = this;
            BindingOperations.SetBinding(this, SampleValue2Property, testBinding);
        }

        public int SampleValue
        {
            get => (int) GetValue(SampleValueProperty);
            set => SetValue(SampleValueProperty, value);
        }

        public int SampleValue2
        {
            get => (int) GetValue(SampleValue2Property);
            set => SetValue(SampleValue2Property, value);
        }
    }
}