using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

internal static class Parola
{
    private static readonly string[] Groups = {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz", "0123456789",
        "!@#$%^&*()-_=+[]{}:,.?"
    };

    [STAThread]
    private static void Main()
    {
        using (var identity = WindowsIdentity.GetCurrent())
        using (var owner = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Owner")))
        {
            if (identity.User.Value != owner.ReadToEnd().Trim())
            {
                MessageBox.Show("Bu uygulama yalnızca kurulduğu Windows hesabında açılabilir.", "Parola Oluşturucu");
                return;
            }
        }

        using (var random = RandomNumberGenerator.Create())
        using (var layout = Assembly.GetExecutingAssembly().GetManifestResourceStream("Layout"))
        {
            var window = (Window)XamlReader.Load(layout);
            var password = (TextBox)window.FindName("Password");
            var slider = (Slider)window.FindName("LengthSlider");
            var length = (TextBlock)window.FindName("Length");
            var strength = (TextBlock)window.FindName("Strength");
            var badge = (Border)window.FindName("Badge");
            var status = (TextBlock)window.FindName("Status");
            var checks = new[] { (CheckBox)window.FindName("Upper"), (CheckBox)window.FindName("Lower"),
                (CheckBox)window.FindName("Digits"), (CheckBox)window.FindName("Symbols") };

            Action generate = delegate {
                var selected = new List<string>();
                for (int i = 0; i < checks.Length; i++)
                    if (checks[i].IsChecked == true) selected.Add(Groups[i]);

                int size = (int)slider.Value;
                password.Text = Generate(random, size, selected);
                password.ScrollToHome();
                length.Text = "Parola uzunluğu: " + size;
                double bits = StrengthBits(size, selected);
                strength.Text = bits >= 80 ? "Çok güçlü" : bits >= 60 ? "Güçlü" : bits >= 40 ? "Orta" : "Zayıf";
                badge.Background = new SolidColorBrush(bits >= 60 ? Color.FromRgb(220, 245, 193) :
                    bits >= 40 ? Color.FromRgb(255, 238, 179) : Color.FromRgb(255, 218, 218));
                status.Text = "";
            };

            foreach (var check in checks)
            {
                check.Checked += delegate { generate(); };
                check.Unchecked += delegate {
                    if (Array.Exists(checks, item => item.IsChecked == true)) generate();
                    else
                    {
                        check.IsChecked = true;
                        status.Text = "En az bir karakter türü seçili olmalı.";
                    }
                };
            }

            slider.ValueChanged += delegate { generate(); };
            ((Button)window.FindName("Decrease")).Click += delegate { slider.Value--; };
            ((Button)window.FindName("Increase")).Click += delegate { slider.Value++; };
            ((Button)window.FindName("Refresh")).Click += delegate { generate(); };
            ((Button)window.FindName("Copy")).Click += delegate {
                try
                {
                    Clipboard.SetText(password.Text);
                    status.Text = "Parola panoya kopyalandı.";
                }
                catch (ExternalException)
                {
                    status.Text = "Pano meşgul. Tekrar deneyin.";
                }
            };

            generate();
            new Application().Run(window);
        }
    }

    private static string Generate(RandomNumberGenerator random, int length, List<string> groups)
    {
        string pool = string.Concat(groups);
        var result = new char[length];
        var sample = new byte[1];
        int limit = 256 - 256 % pool.Length;
        bool complete;
        do
        {
            // Rejection sampling keeps every valid password equally likely.
            for (int i = 0; i < length; i++)
            {
                do { random.GetBytes(sample); } while (sample[0] >= limit);
                result[i] = pool[sample[0] % pool.Length];
            }
            complete = groups.TrueForAll(group => Array.Exists(result, character => group.IndexOf(character) >= 0));
        } while (!complete);
        return new string(result);
    }

    private static double StrengthBits(int length, List<string> groups)
    {
        // Count valid strings by inclusion-exclusion over missing groups.
        double count = 0;
        for (int mask = 0; mask < (1 << groups.Count); mask++)
        {
            int available = 0;
            int sign = 1;
            for (int i = 0; i < groups.Count; i++)
            {
                if ((mask & (1 << i)) == 0) available += groups[i].Length;
                else sign = -sign;
            }
            count += sign * Math.Pow(available, length);
        }
        return Math.Log(count, 2);
    }
}
