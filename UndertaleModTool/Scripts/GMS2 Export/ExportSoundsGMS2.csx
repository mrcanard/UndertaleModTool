// Original script by Kneesnap, updated by Grossley
using System;
using System.IO;
using System.Threading.Tasks;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
    System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

//

int maxCount;

// Setup root export folder.
string winFolder = GetFolder(FilePath); // The folder data.win is located in.
bool usesAGRP = (Data.AudioGroups.Count > 0);

// Sound Folder
string ExportFolder = winFolder + "sounds" + Path.DirectorySeparatorChar;

//Overwrite Folder Check One
if (Directory.Exists(ExportFolder))
{
    Directory.Delete(ExportFolder, true);
}

var externalOGG_Copy = 1;
string externalOGG_Folder = winFolder + "undertale_ogg" + Path.DirectorySeparatorChar;

byte[] EMPTY_WAV_FILE_BYTES = System.Convert.FromBase64String(
    "UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA="
);
string DEFAULT_AUDIOGROUP_NAME = "audiogroup_default";

maxCount = Data.Sounds.Count;
SetProgressBar(null, "Sound", 0, maxCount);
StartProgressBarUpdater();

await Task.Run(DumpSounds); // This runs sync, because it has to load audio groups.

// Export asset
using (StreamWriter writer = new StreamWriter(ExportFolder + "asset_order.txt"))
{
    for (int i = 0; i < Data.Sounds.Count; i++)
    {
        UndertaleSound sound = Data.Sounds[i];
        writer.WriteLine(
            "    {\"id\":{\"name\":\""
                + sound.Name.Content
                + "\",\"path\":\"sounds/"
                + sound.Name.Content
                + "/"
                + sound.Name.Content
                + ".yy\",},},"
        );
    }
}

await StopProgressBarUpdater();
HideProgressBar();

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

    string audioGroupName =
        sound.AudioGroup != null ? sound.AudioGroup.Name.Content : DEFAULT_AUDIOGROUP_NAME;
    if (loadedAudioGroups.ContainsKey(audioGroupName))
        return loadedAudioGroups[audioGroupName];

    string groupFilePath = winFolder + "audiogroup" + sound.GroupID + ".dat";
    if (!File.Exists(groupFilePath))
        return null; // Doesn't exist.

    try
    {
        UndertaleData data = null;
        using (var stream = new FileStream(groupFilePath, FileMode.Open, FileAccess.Read))
            data = UndertaleIO.Read(
                stream,
                warning =>
                    ScriptMessage(
                        "A warning occured while trying to load " + audioGroupName + ":\n" + warning
                    )
            );

        loadedAudioGroups[audioGroupName] = data.EmbeddedAudio;
        return data.EmbeddedAudio;
    }
    catch (Exception e)
    {
        ScriptMessage(
            "An error occured while trying to load " + audioGroupName + ":\n" + e.Message
        );
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
    // MakeFolder("Export_Sounds");
    // MakeFolder("Export_Sounds/audio");
    foreach (UndertaleSound sound in Data.Sounds)
        DumpSound(sound);
}

void DumpSound(UndertaleSound sound)
{
    string soundName = sound.Name.Content;
    bool flagCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
    bool flagEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);
    bool flagDecompressedOnLoad = sound.Flags.HasFlag(
        UndertaleSound.AudioEntryFlags.IsDecompressedOnLoad
    );
    bool flagRegular = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.Regular);

    string audioExt = ".ogg";
    string soundFilePath;

    // Création du répertoire pour le son
    soundFilePath = winFolder + "sounds" + Path.DirectorySeparatorChar + soundName;
    MakeFolder("sounds");
    Directory.CreateDirectory(soundFilePath);

    soundFilePath = soundFilePath + Path.DirectorySeparatorChar + soundName;

    // Compression, Streamed, Unpack on Load.
    // 1 = 000 = IsEmbedded, Regular.               '.wav' type saved in win.
    // 2 = 100 = IsCompressed, Regular.             '.ogg' type saved in win
    // 3 = 101 = IsEmbedded, IsCompressed, Regular. '.ogg' type saved in win.
    // 4 = 110 = Regular.                           '.ogg' type saved outside win.

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
        string dest =
            winFolder
            + "sounds"
            + Path.DirectorySeparatorChar
            + soundName
            + Path.DirectorySeparatorChar
            + soundName
            + audioExt;
        if (externalOGG_Copy == 1)
        {
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
        writer.WriteLine("  \"name\": \"" + sound.Name.Content + "\",");
        writer.WriteLine("  \"audioGroupId\": {");
        if (
            sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.Regular)
            && Data.GeneralInfo.BytecodeVersion >= 14
        )
        {
            writer.WriteLine("    \"name\": \"audiogroup_default\",");
            writer.WriteLine("    \"path\": \"audiogroups/audiogroup_default\",");
        }
        else
        {
            writer.WriteLine("    \"name\": \"" + sound.AudioGroup.Name.Content + "\",");
            writer.WriteLine(
                "    \"path\": \"audiogroups/" + sound.AudioGroup.Name.Content + "\","
            );
        }
        writer.WriteLine("  },");
        writer.WriteLine("  \"bitDepth\": 1,");
        writer.WriteLine("  \"bitRate\": 128,");
        writer.WriteLine("  \"compression\": " + (flagCompressed ? 1 : 0) + ",");
        writer.WriteLine("  \"conversionMode\": 0,");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Sounds\",");
        writer.WriteLine("    \"path\": \"folders/Sounds.yy\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"preload\": " + (sound.Preload ? "true" : "false") + ",");
        writer.WriteLine("  \"sampleRate\": 44100,");
        writer.WriteLine("  \"soundFile\": \"" + sound.Name.Content + audioExt + "\",");
        writer.WriteLine("  \"type\": " + (sound.Type == null ? "0" : sound.Type) + ",");
        writer.WriteLine("  \"volume\": " + sound.Volume.ToString("0.0") + ",");
        writer.Write("}");
    }

    IncProgressLocal();
}
