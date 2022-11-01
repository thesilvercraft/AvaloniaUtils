using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace SilverAudioPlayer.Avalonia
{
    public static class WindowExtensions
    {
        public static string? GetEnv(this string EnvvarName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Environment.GetEnvironmentVariable(EnvvarName, EnvironmentVariableTarget.User);
            }
            return Environment.GetEnvironmentVariable(EnvvarName);
        }
        public static T? GetEnv<T>(this string EnvvarName) where T : struct
        {
            if (Enum.TryParse(GetEnv(EnvvarName), out T value2))
            {
                return value2;
            }
            return null;
        }
        public static void SetEnv(this string EnvvarName, string? Value)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable(EnvvarName, Value, EnvironmentVariableTarget.User);
            }
            else
            {
                Environment.SetEnvironmentVariable(EnvvarName, Value);
            }
        }
        public static EventHandler<Tuple<bool, WindowTransparencyLevel, Color>> OnStyleChange;
        public static Color ReadColor(this string varname, Color? def = null)
        {
            var color = GetEnv(varname);
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
        public static IBrush ReadBackground(this string varname, IBrush? def = null)
        {
            var color = GetEnv(varname);
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
                return new SolidColorBrush(ParseColor(color) ?? Colors.Coral, GetEnv("DisableSAPTransparency") == "true" ? 1 : 0.3);
            }
            return def ?? new SolidColorBrush(Colors.Coral, GetEnv("DisableSAPTransparency") == "true" ? 1 : 0.3);

        }
        public static Color ToColor(this KnownColor kc)
        {
            return Color.FromUInt32((uint)kc);
        }
        public static void DoAfterInitTasks(this Window w, bool firstrun, IBrush? def = null)
        {
            w.TransparencyLevelHint = GetEnv<WindowTransparencyLevel>("SAPTransparency") ?? WindowTransparencyLevel.AcrylicBlur;
            if (firstrun)
            {
                EventHandler<Tuple<bool, WindowTransparencyLevel, Color>> x = (_, _) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() => w.DoAfterInitTasks(false, def: def));
                };
                OnStyleChange += x;
                w.Closing += (_, _) => { if (OnStyleChange != null) { OnStyleChange -= x; } };
            }
            w.Background = ReadBackground("SAPColor", def: def);
        }

    }
}