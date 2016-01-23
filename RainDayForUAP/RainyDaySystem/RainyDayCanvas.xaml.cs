using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.UI.ViewManagement;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RainDayForUAP.RainyDaySystem
{
    public sealed partial class RainyDayCanvas : UserControl
    {

        const float defaultDpi = 96;
        CanvasRenderTarget glassSurface;
        CanvasBitmap imgbackground;
        GaussianBlurEffect blurEffect;
        RainyDay rainday;
        float scalefactor;
        float imgW;
        float imgH;
        float imgX;
        float imgY;

        private bool fullScreen;
        public bool FullScreen
        {
            get
            {
                return fullScreen;
            }
            set
            {
                if (value)
                {
                    ApplicationView.TryUnsnapToFullscreen();
                 
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    toolSP.Visibility = Visibility.Collapsed;
                    fullScreen = true;
                }
                else
                {
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    toolSP.Visibility = Visibility.Visible;
                    fullScreen = false;
                }
            }
        }

        public RainyDayCanvas()
        {
            InitializeComponent();

        }
        private void Canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {

            args.TrackAsyncAction(PrepareRainday(sender).AsAsyncAction());
        }

        private async Task PrepareRainday(CanvasAnimatedControl sender, string demo = "demo1")
        {
            string imgPath = "Images/" + demo + ".jpg";
            imgbackground = await CanvasBitmap.LoadAsync(sender, imgPath, defaultDpi);

            blurEffect = new GaussianBlurEffect()
            {
                Source = imgbackground,
                BlurAmount = 4.0f
            };
            scalefactor = (float)Math.Min(sender.Size.Width / imgbackground.Size.Width, sender.Size.Height / imgbackground.Size.Height);
            imgW = (float)imgbackground.Size.Width * scalefactor;
            imgH = (float)imgbackground.Size.Height * scalefactor;
            imgX = (float)(sender.Size.Width - imgW) / 2;
            imgY = (float)(sender.Size.Height - imgH) / 2;
            glassSurface = new CanvasRenderTarget(sender, imgW, imgH, defaultDpi);

            List<List<float>> pesets;


            if (demo == "demo1")
            {
                rainday = new RainyDay(sender, imgW, imgH, imgbackground)
                {
                    ImgSclaeFactor = scalefactor,
                    GravityAngle = (float)Math.PI / 2
                };
                pesets = new List<List<float>>() {

                new List<float> { 3, 3, 0.88f },
                new List<float> { 5, 5, 0.9f },
                new List<float> { 6, 2, 1 }
                };
            }
            else if (demo == "demo2")
            {
                rainday = new RainyDay(sender, imgW, imgH, imgbackground)
                {
                    ImgSclaeFactor = scalefactor,
                    GravityAngle = (float)Math.PI / 9
                };
                pesets = new List<List<float>>()
                {
                    new List<float> { 1, 0, 1000 },
                    new List<float> { 3, 3, 1 },
                };
            }
            else if (demo == "demo3")
            {
                rainday = new RainyDay(sender, imgW, imgH, imgbackground)
                {
                    ImgSclaeFactor = scalefactor,
                    CurrentGravity = RainyDay.GravityType.Gravity_None_Linear,
                    GravityAngle = (float)Math.PI / 2
                };
                pesets = new List<List<float>>() {
                new List<float> {0, 2, 200},
                new List<float> { 3, 3, 1 }

            };

            }
            else
            {
                rainday = new RainyDay(sender, imgW, imgH, imgbackground)
                {
                    ImgSclaeFactor = scalefactor,
                    GravityAngle = (float)Math.PI / 2,
                    CurrentGravity = RainyDay.GravityType.Gravity_None_Linear,
                    CurrentTrail = RainyDay.TrailType.Trail_Smudge
                };
                pesets = new List<List<float>>() {
                new List<float> { 3, 3, 0.1f }
            };
            }
            rainday.Rain(pesets, 100);
        }

        private void Canvas_Update(ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            using (var ds = glassSurface.CreateDrawingSession())
            {
                rainday.UpdateDrops(ds);
            }
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(blurEffect, new Rect(imgX, imgY, imgW, imgH), new Rect(0, 0, imgbackground.Size.Width, imgbackground.Size.Height));
            args.DrawingSession.DrawImage(glassSurface, imgX, imgY);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitDemoData();
        }

        void InitDemoData()
        {
            List<string> demos = new List<string>()
            {
                "demo1","demo2","demo3","demo4"
            };
            demosCB.SelectionChanged += DemosCB_SelectionChanged;
            demosCB.ItemsSource = demos;
            demosCB.SelectedIndex = 0;
            this.DataContext = this;

        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            string demo;
            if (demosCB.SelectedValue == null)
            {
                demo = "demo1";
            }
            else
            {
                demo = demosCB.SelectedValue.ToString();
            }
            var action = canvas.RunOnGameLoopThreadAsync(async () =>
            {

                if (glassSurface != null && imgbackground != null)
                {
                    await PrepareRainday(canvas, demo);
                    canvas.Invalidate();
                }

            });
        }
        private void DemosCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = sender as ComboBox;
            string demo = s.SelectedValue.ToString();
            var action = canvas.RunOnGameLoopThreadAsync(async () =>
             {

                 if (glassSurface != null && imgbackground != null)
                 {
                     await PrepareRainday(canvas, demo);
                     canvas.Invalidate();
                 }
             });

        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FullScreen = false;
        }
    }
}
