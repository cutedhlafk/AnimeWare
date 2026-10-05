using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Win32;

namespace VelocityStudio;
public partial class MainWindow : Window
{
 private readonly ProfileStore store;
 private readonly ObservableCollection<StudioProfile> profiles=new();
 private string accent="#F28E9C";
 private bool ready;
 public MainWindow(ProfileStore store)
 {
  this.store=store;InitializeComponent();
  foreach(var profile in store.Load())profiles.Add(profile);
  ProfilesList.ItemsSource=profiles;
  BuildIcons();ready=true;UpdatePreview();RefreshProfiles();
  if(store.LoadWarning!=null)StatusText.Text=store.LoadWarning;
  else if(profiles.Count>0)StatusText.Text=$"Gotowe • {profiles.Count} zapisanych profili";
  string image=System.IO.Path.Combine(store.Root,"brand.png");
  if(File.Exists(image))try {ShowBrand(image);}catch(Exception ex) when(ex is IOException or NotSupportedException or FileFormatException){StatusText.Text="Nie udało się wczytać zapisanej grafiki. Możesz wybrać nowy plik.";}
 }
 internal void ShowPreviewPage(string page)=>ShowPage(page);
 private void Navigate(object sender,RoutedEventArgs e)=>ShowPage(((Button)sender).Tag?.ToString()??"appearance");
 private void ShowPage(string page)
 {
  AppearancePage.Visibility=page=="appearance"?Visibility.Visible:Visibility.Collapsed;
  ProfilesPage.Visibility=page=="profiles"?Visibility.Visible:Visibility.Collapsed;
  AssetsPage.Visibility=page=="assets"?Visibility.Visible:Visibility.Collapsed;
  foreach(var button in new[]{AppearanceNav,ProfilesNav,AssetsNav})button.Background=(Brush)new BrushConverter().ConvertFromString(button.Tag?.ToString()==page?"#3A2932":"Transparent")!;
  PageTitle.Text=page switch{"profiles"=>"Twoje profile","assets"=>"Grafika i zasoby",_=>"Wygląd interfejsu"};
  PageSubtitle.Text=page switch{"profiles"=>"Zapisuj, wczytuj i przenoś swoje ustawienia.","assets"=>"Miejsce na Twoją identyfikację wizualną.",_=>"Ciemny motyw. Łososiowy akcent. Twój styl."};
 }
 private void SettingChanged(object sender,RoutedEventArgs e){if(ready)UpdatePreview();}
 private void SliderChanged(object sender,RoutedPropertyChangedEventArgs<double> e){if(ready)UpdatePreview();}
 private void SelectAccent(object sender,RoutedEventArgs e){accent=((Button)sender).Tag.ToString()!;UpdatePreview();}
 private void UpdatePreview()
 {
  Resources["Accent"]=(Brush)new BrushConverter().ConvertFromString(accent)!;
  PreviewName.Text=string.IsNullOrWhiteSpace(ProfileName.Text)?"Twój profil":ProfileName.Text.Trim();
  ScaleText.Text=$"{ScaleSlider.Value:0}%";OpacityText.Text=$"{OpacitySlider.Value:0}%";
  PreviewScale.ScaleX=PreviewScale.ScaleY=ScaleSlider.Value/100;
  PreviewPanel.Opacity=OpacitySlider.Value/100;
  PreviewWatermark.Visibility=WatermarkToggle.IsChecked==true?Visibility.Visible:Visibility.Hidden;
  bool compact=CompactToggle.IsChecked==true;PreviewPanel.Padding=new Thickness(compact?12:18);PreviewRow.Margin=new Thickness(0,compact?5:10,0,0);
  LayoutValue.Text=compact?"Kompaktowy":"Standardowy";
  AccentValue.Text=accent switch{"#F5A6B8"=>"Różowy","#F5A58E"=>"Brzoskwinia",_=>"Łosoś"};
  StatusText.Text="Podgląd zaktualizowany • zapisz profil, aby zachować zmiany";
 }
 private StudioProfile ReadProfile()=>new(){Name=ProfileName.Text.Trim(),Accent=accent,Scale=ScaleSlider.Value,Opacity=OpacitySlider.Value,ShowWatermark=WatermarkToggle.IsChecked==true,Compact=CompactToggle.IsChecked==true};
 private void ApplyProfile(StudioProfile profile)
 {
  ready=false;ProfileName.Text=profile.Name;accent=profile.Accent;ScaleSlider.Value=profile.Scale;OpacitySlider.Value=profile.Opacity;WatermarkToggle.IsChecked=profile.ShowWatermark;CompactToggle.IsChecked=profile.Compact;ready=true;UpdatePreview();
 }
 private void SaveProfile(object sender,RoutedEventArgs e)=>Guard(()=>{
  var profile=ReadProfile();profile.Validate();
  var old=profiles.FirstOrDefault(x=>string.Equals(x.Name,profile.Name,StringComparison.OrdinalIgnoreCase));
  if(old!=null&&MessageBox.Show(this,$"Zastąpić zapisany profil „{old.Name}”?","Zapis profilu",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
  var next=profiles.ToList();if(old!=null)next.Remove(old);next.Add(profile);store.Save(next);
  if(old!=null)profiles.Remove(old);profiles.Add(profile);ProfilesList.SelectedItem=profile;RefreshProfiles();StatusText.Text=$"Zapisano profil „{profile.Name}”";
 });
 private void LoadProfile(object sender,RoutedEventArgs e)
 {
  if(ProfilesList.SelectedItem is not StudioProfile profile){StatusText.Text="Najpierw wybierz profil z listy.";return;}
  ApplyProfile(profile);ShowPage("appearance");StatusText.Text=$"Wczytano profil „{profile.Name}”";
 }
 private void ExportProfile(object sender,RoutedEventArgs e)=>Guard(()=>{
  if(ProfilesList.SelectedItem is not StudioProfile profile){StatusText.Text="Wybierz zapisany profil do eksportu.";return;}
  var dialog=new SaveFileDialog{Title="Eksport profilu",Filter="Profil JSON (*.json)|*.json",FileName="velocity-profile.json",DefaultExt=".json"};
  if(dialog.ShowDialog(this)==true){ProfileStore.Export(dialog.FileName,profile);StatusText.Text="Profil wyeksportowany.";}
 });
 private void ImportProfile(object sender,RoutedEventArgs e)=>Guard(()=>{
  var dialog=new OpenFileDialog{Title="Import profilu",Filter="Profil JSON (*.json)|*.json"};
  if(dialog.ShowDialog(this)!=true)return;
  var profile=ProfileStore.Import(dialog.FileName);ApplyProfile(profile);ShowPage("appearance");StatusText.Text="Wczytano podgląd importowanego profilu. Wybierz „Zapisz profil”, aby go zachować.";
 });
 private void ResetAppearance(object sender,RoutedEventArgs e){ApplyProfile(new StudioProfile());StatusText.Text="Przywrócono domyślny wygląd podglądu. Zapisane profile pozostają dostępne.";}
 private void RefreshProfiles()=>EmptyProfiles.Visibility=profiles.Count==0?Visibility.Visible:Visibility.Collapsed;
 private void Guard(Action action){try{action();}catch(Exception ex) when(ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException or NotSupportedException or FileFormatException){StatusText.Text="Nie udało się wykonać operacji: "+ex.Message;}}
 internal void RunUiSmokeTest()
 {
  ProfileName.Text="Test interfejsu";ScaleSlider.Value=110;OpacitySlider.Value=75;CompactToggle.IsChecked=true;WatermarkToggle.IsChecked=false;
  if(PreviewScale.ScaleX!=1.1||PreviewPanel.Opacity!=0.75||PreviewWatermark.Visibility!=Visibility.Hidden||LayoutValue.Text!="Kompaktowy")throw new Exception("Podgląd nie reaguje na ustawienia.");
  SaveProfile(this,new RoutedEventArgs());
  if(!store.Load().Any(x=>x.Name=="Test interfejsu"&&x.Compact&&x.Scale==110))throw new Exception("Zapis z interfejsu nie zachował ustawień.");
  ResetAppearance(this,new RoutedEventArgs());LoadProfile(this,new RoutedEventArgs());
  if(ScaleSlider.Value!=110||CompactToggle.IsChecked!=true)throw new Exception("Wczytanie profilu z interfejsu.");
  ShowPage("profiles");if(ProfilesPage.Visibility!=Visibility.Visible||AppearancePage.Visibility!=Visibility.Collapsed)throw new Exception("Nawigacja profili.");
  ShowPage("assets");if(AssetsPage.Visibility!=Visibility.Visible||IconGallery.Children.Count!=19)throw new Exception("Nawigacja i zasoby.");
 }
 private void ImportBrand(object sender,RoutedEventArgs e)=>Guard(()=>{
  var dialog=new OpenFileDialog{Title="Wybierz swoją grafikę",Filter="Grafika (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"};if(dialog.ShowDialog(this)!=true)return;
  if(new FileInfo(dialog.FileName).Length>20*1024*1024)throw new InvalidDataException("Wybierz grafikę mniejszą niż 20 MB.");
  var image=ReadImage(dialog.FileName);Directory.CreateDirectory(store.Root);
  string target=System.IO.Path.Combine(store.Root,"brand.png");string temp=System.IO.Path.Combine(store.Root,"brand.tmp");
  var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using(var stream=File.Create(temp))encoder.Save(stream);File.Move(temp,target,true);
  BrandImage.Source=image;BrandImage.Visibility=Visibility.Visible;BrandPlaceholder.Visibility=Visibility.Collapsed;StatusText.Text="Twoja grafika została wczytana i zapisana lokalnie.";
 });
 private static BitmapImage ReadImage(string path){var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=1600;image.UriSource=new Uri(System.IO.Path.GetFullPath(path));image.EndInit();image.Freeze();return image;}
 private void ShowBrand(string path){BrandImage.Source=ReadImage(path);BrandImage.Visibility=Visibility.Visible;BrandPlaceholder.Visibility=Visibility.Collapsed;}
 private void BuildIcons()
 {
  // SVG path data is rendered as vector geometry; no browser or third-party package is needed.
  string[] names={"0033fc31","0033fcee","003403bd","003407a4","003408c2","00340b0b","00340d04","00340eff","0034164c","00341883","00341ad2","00341d07","00341f26","00342329","003424f5","00342a52","00342eaf","00342ff2","00270300"};
  foreach(string name in names){
   var resource=Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Icons/icon_"+name+".svg"));if(resource==null)continue;
   using var stream=resource.Stream;var svg=XDocument.Load(stream);var root=svg.Root!;
   string[] box=(root.Attribute("viewBox")?.Value??"0 0 24 24").Split(' ',StringSplitOptions.RemoveEmptyEntries);
   double w=double.Parse(box[2],CultureInfo.InvariantCulture),h=double.Parse(box[3],CultureInfo.InvariantCulture);var canvas=new Canvas{Width=w,Height=h};
   foreach(var p in root.Descendants().Where(x=>x.Name.LocalName=="path")){
    string? d=p.Attribute("d")?.Value;if(d==null)continue;
    var path=new System.Windows.Shapes.Path{Data=Geometry.Parse(d),Stroke=p.Attribute("stroke")!=null?new SolidColorBrush(Color.FromRgb(242,142,156)):null,Fill=p.Attribute("fill")?.Value is string fill&&fill!="none"?new SolidColorBrush(Color.FromRgb(242,142,156)):null,StrokeThickness=double.TryParse(p.Attribute("stroke-width")?.Value,NumberStyles.Float,CultureInfo.InvariantCulture,out var sw)?sw:1,StrokeLineJoin=PenLineJoin.Round};canvas.Children.Add(path);
   }
   IconGallery.Children.Add(new Border{Background=new SolidColorBrush(Color.FromRgb(40,32,40)),CornerRadius=new CornerRadius(8),Width=48,Height=48,Margin=new Thickness(0,0,8,8),Padding=new Thickness(12),Child=new Viewbox{Child=canvas},ToolTip="icon_"+name+".svg"});
  }
 }
}
