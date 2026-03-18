using cAlgo.API;
using cAlgo.API.Indicators;
using static cAlgo.FreeVolumeProfileV20;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    // ========= THEME =========
    public static class Styles
    {
        public static readonly Color ColorDarkTheme_Panel = GetColorWithOpacity(Color.FromHex("#292929"), 0.85);
        public static readonly Color ColorLightTheme_Panel = GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.85);

        public static readonly Color ColorDarkTheme_PanelBorder = Color.FromHex("#3C3C3C");
        public static readonly Color ColorLightTheme_PanelBorder = Color.FromHex("#C3C3C3");

        public static readonly Color ColorDarkTheme_CommonBorder = GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.12);
        public static readonly Color ColorLightTheme_CommonBorder = GetColorWithOpacity(Color.FromHex("#000000"), 0.12);

        public static readonly Color ColorDarkTheme_Header = GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.70);
        public static readonly Color ColorLightTheme_Header = GetColorWithOpacity(Color.FromHex("#000000"), 0.65);

        public static readonly Color ColorDarkTheme_Input = Color.FromHex("#1A1A1A");
        public static readonly Color ColorDarkTheme_InputHover = Color.FromHex("#111111");
        public static readonly Color ColorLightTheme_Input = Color.FromHex("#E7EBED");
        public static readonly Color ColorLightTheme_InputHover = Color.FromHex("#D6DADC");

        public static readonly Color ColorDarkTheme_ButtonHover = Color.FromHex("#444444");

        public static Style CreatePanelBackgroundStyle()
        {
            Style style = new();
            style.Set(ControlProperty.CornerRadius, 3);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_Panel, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_Panel, ControlState.LightTheme);
            style.Set(ControlProperty.BorderColor, ColorDarkTheme_PanelBorder, ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, ColorLightTheme_PanelBorder, ControlState.LightTheme);
            style.Set(ControlProperty.BorderThickness, new Thickness(1));

            return style;
        }
        public static Style CreateButtonStyle()
        {
            Style style = new(DefaultStyles.TextBoxStyle);
            style.Set(ControlProperty.CornerRadius, 3);

            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_PanelBorder, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_ButtonHover, ControlState.DarkTheme | ControlState.Hover);

            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_Input, ControlState.LightTheme);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_InputHover, ControlState.LightTheme | ControlState.Hover);

            style.Set(ControlProperty.BorderColor, ColorDarkTheme_PanelBorder, ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, ColorLightTheme_PanelBorder, ControlState.LightTheme);
            style.Set(ControlProperty.BorderThickness, new Thickness(1));

            return style;
        }
        public static Style CreateCommonBorderStyle()
        {
            Style style = new();
            style.Set(ControlProperty.BorderColor, ColorDarkTheme_CommonBorder, ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, ColorLightTheme_CommonBorder, ControlState.LightTheme);
            return style;
        }
        public static Style CreateHeaderStyle()
        {
            Style style = new();
            style.Set(ControlProperty.ForegroundColor, ColorDarkTheme_Header, ControlState.DarkTheme);
            style.Set(ControlProperty.ForegroundColor, ColorLightTheme_Header, ControlState.LightTheme);
            return style;
        }
        public static Style CreateInputStyle()
        {
            Style style = new(DefaultStyles.TextBoxStyle);
            style.Set(ControlProperty.CornerRadius, 3);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_Input, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_InputHover, ControlState.DarkTheme | ControlState.Hover);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_Input, ControlState.LightTheme);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_InputHover, ControlState.LightTheme | ControlState.Hover);
            return style;
        }
        public static Style CreateComboBoxStyle()
        {
            Style style = new(DefaultStyles.TextBoxStyle);
            style.Set(ControlProperty.CornerRadius, 3);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_Input, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, ColorDarkTheme_InputHover, ControlState.DarkTheme | ControlState.Hover);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_Input, ControlState.LightTheme);
            style.Set(ControlProperty.BackgroundColor, ColorLightTheme_InputHover, ControlState.LightTheme | ControlState.Hover);
            return style;
        }
        public static Style CreateScrollViewerTransparentStyle()
        {
            var style = new Style();

            style.Set(ControlProperty.BackgroundColor, Color.Transparent, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, Color.Transparent, ControlState.LightTheme);

            style.Set(ControlProperty.BorderColor, Color.Transparent, ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, Color.Transparent, ControlState.LightTheme);

            style.Set(ControlProperty.BorderThickness, new Thickness(0));
            style.Set(ControlProperty.CornerRadius, 0);
            style.Set(ControlProperty.Padding, new Thickness(0));
            style.Set(ControlProperty.Margin, new Thickness(0));

            return style;
        }
        private static Color GetColorWithOpacity(Color baseColor, double opacity)
        {
            if (opacity < 0.0 || opacity > 1.0)
                throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0.0 and 1.0");

            byte alpha = (byte)Math.Round(255 * opacity, MidpointRounding.AwayFromZero);
            return Color.FromArgb(alpha, baseColor);
        }
    }


}
