using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Ratings;
using TailBlazer.Domain.Settings;
using Color = System.Windows.Media.Color;
using Theme = TailBlazer.Domain.Formatting.Theme;

namespace TailBlazer.Views.Formatting
{
    public sealed class SystemSetterJob: IDisposable
    {
        private readonly IDisposable _cleanUp;
        
        public SystemSetterJob(ISetting<GeneralOptions> setting,
            IRatingService ratingService,
            ISchedulerProvider schedulerProvider)
        {
             var themeSetter =  setting.Value.Select(options => options.Theme)
                .DistinctUntilChanged()
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(theme =>
                {
                    var dark = theme == Theme.Dark;

                    ModifyTheme(t => t.SetBaseTheme(dark ? (IBaseTheme)new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme()));
                    var sysColor = System.Drawing.Color.FromName(theme.GetAccentColor());
                    var color = Color.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);
                    ModifyTheme(t => t.SetSecondaryColor(color));
                });

            var frameRate = ratingService.Metrics
                .Take(1)
                .Select(metrics => metrics.FrameRate)
                .Wait();

            schedulerProvider.MainThread.Schedule(() =>
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = frameRate });

            });

            _cleanUp = new CompositeDisposable( themeSetter);
        }

        private void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}