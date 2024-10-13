using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UndertaleModLib.Models;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
//

string sequencesFolder = GetFolder(FilePath) + "sequences" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));
if (Directory.Exists(sequencesFolder))
{
    Directory.Delete(sequencesFolder, true);
}

Directory.CreateDirectory(sequencesFolder);

bool exportFromCache = false;
// if (GMLCacheEnabled && Data.GMLCache is not null)
//     exportFromCache = ScriptQuestion("Export from the cache?");

List<UndertaleSequence> toDump;
if (!exportFromCache)
{
    toDump = new();
    foreach (UndertaleSequence sequence in Data.Sequences)
    {
        toDump.Add(sequence);
    }
}

SetProgressBar(null, "Sequence Entries", 0, toDump.Count);
StartProgressBarUpdater();

await DumpSequences();

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + sequencesFolder);


string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}


async Task DumpSequences()
{

    if (Data.KnownSubFunctions is null) //if we run script before opening any code
        Decompiler.BuildSubFunctionCache(Data);

    await Task.Run(() => Parallel.ForEach(toDump, DumpSequence));
}

void DumpTracks(StreamWriter writer,UndertaleModLib.UndertaleSimpleList<UndertaleSequence.Track> tracks, int tabnum = 2) {
    String mytab = new String('\t', tabnum);

        foreach(var track in tracks) {
          switch(track.ModelName.Content)
          {
            case "GMGraphicTrack":
              writer.WriteLine(track.ModelName.Content); // GMGraphicTrack
              writer.WriteLine(track.Name.Content); // spr_arc_long
              writer.WriteLine(track.BuiltinName); // 0
              writer.WriteLine(track.Traits); // None
              writer.WriteLine(track.IsCreationTrack); // False
              writer.WriteLine(track.Tags.Count); // 0
              writer.WriteLine(track.OwnedResources.Count); // 0
              writer.WriteLine(track.Tracks.Count); // 5
              writer.WriteLine(mytab + "{\"resourceType\":\"GMGraphicTrack\",\"resourceVersion\":\"1.0\",\"name\":\"spr_arc_long\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<AssetSpriteKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
              // Keyframes
              writer.WriteLine(mytab + "],},\"modifiers\":[],\"trackColour\":4292102386,\"tracks\":[");
              // Tracks
              writer.WriteLine(mytab + "],\"traits\":0,}");
              break;

            default:
              writer.WriteLine(track.ModelName.Content);
              break;
          }
        }
 
}

void DumpSequence(UndertaleSequence sequence)
{

    Directory.CreateDirectory(sequencesFolder + sequence.Name.Content);

    using (StreamWriter writer = new StreamWriter(sequencesFolder + sequence.Name.Content + Path.DirectorySeparatorChar + sequence.Name.Content + ".yy"))
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMSequence\",");
        writer.WriteLine("  \"resourceVersion\": \"1.4\",");
        writer.WriteLine("  \"name\": \""+sequence.Name.Content+"\",");
        writer.WriteLine("  \"autoRecord\": true,");
        writer.WriteLine("  \"backdropHeight\": 1080,");
        writer.WriteLine("  \"backdropImageOpacity\": 0.5,");
        writer.WriteLine("  \"backdropImagePath\": \"\",");
        writer.WriteLine("  \"backdropWidth\": 1920,");
        writer.WriteLine("  \"backdropXOffset\": 0.0,");
        writer.WriteLine("  \"backdropYOffset\": 0.0,");
        writer.WriteLine("  \"events\": {");
        writer.WriteLine("    \"resourceType\": \"KeyframeStore<MessageEventKeyframe>\",");
        writer.WriteLine("    \"resourceVersion\": \"1.0\",");
        writer.WriteLine("    \"Keyframes\": [],");
        writer.WriteLine("  },");
        writer.WriteLine("  \"eventStubScript\": null,");
        writer.WriteLine("  \"eventToFunction\": {},");
        writer.WriteLine("  \"length\": "+sequence.Length.ToString("0.0")+",");
        writer.WriteLine("  \"lockOrigin\": false,");
        writer.WriteLine("  \"moments\": {");
        writer.WriteLine("    \"resourceType\": \"KeyframeStore<MomentsEventKeyframe>\",");
        writer.WriteLine("    \"resourceVersion\": \"1.0\",");
        writer.WriteLine("    \"Keyframes\": [],");
        writer.WriteLine("  },");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Sequences\",");
        writer.WriteLine("    \"path\": \"folders/Sequences.yy\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"playback\": "+(int) sequence.Playback+",");
        writer.WriteLine("  \"playbackSpeed\": "+sequence.PlaybackSpeed.ToString("0.0")+",");
        writer.WriteLine("  \"playbackSpeedType\": "+(int) sequence.PlaybackSpeedType+",");
        writer.WriteLine("  \"showBackdrop\": true,");
        writer.WriteLine("  \"showBackdropImage\": false,");
        writer.WriteLine("  \"spriteId\": null,");
        writer.WriteLine("  \"timeUnits\": 1,");
        writer.WriteLine("  \"tracks\": [");

        DumpTracks(writer, sequence.Tracks); 

        writer.WriteLine("  ],");
        writer.WriteLine("  \"visibleRange\": null,");
        writer.WriteLine("  \"volume\": 1.0,");
        writer.WriteLine("  \"xorigin\": "+ sequence.OriginX +",");
        writer.WriteLine("  \"yorigin\": "+ sequence.OriginY +",");
        writer.Write("}");
    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(sequencesFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}
