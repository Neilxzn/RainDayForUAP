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

        const float defaultDpi = 96;
        CanvasRenderTarget glassSurface;
        CanvasBitmap imgbackground;
        GaussianBlurEffect blurEffect;
        RainyDay rainday;

        public RainyDayCanvas()
        {
            InitializeComponent();
        }
        private void Canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
           
            args.TrackAsyncAction(PrepareRainday(sender).AsAsyncAction());    
        }

        private async Task PrepareRainday(CanvasAnimatedControl sender)
        {
            imgbackground = await CanvasBitmap.LoadAsync(sender, "Images/bg2.jpg", defaultDpi);
           
            blurEffect = new GaussianBlurEffect()
            {
                Source = imgbackground,
                BlurAmount = 4.0f
            };
            float scalefactor = (float)Math.Min(sender.Size.Width / imgbackground.Size.Width, sender.Size.Height / imgbackground.Size.Height);

            float imgW = (float)imgbackground.Size.Width * scalefactor;
            float imgH = (float)imgbackground.Size.Height * scalefactor;
            float imgX = (float)(sender.Size.Width - imgW) / 2;
            float imgY = (float)(sender.Size.Height - imgH) / 2;
            glassSurface = new CanvasRenderTarget(sender, imgW, imgH, defaultDpi);
            rainday = new RainyDay(sender, imgW, imgH, imgbackground)
            {
                ImgSclaeFactor = scalefactor,
                GravityAngle = (float)Math.PI / 9
            };
           // rainday.ImgSclaeFactor = scalefactor;
            var pesets = new List<List<float>>() {
                //new List<float> { 3, 3, 0.88f },
                //new List<float> { 5, 5, 0.9f },
                //new List<float> { 6, 2, 1 }
                new List<float> { 1, 0, 1000 },
                new List<float> { 3, 3, 1 },
            };
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

            float scalefactor = rainday.ImgSclaeFactor;

            float imgW = (float)imgbackground.Size.Width * scalefactor;
            float imgH = (float)imgbackground.Size.Height * scalefactor;
            float imgX = (float)(sender.Size.Width - imgW) / 2;
            float imgY = (float)(sender.Size.Height - imgH) / 2;
            args.DrawingSession.DrawImage(blurEffect,new Rect(imgX,imgY,imgW,imgH),new Rect(0,0,imgbackground.Size.Width,imgbackground.Size.Height));
            args.DrawingSession.DrawImage(glassSurface,imgX,imgY);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
