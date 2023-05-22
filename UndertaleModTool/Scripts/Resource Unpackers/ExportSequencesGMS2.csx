using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

void DumpSequence(UndertaleSequence sequence)
{

    Directory.CreateDirectory(sequencesFolder + sequence.Name.Content);

    using (StreamWriter writer = new StreamWriter(sequencesFolder + sequence.Name.Content + "\\" + sequence.Name.Content + ".yy"))
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
        writer.WriteLine("    {\"resourceType\":\"GMInstanceTrack\",\"resourceVersion\":\"1.0\",\"name\":\"obj_hud_coins\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("          {\"resourceType\":\"Keyframe<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"AssetInstanceKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\"obj_hud_coins\",\"path\":\"objects/obj_hud_coins/obj_hud_coins.yy\",},},},\"Disabled\":false,\"id\":\"8a94dc1f-cb7f-4278-ad63-019475a8a382\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("        ],},\"modifiers\":[],\"trackColour\":4282949618,\"tracks\":[");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"origin\",\"builtinName\":16,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"c36e823e-724b-4d55-ba22-41850acc20ed\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282949618,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"position\",\"builtinName\":14,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":796.755,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":-514.4296,},},\"Disabled\":false,\"id\":\"589e3835-d596-4de5-9415-422bb8b9c65d\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282949618,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"rotation\",\"builtinName\":8,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"a0f77c09-ad85-4ca0-b505-dea7575e66d8\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282949618,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"scale\",\"builtinName\":15,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0992798,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0992798,},},\"Disabled\":false,\"id\":\"711e4102-88c3-444d-9000-ba2289644cff\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282949618,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("      ],\"traits\":0,},");
        writer.WriteLine("    {\"resourceType\":\"GMInstanceTrack\",\"resourceVersion\":\"1.0\",\"name\":\"obj_hud_hearts\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("          {\"resourceType\":\"Keyframe<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"AssetInstanceKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\"obj_hud_hearts\",\"path\":\"objects/obj_hud_hearts/obj_hud_hearts.yy\",},},},\"Disabled\":false,\"id\":\"0f44238e-42ac-4eaa-a908-a4f28f1c359b\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("        ],},\"modifiers\":[],\"trackColour\":4287777010,\"tracks\":[");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"origin\",\"builtinName\":16,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"cf2e5890-28e7-409c-808f-00c6a6f800f3\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4287777010,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"position\",\"builtinName\":14,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":554.39087,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":-511.76477,},},\"Disabled\":false,\"id\":\"1e36e1f0-b0e8-4e42-abe2-0792e879d84b\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4287777010,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"rotation\",\"builtinName\":8,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"027aeb72-a9ab-4caf-86c6-9b921c285466\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4287777010,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"scale\",\"builtinName\":15,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0,},},\"Disabled\":false,\"id\":\"c55b9dc8-acf7-4714-9443-fb573ca77f23\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4287777010,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("      ],\"traits\":0,},");
        writer.WriteLine("    {\"resourceType\":\"GMInstanceTrack\",\"resourceVersion\":\"1.0\",\"name\":\"obj_hud_background\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("          {\"resourceType\":\"Keyframe<AssetInstanceKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"AssetInstanceKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\"obj_hud_background\",\"path\":\"objects/obj_hud_background/obj_hud_background.yy\",},},},\"Disabled\":false,\"id\":\"32696a72-c31a-47f1-8527-5a95e0f60ee9\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":2.0,\"Stretch\":false,},");
        writer.WriteLine("        ],},\"modifiers\":[],\"trackColour\":4282970841,\"tracks\":[");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"origin\",\"builtinName\":16,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"78e45ebd-6cdb-4e73-9b61-961c87a79ffe\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282970841,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"position\",\"builtinName\":14,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":960.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":-540.0,},},\"Disabled\":false,\"id\":\"5f84da29-b241-4e2c-9bf6-43c978350cc1\",\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282970841,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"rotation\",\"builtinName\":8,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":0.0,},},\"Disabled\":false,\"id\":\"7a512dd8-df98-40bc-afab-d7b5ec8978be\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282970841,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("        {\"resourceType\":\"GMRealTrack\",\"resourceVersion\":\"1.0\",\"name\":\"scale\",\"builtinName\":15,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":true,\"keyframes\":{\"resourceType\":\"KeyframeStore<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
        writer.WriteLine("              {\"resourceType\":\"Keyframe<RealKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0,},\"1\":{\"resourceType\":\"RealKeyframe\",\"resourceVersion\":\"1.0\",\"AnimCurveId\":null,\"EmbeddedAnimCurve\":null,\"RealValue\":1.0,},},\"Disabled\":false,\"id\":\"453a30dc-a39f-424e-9169-822939610b89\",\"IsCreationKey\":true,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},");
        writer.WriteLine("            ],},\"modifiers\":[],\"trackColour\":4282970841,\"tracks\":[],\"traits\":0,},");
        writer.WriteLine("      ],\"traits\":0,},");
        writer.WriteLine("  ],");
        writer.WriteLine("  \"visibleRange\": null,");
        writer.WriteLine("  \"volume\": 1.0,");
        writer.WriteLine("  \"xorigin\": -960,");
        writer.WriteLine("  \"yorigin\": -540,");
        writer.WriteLine("}");
    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(sequencesFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}