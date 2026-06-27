using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AlligatorGh.Components.UI.PlugInManager.Wpf.Theming;
using AlligatorGh.Components.UI.ThemeCustomizer;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.Services
{
    /// <summary>
    /// Owns the window's themable brushes and drives smooth Light/Dark switching. Every themable
    /// brush is a single shared, unfrozen <see cref="SolidColorBrush"/> stored in the target
    /// <see cref="ResourceDictionary"/> and referenced from XAML via <c>DynamicResource</c>; a theme
    /// switch animates each brush's <see cref="SolidColorBrush.Color"/> to the new palette, so every
    /// bound element crossfades in lockstep. Neumorphic shadow colours are plain <see cref="Color"/>
    /// resources, swapped instantly (imperceptible under the brush crossfade).
    /// </summary>
    public sealed class ThemeService
    {
        // Brush resource keys (string keys, referenced from XAML as {DynamicResource <key>}).
        public const string WindowBackground = "Brush.Window.Background";
        public const string SurfaceAlt = "Brush.Surface.Alt";
        public const string CardBackground = "Brush.Card.Background";
        public const string Text = "Brush.Text";
        public const string MutedText = "Brush.Muted.Text";
        public const string Accent = "Brush.Accent";
        public const string AccentText = "Brush.Accent.Text";
        public const string Border = "Brush.Border";
        public const string RowSelected = "Brush.Row.Selected";
        public const string TrackOff = "Brush.Track.Off";

        // Shadow colour resource keys (consumed by DropShadowEffect.Color via DynamicResource).
        public const string ShadowDarkColor = "Color.Shadow.Dark";
        public const string ShadowLightColor = "Color.Shadow.Light";

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(220);

        private readonly ResourceDictionary _target;

        /// <summary>
        /// Initialises the service, installing brushes for the given starting theme into the target
        /// dictionary (no animation for the initial paint).
        /// </summary>
        /// <param name="target">The dictionary to install brush/colour resources into (the window's Resources).</param>
        /// <param name="isDark">The starting theme.</param>
        public ThemeService(ResourceDictionary target, bool isDark)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            IsDark = isDark;
            InstallInitial(ThemePalette.For(isDark));
        }

        /// <summary>
        /// Gets the current theme: true for dark, false for light.
        /// </summary>
        public bool IsDark { get; private set; }

        /// <summary>
        /// Switches the theme, animating the brush crossfade. No-op if already on the requested theme.
        /// </summary>
        /// <param name="isDark">True for dark, false for light.</param>
        /// <param name="animate">When false, applies the new colours instantly (e.g. initial paint).</param>
        public void SetTheme(bool isDark, bool animate = true)
        {
            if (isDark == IsDark)
            {
                return;
            }

            IsDark = isDark;
            ThemePalette p = ThemePalette.For(isDark);

            Animate(WindowBackground, p.WindowBackground, animate);
            Animate(SurfaceAlt, p.SurfaceAlt, animate);
            Animate(CardBackground, p.CardBackground, animate);
            Animate(Text, p.Text, animate);
            Animate(MutedText, p.MutedText, animate);
            Animate(Accent, p.Accent, animate);
            Animate(AccentText, p.AccentText, animate);
            Animate(Border, p.Border, animate);
            Animate(RowSelected, p.RowSelected, animate);
            Animate(TrackOff, p.TrackOff, animate);

            // Shadow colours swap instantly under the brush crossfade.
            _target[ShadowDarkColor] = p.ShadowDark;
            _target[ShadowLightColor] = p.ShadowLight;
        }

        /// <summary>
        /// Persists the current choice back to <see cref="ThemeManager.CurrentBaseTheme"/> so the
        /// window reopens on the user's last-used theme.
        /// </summary>
        public void PersistChoice()
        {
            ThemeManager.CurrentBaseTheme = IsDark ? "Dark" : "Default";
        }

        private void InstallInitial(ThemePalette p)
        {
            Register(WindowBackground, p.WindowBackground);
            Register(SurfaceAlt, p.SurfaceAlt);
            Register(CardBackground, p.CardBackground);
            Register(Text, p.Text);
            Register(MutedText, p.MutedText);
            Register(Accent, p.Accent);
            Register(AccentText, p.AccentText);
            Register(Border, p.Border);
            Register(RowSelected, p.RowSelected);
            Register(TrackOff, p.TrackOff);

            _target[ShadowDarkColor] = p.ShadowDark;
            _target[ShadowLightColor] = p.ShadowLight;
        }

        // Creates one mutable brush, keeps it in the field map (so we always hold a live reference)
        // and registers the SAME instance in the window resources for DynamicResource lookups. The
        // dictionary is code-added (not Source-loaded), so the instance is not frozen and stays
        // animatable for the window's lifetime.
        private void Register(string key, Color color)
        {
            var brush = new SolidColorBrush(color);
            _brushes[key] = brush;
            _target[key] = brush;
        }

        // Live, mutable brushes kept OUTSIDE any element's ResourceDictionary. WPF freezes a
        // Freezable when it is placed into the resources of an element that is connected to a
        // presentation source, which is what made the in-dictionary brushes non-animatable. Holding
        // them in a plain field dictionary keeps them mutable for the lifetime of the window; the
        // window's resources hold the SAME instances (inserted once at construction, before the
        // window is shown) so DynamicResource lookups and these animations act on one object.
        private readonly System.Collections.Generic.Dictionary<string, SolidColorBrush> _brushes
            = new System.Collections.Generic.Dictionary<string, SolidColorBrush>();

        private void Animate(string key, Color target, bool animate)
        {
            if (!_brushes.TryGetValue(key, out SolidColorBrush brush))
            {
                return;
            }

            // WPF freezes a Freezable when it is inserted into the resources of an element that is
            // already connected to a presentation source — which is why a brush taken from / placed
            // into the live window resources is frozen and cannot be animated. The escape: build a
            // fresh brush, START its animation (a Freezable with an active animation clock cannot be
            // frozen), and only THEN publish it into the resources. DynamicResource consumers
            // re-resolve to the new, animating instance.
            Color start = brush.Color;
            var fresh = new SolidColorBrush(start);

            if (animate)
            {
                var anim = new ColorAnimation(target, new Duration(TransitionDuration))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                };
                fresh.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
            else
            {
                fresh.Color = target;
            }

            _brushes[key] = fresh;
            _target[key] = fresh;
        }
    }
}
