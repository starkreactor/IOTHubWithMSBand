using System;
using Xamarin.Forms;

namespace BandSpy
{
	public class IconFontLabel : Label
	{
		public IconFontLabel()
		{
			//We need to load the font family for iOS by specifying it here
			FontFamily = "FontAwesome";
			FontSize = 35;
            HorizontalOptions = LayoutOptions.Center;
			//For android, we set it in the custom rendered in the Droid project under the IconFontLabelRenderer
		}
        public static readonly BindableProperty IconSizeProperty = BindableProperty.Create<IconFontLabel, Object>(
                                                                   p => p.IconSize,
                                                               null,
                                                               propertyChanged: (bindable,
                                                                                 oldValue,
                                                                                 newValue) => ((IconFontLabel)bindable).SetIconSize(newValue)
                                                           );

        /// <summary>
        /// This is the UTF-8 integer or hex representation of the character in the cc-fonts-4.ttf file in the assets folder for Android or the resources folder for iOS e.g. 0xE029 or E029
        /// </summary>
        public Object IconSize
        {
            get { return (Object)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public void SetIconSize(object o)
        {
            if (o is String)
            {
                FontSize = Convert.ToDouble(o);
            }

            if (o is Int32)
            {
                FontSize = Convert.ToDouble(o);
            }
        }

        public static readonly BindableProperty IconFontProperty = BindableProperty.Create<IconFontLabel, Object>(
                                                                           p => p.IconFont,
                                                                       null,
                                                                       propertyChanged: (bindable,
                                                                                         oldValue,
                                                                                         newValue) => ((IconFontLabel)bindable).SetIconFontText(newValue)
                                                                   );

        /// <summary>
        /// This is the UTF-8 integer or hex representation of the character in the cc-fonts-4.ttf file in the assets folder for Android or the resources folder for iOS e.g. 0xE029 or E029
        /// </summary>
        public Object IconFont
        {
            get { return (Object)GetValue(IconFontProperty); }
            set { SetValue(IconFontProperty, value); }
        }

        public void SetIconFontText(object o)
        {
            if (o is String)
            {
                var utf8_ref = Int32.Parse((String)o, System.Globalization.NumberStyles.HexNumber);
                Text = Char.ConvertFromUtf32(utf8_ref);
            }

            if (o is Int32)
            {
                Text = Char.ConvertFromUtf32((Int32)o);
            }
        }
    }
}
