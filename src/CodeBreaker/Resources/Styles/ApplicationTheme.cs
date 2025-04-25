using MauiReactor;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace CodeBreaker.Resources.Styles;

class ApplicationTheme : Theme
{
    public static Color Primary { get; } = Color.FromRgba(81, 43, 212, 255); // #512BD4
    public static Color PrimaryDark { get; } = Color.FromRgba(172, 153, 234, 255); // #AC99EA
    public static Color PrimaryDarkText { get; } = Color.FromRgba(36, 36, 36, 255); // #242424
    public static Color Secondary { get; } = Color.FromRgba(223, 216, 247, 255); // #DFD8F7
    public static Color SecondaryDarkText { get; } = Color.FromRgba(152, 128, 229, 255); // #9880E5
    public static Color Tertiary { get; } = Color.FromRgba(43, 11, 152, 255); // #2B0B98

    public static Color White { get; } = Colors.White; // #FFFFFF
    public static Color Black { get; } = Colors.Black; // #000000
    public static Color Magenta { get; } = Color.FromRgb(0xB1, 0x0D, 0xC9); // #B10DC9
    public static Color MidnightBlue { get; } = Color.FromRgba(25, 6, 73, 255); // #190649
    public static Color OffBlack { get; } = Color.FromRgb(0x11, 0x11, 0x11); // #111111
    public static Color OffWhite { get; } = Color.FromRgba(241, 241, 241, 255); // #F1F1F1

    public static Color Gray100 { get; } = Color.FromRgba(225, 225, 225, 255); // #E1E1E1
    public static Color Gray200 { get; } = Color.FromRgba(200, 200, 200, 255); // #C8C8C8
    public static Color Gray300 { get; } = Color.FromRgba(172, 172, 172, 255); // #ACACAC
    public static Color Gray400 { get; } = Color.FromRgba(145, 145, 145, 255); // #919191
    public static Color Gray500 { get; } = Color.FromRgba(110, 110, 110, 255); // #6E6E6E
    public static Color Gray600 { get; } = Color.FromRgba(64, 64, 64, 255); // #404040
    public static Color Gray900 { get; } = Color.FromRgba(33, 33, 33, 255); // #212121
    public static Color Gray950 { get; } = Color.FromRgba(20, 20, 20, 255); // #141414

    // Game-specific colors
    public static Color GameRed { get; } = Color.FromRgb(0xFF, 0x41, 0x36); // #FF4136
    public static Color GameGreen { get; } = Color.FromRgb(0x2E, 0xCC, 0x40); // #2ECC40
    public static Color GameBlue { get; } = Color.FromRgb(0x00, 0x74, 0xD9); // #0074D9
    public static Color GameYellow { get; } = Color.FromRgb(0xFF, 0xDC, 0x00); // #FFDC00
    public static Color GameCyan { get; } = Color.FromRgb(0x7F, 0xDB, 0xFF); // #7FDBFF
    public static Color GameAmber { get; } = Color.FromRgb(0xCC, 0xAA, 0x33); // #CCAA33
    public static Color GameDarkRed { get; } = Color.FromRgb(0x80, 0x00, 0x00); // #800000

    // Visual effects colors
    public static Color ActiveRowHighlight { get; } = Color.FromRgba(127, 219, 255, 25); // rgba(127,219,255,0.1)

    // Icons
    public static ImageSource IconKey = new FontImageSource
    {
        FontFamily = Fonts.FluentUI.FontFamily,
        Glyph = Fonts.FluentUI.key_16_regular,
        Color = White
    };

    public static ImageSource IconKeyDisabled = new FontImageSource
    {
        FontFamily = Fonts.FluentUI.FontFamily,
        Glyph = Fonts.FluentUI.key_16_regular,
        Color = Gray950
    };

    public static ImageSource IconEraser = new FontImageSource
    {
        FontFamily = Fonts.FluentUI.FontFamily,
        Glyph = Fonts.FluentUI.eraser_20_regular,
        Color = White
    };

    

    protected override void OnApply()
    {

    }
}
