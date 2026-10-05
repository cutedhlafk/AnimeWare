using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace VelocityStudio;
public partial class App : Application
{
 protected override void OnStartup(StartupEventArgs e)
 {
  base.OnStartup(e);
  if(e.Args.Contains("--self-test")) { ShutdownMode=ShutdownMode.OnExplicitShutdown;try { SelfTests.Run(); Shutdown(0); } catch(Exception ex) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"self-test-error.txt"),ex.ToString()); Shutdown(1); } return; }
  bool preview=e.Args.Length>=2&&e.Args[0]=="--render-preview";
  string data=preview?Path.Combine(Path.GetTempPath(),"VelocityStudio-preview"):Path.Combine(AppContext.BaseDirectory,"UserData");
  var window=new MainWindow(new ProfileStore(data)); MainWindow=window; window.Show();
  if(preview) window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,new Action(()=>{
   try {
    if(e.Args.Length>2)window.ShowPreviewPage(e.Args[2]);window.UpdateLayout();
    var content=(FrameworkElement)window.Content;
    var bitmap=new RenderTargetBitmap((int)content.ActualWidth,(int)content.ActualHeight,96,96,PixelFormats.Pbgra32);
    bitmap.Render(content);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
    using var stream=File.Create(Path.GetFullPath(e.Args[1]));encoder.Save(stream);
   } finally {window.Close();}
  }));
 }
}
