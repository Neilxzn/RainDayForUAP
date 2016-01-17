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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RainDayForUAP.RainyDaySystem
{
    public sealed partial class RainyDayCanvas : UserControl
    {
        public static ICanvasBrush opacityZeroBrush;
        const float defaultDpi = 96;
        CanvasRenderTarget glassSurface;
        CanvasBitmap imgbackground;
        GaussianBlurEffect blurEffect;
        RainyDay rainday;



        public RainyDayCanvas()
        {
            InitializeComponent();
        }
        private void Canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            glassSurface = new CanvasRenderTarget(sender, (float)sender.Width, (float)sender.Height, defaultDpi);           
            args.TrackAsyncAction(PrepareRainday(sender).AsAsyncAction());
            opacityZeroBrush = new CanvasSolidColorBrush(sender, Colors.Transparent);        
        }

        private async Task PrepareRainday(CanvasAnimatedControl sender)
        {
            imgbackground = await CanvasBitmap.LoadAsync(sender, "Images/bg2.jpg", defaultDpi);
            blurEffect = new GaussianBlurEffect()
            {
                Source = imgbackground,
                BlurAmount = 4.0f
            };
            rainday = new RainyDay(sender, (float)sender.Width, (float)sender.Height, imgbackground);
            var pesets = new List<List<float>>() {
                new List<float> { 3, 3, 0.88f },
                new List<float> { 5, 5, 0.9f },
                new List<float> { 6, 2, 1 } };
            rainday.Rain(pesets, 100);
        }


        private void Canvas_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
                using (var ds = glassSurface.CreateDrawingSession())
                {
                    rainday.UpdateDrops(ds);
                }
        }

       
        private void Canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(blurEffect);
            args.DrawingSession.DrawImage(glassSurface,0,0);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }
    }
}
