using System;
using Xamarin.Forms;

// https://stackoverflow.com/questions/30772510/how-to-add-checkbox-in-xamarin-forms-in-xaml-file

namespace VerdeNFC.Resources.Controls
{

    public class Checkbox : Button
    {
        public Checkbox()
        {
            base.Image = "Image_Unchecked.png";
            base.Clicked += new EventHandler(OnClicked);
            base.SizeChanged += new EventHandler(OnSizeChanged);
            base.BackgroundColor = Color.Transparent;
            base.BorderWidth = 0;
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
            declaringType: typeof(Checkbox),
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

        private static void CheckedValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null && (Boolean)newValue == true)
            {
                ((Checkbox)bindable).Image = "Image_Checked.png";
            }
            else
            {
                ((Checkbox)bindable).Image = "Image_Unchecked.png";
            }
        }

        public event EventHandler CheckedChanged;
        private void RaiseCheckedChanged()
        {
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

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
