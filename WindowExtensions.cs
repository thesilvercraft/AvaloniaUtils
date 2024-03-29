﻿using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;

namespace SilverCraft.AvaloniaUtils
{
    public record class StylingChangeData(bool? IsTransparent,string? SAPColor, WindowTransparencyLevel[]? SAPTransparency);
    public static class WindowExtensions
    {
        public static IEnvBackend envBackend = new ModernDotFileBackend(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SilverCraftAvaloniav1Shared",
            "dotfile.json"));

        public static EventHandler<StylingChangeData> OnStyleChange;
        static bool DisSAPTransparency => envBackend.GetBool("DisableSAPTransparency") == true;

        public static Color ReadColor(this string varname, Color? def = null)
        {
            var color = envBackend.GetString(varname);
            if (color == null) return def ?? Colors.Coral;
            if (Color.TryParse(color, out var c)) return c;
            if (Enum.TryParse(color, out KnownColor kc))
            {
                c = kc.ToColor();
            }
            else
            {
                return def ?? Colors.Coral;
            }
            return c;

        }
        public static Color? ParseColor(string value)
        {
            if (Color.TryParse(value, out var c)) return c;
            if (Enum.TryParse(value, out KnownColor kc))
            {
                return kc.ToColor();
            }
            else
            {
                return null;
            }
        }
        public static IBrush ParseBackground(this string? color, IBrush? def = null)
        {
            if (color == null) return def ?? new SolidColorBrush(Colors.Coral, DisSAPTransparency ? 1 : 0.3);
            if (color.Contains(','))
            {
                var colors = color.Split(',');
                var perpart = 1d / colors.Length;
                double alreadygiven = 0;

                var gradient = new LinearGradientBrush();

                foreach (var c in colors)
                {
                    var cc = ParseColor(c);
                    if (cc == null) continue;
                    gradient.GradientStops.Add(new((Color)cc, alreadygiven));
                    alreadygiven += perpart;
                }
                return gradient;
            }
            var co = ParseColor(color);
            return co ==null? (def ?? new SolidColorBrush(Colors.Coral, DisSAPTransparency ? 1 : 0.3)) :new SolidColorBrush((Color)co, envBackend.GetBool("DisableSAPTransparency") == true ? 1 : 0.3);

        }
        public static Color ToColor(this KnownColor kc) => Color.FromUInt32((uint)kc);

        public static WindowTransparencyLevel GetTransparencyLevelFromString(string s) =>
            s switch
            {   
                "AcrylicBlur" => WindowTransparencyLevel.AcrylicBlur,
                "Transparent"  => WindowTransparencyLevel.Transparent,
                "Mica"  => WindowTransparencyLevel.Mica,
                _=> WindowTransparencyLevel.AcrylicBlur
            };

        public static WindowTransparencyLevel[] GetTransparencyLevelsFromString(string s)
        {
            return s.Split(',').Select(GetTransparencyLevelFromString).ToArray();
        }
        public static void DoAfterInitTasks(this Window w, bool firstrun, IBrush? def = null)
        {

            w.TransparencyLevelHint =
                GetTransparencyLevelsFromString(envBackend.GetString("SAPTransparency") ?? "Mica,AcrylicBlur,None");
            w.Background = ParseBackground(envBackend.GetString("SAPColor"), def: def);
            if (!firstrun) return;
            EventHandler<StylingChangeData> x = (_, y) =>
            {
                if(y!=null)
                {
                    if (y.SAPTransparency != null)
                    {
                        Dispatcher.UIThread.InvokeAsync(() => w.TransparencyLevelHint = y.SAPTransparency);
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