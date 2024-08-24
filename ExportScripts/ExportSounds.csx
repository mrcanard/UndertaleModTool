// Original script by Kneesnap, updated by Grossley
using System;
using System.IO;
using System.Text;
using System.Threading;
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
string ExportFolder;
if(Data.IsGameMaker2()) {
    ExportFolder = winFolder + "sounds" + Path.DirectorySeparatorChar;
} else {
    ExportFolder = winFolder + "sound" + Path.DirectorySeparatorChar;
}

Console.WriteLine(winFolder);
Console.WriteLine(ExportFolder);

//Overwrite Folder Check One
if (Directory.Exists(ExportFolder))
{
    Directory.Delete(ExportFolder, true);
}

Directory.CreateDirectory(ExportFolder);

var externalOGG_Copy = 1;
string externalOGG_Folder = winFolder + "undertale_ogg/";

byte[] EMPTY_WAV_FILE_BYTES = System.Convert.FromBase64String(
    "UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA="
);
string DEFAULT_AUDIOGROUP_NAME = "audiogroup_default";

maxCount = Data.Sounds.Count;
SetProgressBar(null, "Sound", 0, maxCount);
StartProgressBarUpdater();

await Task.Run(DumpSounds); // This runs sync, because it has to load audio groups.

// Export asset
if(Data.IsGameMaker2()) {
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

} else {
    using (StreamWriter writer = new StreamWriter(ExportFolder + "asset_order.txt"))
    {
        writer.WriteLine("  <sounds name=\"sound\">");
        for (int i = 0; i < Data.Sounds.Count; i++)
        {
            UndertaleSound sound = Data.Sounds[i];
            writer.WriteLine("    <sound>sound\\" + sound.Name.Content + "</sound>");
        }
        writer.WriteLine("  </sounds>");
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
    if (!Directory.Exists(winFolder + folderName + Path.DirectorySeparatorChar))
        Directory.CreateDirectory(winFolder + folderName + Path.DirectorySeparatorChar);
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
    foreach (UndertaleSound sound in Data.Sounds)
        DumpSound(sound);
}

void DumpSound(UndertaleSound sound)
{
    if(Data.IsGameMaker2()) {
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
            string dest = winFolder + "sounds" + Path.DirectorySeparatorChar + soundName + Path.DirectorySeparatorChar + soundName + audioExt;
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
            writer.WriteLine("    \"name\": \"audiogroup_default\",");
            writer.WriteLine("    \"path\": \"audiogroups/audiogroup_default\",");
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
            writer.WriteLine("  \"soundFile\": \"" + sound.Name.Content + audioExt + "\",");
            writer.WriteLine("  \"volume\": " + sound.Volume.ToString("0.0") + ",");
            writer.Write("}");
        }

    } else {
        string soundName = sound.Name.Content;
        bool flagCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
        bool flagEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);
        // Compression, Streamed, Unpack on Load.
        // 1 = 000 = IsEmbedded, Regular.               '.wav' type saved in win.
        // 2 = 100 = IsCompressed, Regular.             '.ogg' type saved in win
        // 3 = 101 = IsEmbedded, IsCompressed, Regular. '.ogg' type saved in win.
        // 4 = 110 = Regular.                           '.ogg' type saved outside win.
        string audioExt = ".ogg";
        string soundFilePath;
        //if (groupedExport == 1)
        //    soundFilePath = winFolder + "sound\\audio\\" + sound.AudioGroup.Name.Content + "\\" + soundName;
        //else
        soundFilePath = winFolder + "sound" + Path.DirectorySeparatorChar + "audio" + Path.DirectorySeparatorChar + soundName;
        MakeFolder("sound");
        //if (groupedExport == 1)
        //    MakeFolder("sound\\" + sound.AudioGroup.Name.Content);
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
            string dest = winFolder + "sound" + Path.DirectorySeparatorChar + "audio" + Path.DirectorySeparatorChar + soundName + audioExt;
            if (externalOGG_Copy == 1)
            {
                //if (groupedExport == 1)
                //{
                //    dest = winFolder + "sound\\audio\\" + sound.AudioGroup.Name.Content + "\\" + soundName + audioExt;
                //    MakeFolder("sound\\audio\\" + sound.AudioGroup.Name.Content);
                //}
                MakeFolder("sound" + Path.DirectorySeparatorChar + "audio" + Path.DirectorySeparatorChar);
                System.IO.File.Copy(source, dest, false);
            }
        }
        if (process && !File.Exists(soundFilePath + audioExt))
            File.WriteAllBytes(soundFilePath + audioExt, GetSoundData(sound));

        using (StreamWriter writer = new StreamWriter(ExportFolder + sound.Name.Content + ".sound.gmx"))
        {
            writer.WriteLine(
                "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
            );
            writer.WriteLine("<sound>");
            writer.WriteLine("  <kind>3</kind>");
            writer.WriteLine("  <extension>" + sound.Type.Content + "</extension>");
            writer.WriteLine("  <origname>sound\\audio\\" + sound.File.Content + "</origname>");
            writer.WriteLine("  <effects>" + sound.Effects + "</effects>");
            writer.WriteLine("  <volume>");
            writer.WriteLine(
                "    <volume>"
                + (
                    sound.Volume == 1
                    ? sound.Volume
                    : sound.Volume.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                )
                + "</volume>"
            );
            writer.WriteLine("  </volume>");
            if (sound.Pitch == 0)
            {
                writer.WriteLine("  <pan>0</pan>");
            }
            else
            {
                writer.WriteLine(
                    "  <pan>"
                    + sound.Pitch.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</pan>"
                );
            }
            writer.WriteLine("  <bitRates>");
            writer.WriteLine("    <bitRate>192</bitRate>");
            writer.WriteLine("  </bitRates>");
            writer.WriteLine("  <sampleRates>");
            writer.WriteLine("    <sampleRate>44100</sampleRate>");
            writer.WriteLine("  </sampleRates>");
            writer.WriteLine("  <types>");
            writer.WriteLine("    <type>0</type>");
            writer.WriteLine("  </types>");
            writer.WriteLine("  <bitDepths>");
            writer.WriteLine("    <bitDepth>16</bitDepth>");
            writer.WriteLine("  </bitDepths>");
            writer.WriteLine("  <preload>" + (sound.Preload ? -1 : 0) + "</preload>");
            writer.WriteLine("  <data>" + sound.File.Content + "</data>");
            writer.WriteLine("  <compressed>0</compressed>");
            writer.WriteLine("  <streamed>0</streamed>");
            writer.WriteLine("  <uncompressOnLoad>0</uncompressOnLoad>");
            writer.WriteLine("  <audioGroup>0</audioGroup>");
            writer.WriteLine("</sound>");
        }

    }

    IncrementProgressParallel();
}
