using System;
using Xamarin.Forms;

// https://stackoverflow.com/questions/30772510/how-to-add-CheckButton-in-xamarin-forms-in-xaml-file

namespace VerdeNFC.Resources.Controls
{

    public class CheckButton : Button
    {
        public CheckButton()
        {
            base.Clicked += new EventHandler(OnClicked);
            base.SizeChanged += new EventHandler(OnSizeChanged);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            //if (base.Height > 0)
            //{
            //    base.WidthRequest = base.Height;
            //}
        }

        public static BindableProperty CheckedProperty = BindableProperty.Create(
            propertyName: "Checked",
            returnType: typeof(Boolean?),
            declaringType: typeof(CheckButton),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: CheckedValueChanged);

        public Boolean? Checked
        {
            get
            {
                if (GetValue(CheckedProperty) == null)
                {
                    return null;
                }
                return (Boolean)GetValue(CheckedProperty);
            }
            set
            {
                SetValue(CheckedProperty, value);
                OnPropertyChanged();
                RaiseCheckedChanged();
            }
        }

        public string CheckedText { get; set; }

        public string UncheckedText { get; set; }

        private static void CheckedValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null && (Boolean)newValue == true)
            {
                ((CheckButton)bindable).Text = (string)((CheckButton)bindable).CheckedText;
            }
            else
            {
                ((CheckButton)bindable).Text = (string)((CheckButton)bindable).UncheckedText;
            }
        }

        public event EventHandler CheckedChanged;
        private void RaiseCheckedChanged()
        {
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
        /*
        private Boolean _IsEnabled = true;
        public new Boolean IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                _IsEnabled = value;
                OnPropertyChanged();
                if (value == true)
                {
                    this.Opacity = 1;
                }
                else
                {
                    this.Opacity = .5;
                }
                base.IsEnabled = value;
            }
        }
        */
        public void OnEnabled_Changed()
        {

        }

        public void OnClicked(object sender, EventArgs e)
        {
            Checked = !Checked;

            // Call the base class event invocation method.
            //base.Clicked(sender, e);
        }

    }
}
