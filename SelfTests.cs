using System.IO;
namespace VelocityStudio;
internal static class SelfTests
{
 public static void Run()
 {
  string root=Path.Combine(Path.GetTempPath(),"VelocityStudio-test-"+Guid.NewGuid().ToString("N"));
  Directory.CreateDirectory(root);
  try {
   var store=new ProfileStore(root);var profile=new StudioProfile{Name="Łososiowy – mój profil",Compact=true,Scale=110,Opacity=80};
   store.Save([profile]);var loaded=new ProfileStore(root).Load();Assert(loaded.Count==1&&loaded[0]==profile,"Zapis i odczyt pełnego profilu");
   string export=Path.Combine(root,"export.json");ProfileStore.Export(export,profile);Assert(ProfileStore.Import(export)==profile,"Eksport i import z polskimi znakami");
   File.WriteAllText(export,"{\"Scale\":1000}");ExpectFailure(()=>ProfileStore.Import(export),"Walidacja zewnętrznego profilu");
   var window=new MainWindow(store);window.RunUiSmokeTest();window.Close();
   File.WriteAllText(Path.Combine(root,"profiles.json"),"{ broken");var broken=new ProfileStore(root);Assert(broken.Load().Count==0&&broken.LoadWarning!=null,"Obsługa uszkodzonych danych");
   ExpectFailure(()=>broken.Save([profile]),"Ochrona uszkodzonego pliku przed nadpisaniem");Assert(File.ReadAllText(Path.Combine(root,"profiles.json"))=="{ broken","Zachowanie oryginalnych danych");
   File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"self-test-result.txt"),"PASS: zapis/odczyt, import/eksport, polskie znaki, walidacja, ochrona uszkodzonych danych, kontrolki podglądu, nawigacja, zapis/wczytanie z interfejsu, 19 ikon.\n");
  }finally{foreach(var file in Directory.EnumerateFiles(root))File.Delete(file);Directory.Delete(root);}
 }
 private static void Assert(bool value,string message){if(!value)throw new Exception(message);}
 private static void ExpectFailure(Action action,string message){try{action();}catch(Exception ex) when(ex is IOException or InvalidDataException or ArgumentException){return;}throw new Exception(message);}
}
