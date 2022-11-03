using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;

namespace SilverCraft.AvaloniaUtils
{
    public record class StylingChangeData(bool? IsTransparent,string? SAPColor, WindowTransparencyLevel? SAPTransparency);
    public static class WindowExtensions
    {

        public static IEnvBackend envBackend = new ModernDotFileBackend(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SilverCraftAvaloniav1Shared","dotfile.json"));
public static EventHandler<StylingChangeData> OnStyleChange;
        static bool DisSAPTransparency => envBackend.GetBool("DisableSAPTransparency") == true;
        public static Color ReadColor(this string varname, Color? def = null)
        {
            var color = envBackend.GetString(varname);
            if (color != null)
            {
                if (!Color.TryParse(color, out Color c))
                {
                    if (Enum.TryParse(color, out KnownColor kc))
                    {
                        c = kc.ToColor();
                    }
                    else
                    {
                        return def ?? Colors.Coral;
                    }
                }
                return c;
            }
            return def ?? Colors.Coral;

        }
        public static Color? ParseColor(string value)
        {
            if (!Color.TryParse(value, out Color c))
            {
                if (Enum.TryParse(value, out KnownColor kc))
                {
                    return kc.ToColor();
                }
                else
                {
                    return null;
                }
            }
            return c;
        }
        public static IBrush ParseBackground(this string? color, IBrush? def = null)
        {
            if (color != null)
            {
                if (color.Contains(','))
                {
                    var colors = color.Split(',');
                    double perpart = 1d / colors.Length;
                    double alreadygiven = 0;

                    var gradient = new LinearGradientBrush();

                    foreach (var c in colors)
                    {
                        var cc = ParseColor(c);
                        if (cc != null)
                        {
                            gradient.GradientStops.Add(new((Color)cc, alreadygiven));
                            alreadygiven += perpart;
                        }
                    }
                    return gradient;
                }
                var co = ParseColor(color);
                return co ==null? (def ?? new SolidColorBrush(Colors.Coral, DisSAPTransparency ? 1 : 0.3)) :new SolidColorBrush((Color)co, envBackend.GetBool("DisableSAPTransparency") == true ? 1 : 0.3);
            }
            return def ?? new SolidColorBrush(Colors.Coral, DisSAPTransparency ? 1 : 0.3);

        }
        public static Color ToColor(this KnownColor kc)
        {
            return Color.FromUInt32((uint)kc);
        }
        public static void DoAfterInitTasks(this Window w, bool firstrun, IBrush? def = null)
        {
            w.TransparencyLevelHint = envBackend.GetEnum<WindowTransparencyLevel>("SAPTransparency") ?? WindowTransparencyLevel.AcrylicBlur;
            w.Background = ParseBackground(envBackend.GetString("SAPColor"), def: def);
            if (firstrun)
            {
                EventHandler<StylingChangeData> x = (_, y) =>
                {
                    if(y!=null)
                    {
                        if (y.SAPTransparency != null)
                        {
                            Dispatcher.UIThread.InvokeAsync(() => w.TransparencyLevelHint = y.SAPTransparency ?? WindowTransparencyLevel.AcrylicBlur);
                        }
                        if (y.SAPColor != null)
                        {
                            Dispatcher.UIThread.InvokeAsync(() => w.Background = ParseBackground(y.SAPColor, def));
                            return;
                        }
                        if (y.IsTransparent != null)
                        {
                            Dispatcher.UIThread.InvokeAsync(() => w.Background = ParseBackground(y.SAPColor, def));
                        }
                    }
                    else
                    {
                        Dispatcher.UIThread.InvokeAsync(() => w.DoAfterInitTasks(false, def: def));
                    }
                };
                OnStyleChange += x;
                w.Closing += (_, _) => { if (OnStyleChange != null) { OnStyleChange -= x; } };
            }
        }

    }
}