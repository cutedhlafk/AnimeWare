using System.IO;
using System.Text.Json;
namespace VelocityStudio;
public sealed record StudioProfile
{
 public string Name {get;set;}="Mój profil";
 public string Accent {get;set;}="#F28E9C";
 public double Scale {get;set;}=100;
 public double Opacity {get;set;}=95;
 public bool ShowWatermark {get;set;}=true;
 public bool Compact {get;set;}=false;
 public void Validate()
 {
  if(string.IsNullOrWhiteSpace(Name)||Name.Length>48||Name!=Name.Trim()) throw new InvalidDataException("Nazwa profilu musi mieć od 1 do 48 znaków, bez spacji na początku i końcu.");
  if(!new[]{"#F28E9C","#F5A6B8","#F5A58E"}.Contains(Accent)) throw new InvalidDataException("Nieobsługiwany kolor akcentu.");
  if(!double.IsFinite(Scale)||Scale<85||Scale>115||!double.IsFinite(Opacity)||Opacity<50||Opacity>100) throw new InvalidDataException("Ustawienia podglądu są poza dozwolonym zakresem.");
 }
}
public sealed class ProfileStore(string root)
{
 public string Root {get;}=Path.GetFullPath(root);
 public string? LoadWarning {get;private set;}
 private string FilePath=>Path.Combine(Root,"profiles.json");
 private static readonly JsonSerializerOptions Options=new(){WriteIndented=true};
 public List<StudioProfile> Load()
 {
  LoadWarning=null;
  if(!File.Exists(FilePath)) return [];
  try {
   if(new FileInfo(FilePath).Length>1024*1024) throw new InvalidDataException("Plik profili jest zbyt duży.");
   var rows=JsonSerializer.Deserialize<List<StudioProfile>>(File.ReadAllText(FilePath))??throw new InvalidDataException("Pusty plik profili.");
   foreach(var row in rows) { if(row is null)throw new InvalidDataException("Nieprawidłowy profil.");row.Validate(); }
   if(rows.Select(x=>x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=rows.Count) throw new InvalidDataException("Powtórzone nazwy profili.");
   return rows;
  } catch(Exception ex) when(ex is JsonException or IOException or InvalidDataException or UnauthorizedAccessException) {LoadWarning="Nie wczytano profili: "+ex.Message+" Oryginalny plik pozostaje zachowany.";return [];}
 }
 public void Save(IEnumerable<StudioProfile> profiles)
 {
  if(LoadWarning!=null) throw new IOException("Zapis zablokowany, aby zachować uszkodzony plik profili. Zamknij aplikację i przenieś profiles.json w bezpieczne miejsce przed ponownym uruchomieniem.");
  var rows=profiles.ToList();foreach(var row in rows)row.Validate();
  Directory.CreateDirectory(Root);
  string temp=Path.Combine(Root,"profiles."+Guid.NewGuid().ToString("N")+".tmp");
  try {File.WriteAllText(temp,JsonSerializer.Serialize(rows,Options));File.Move(temp,FilePath,true);} finally {if(File.Exists(temp))File.Delete(temp);}
 }
 public static StudioProfile Import(string path)
 {
  if(new FileInfo(path).Length>65536)throw new InvalidDataException("Profil jest zbyt duży.");
  var profile=JsonSerializer.Deserialize<StudioProfile>(File.ReadAllText(path))??throw new InvalidDataException("Brak profilu w pliku.");profile.Validate();return profile;
 }
 public static void Export(string path,StudioProfile profile){profile.Validate();File.WriteAllText(path,JsonSerializer.Serialize(profile,Options));}
}
