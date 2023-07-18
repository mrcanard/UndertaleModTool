// Original script by Kneesnap, updated by Grossley
using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

int maxCount;

// Setup root export folder.
string winFolder = GetFolder(FilePath); // The folder data.win is located in.
bool usesAGRP               = (Data.AudioGroups.Count > 0);

// Sound Folder
string ExportFolder = winFolder + "sounds\\";

//Overwrite Folder Check One
if (Directory.Exists(ExportFolder))
{
    Directory.Delete(ExportFolder, true);
}

var externalOGG_Copy = 1;
string externalOGG_Folder = winFolder + "undertale_ogg\\";


// Group by audio group check
//var groupedExport = 0;
//if (usesAGRP)
//{
//    bool groupedCheck = ScriptQuestion(@"Group sounds by audio group?
//    ");
//    if (groupedCheck)
//        groupedExport = 1;
//    if (!groupedCheck)
//        groupedExport = 0;
//}

byte[] EMPTY_WAV_FILE_BYTES = System.Convert.FromBase64String("UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA=");
string DEFAULT_AUDIOGROUP_NAME = "audiogroup_default";

maxCount = Data.Sounds.Count;
SetProgressBar(null, "Sound", 0, maxCount);
StartProgressBarUpdater();

await Task.Run(DumpSounds); // This runs sync, because it has to load audio groups.

await StopProgressBarUpdater();
HideProgressBar();
//if (Directory.Exists(winFolder + "External_Sounds\\"))
//    ScriptMessage("Sounds exported to " + winFolder + " in the 'Exported_Sounds' and 'External_Sounds' folders.");
//else
//    ScriptMessage("Sounds exported to " + winFolder + " in the 'Exported_Sounds' folder.");

void IncProgressLocal()
{
    if (GetProgress() < maxCount)
        IncrementProgress();
}

void MakeFolder(String folderName)
{
    if (!Directory.Exists(winFolder + folderName + "/"))
        Directory.CreateDirectory(winFolder + folderName + "/");
}

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

Dictionary<string, IList<UndertaleEmbeddedAudio>> loadedAudioGroups;
IList<UndertaleEmbeddedAudio> GetAudioGroupData(UndertaleSound sound)
{
    if (loadedAudioGroups == null)
        loadedAudioGroups = new Dictionary<string, IList<UndertaleEmbeddedAudio>>();

    string audioGroupName = sound.AudioGroup != null ? sound.AudioGroup.Name.Content : DEFAULT_AUDIOGROUP_NAME;
    if (loadedAudioGroups.ContainsKey(audioGroupName))
        return loadedAudioGroups[audioGroupName];

    string groupFilePath = winFolder + "audiogroup" + sound.GroupID + ".dat";
    if (!File.Exists(groupFilePath))
        return null; // Doesn't exist.

    try
    {
        UndertaleData data = null;
        using (var stream = new FileStream(groupFilePath, FileMode.Open, FileAccess.Read))
            data = UndertaleIO.Read(stream, warning => ScriptMessage("A warning occured while trying to load " + audioGroupName + ":\n" + warning));

        loadedAudioGroups[audioGroupName] = data.EmbeddedAudio;
        return data.EmbeddedAudio;
    } catch (Exception e)
    {
        ScriptMessage("An error occured while trying to load " + audioGroupName + ":\n" + e.Message);
        return null;
    }
}

byte[] GetSoundData(UndertaleSound sound)
{
    if (sound.AudioFile != null)
        return sound.AudioFile.Data;

    if (sound.GroupID > Data.GetBuiltinSoundGroupID())
    {
        IList<UndertaleEmbeddedAudio> audioGroup = GetAudioGroupData(sound);
        if (audioGroup != null)
            return audioGroup[sound.AudioID].Data;
    }
    return EMPTY_WAV_FILE_BYTES;
}

void DumpSounds()
{
    MakeFolder("Export_Sounds");
    // MakeFolder("Export_Sounds\\audio");
    foreach (UndertaleSound sound in Data.Sounds)
        DumpSound(sound);
}

void DumpSound(UndertaleSound sound)
{
    string soundName = sound.Name.Content;
    bool flagCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
    bool flagEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);
    bool flagDecompressedOnLoad = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsDecompressedOnLoad);
    bool flagRegular = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.Regular);

    string audioExt = ".ogg";
    string soundFilePath;

    // Création du répertoire pour le son
    soundFilePath = winFolder + "sounds\\" + soundName;
    MakeFolder("sounds");
    Directory.CreateDirectory(soundFilePath);

    soundFilePath = soundFilePath + "\\" + soundName;

    // Compression, Streamed, Unpack on Load.
    // 1 = 000 = IsEmbedded, Regular.               '.wav' type saved in win.
    // 2 = 100 = IsCompressed, Regular.             '.ogg' type saved in win
    // 3 = 101 = IsEmbedded, IsCompressed, Regular. '.ogg' type saved in win.
    // 4 = 110 = Regular.                           '.ogg' type saved outside win.

    /*
    if (groupedExport == 1)
       soundFilePath = winFolder + "Export_Sounds\\audio\\" + sound.AudioGroup.Name.Content + "\\" + soundName;
    else
    if (groupedExport == 1)
       MakeFolder("Export_Sounds\\" + sound.AudioGroup.Name.Content);
    */

    bool process = true;
    if (flagEmbedded && !flagCompressed) // 1.
        audioExt = ".wav";
    else if (flagCompressed && !flagEmbedded) // 2.
        audioExt = ".ogg";
    else if (flagCompressed && flagEmbedded) // 3.
        audioExt = ".ogg";
    else if (!flagCompressed && !flagEmbedded)
    {
        process = false;
        audioExt = ".ogg";
        string source = externalOGG_Folder + soundName + audioExt;
        string dest = winFolder + "sounds" + "\\" + soundName + "\\" + soundName + audioExt;
        if (externalOGG_Copy == 1)
        {
            //if (groupedExport == 1)
            //{
            //    dest = winFolder + "Export_Sounds\\audio\\" + sound.AudioGroup.Name.Content + "\\" + soundName + audioExt;
            //    MakeFolder("Export_Sounds\\audio\\" + sound.AudioGroup.Name.Content);
            //}
            System.IO.File.Copy(source, dest, false);
        }
    }

    if (process && !File.Exists(soundFilePath + audioExt))
        File.WriteAllBytes(soundFilePath + audioExt, GetSoundData(sound));

    using (StreamWriter writer = new StreamWriter(soundFilePath + ".yy"))
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMSound\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \""+ sound.Name.Content + "\",");
        writer.WriteLine("  \"audioGroupId\": {");
        writer.WriteLine("    \"name\": \"audiogroup_default\",");
        writer.WriteLine("    \"path\": \"audiogroups/audiogroup_default\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Sounds\",");
        writer.WriteLine("    \"path\": \"folders/Sounds.yy\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"bitDepth\": 1,");
        writer.WriteLine("  \"bitRate\": 128,");
        writer.WriteLine("  \"compression\": "+ (flagCompressed ? 1 : 0) +",");
        writer.WriteLine("  \"conversionMode\": 0,");
        writer.WriteLine("  \"preload\": "+ (sound.Preload ? "true" : "false") +",");
        writer.WriteLine("  \"soundFile\": \""+ sound.Name.Content + audioExt +"\",");
        writer.WriteLine("  \"volume\": "+ sound.Volume +",");
        writer.WriteLine("}");
    }


    IncProgressLocal();
}

