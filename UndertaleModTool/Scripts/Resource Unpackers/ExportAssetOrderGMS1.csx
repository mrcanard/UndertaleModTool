// Exports the names of assets in a data file in order.
// Made by Grossley and colinator27.

using System.Text;
using System;
using System.IO;

EnsureDataLoaded();

// Get the path, and check for overwriting
string outputPath = Path.Combine(Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar, "asset_names.txt");
if (File.Exists(outputPath))
{
    bool overwriteCheck = ScriptQuestion(@"An 'asset_names.txt' file already exists. 
Would you like to overwrite it?");
    if (overwriteCheck)
        File.Delete(outputPath);
    else
    {
        ScriptError("An 'asset_names.txt' file already exists. Please remove it and try again.", "Error: Export already exists.");
        return;
    }
}

using (StreamWriter writer = new StreamWriter(outputPath))
{

    // Header
    writer.WriteLine("<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->");
    writer.WriteLine("<assets>");
    writer.WriteLine("  <Configs name=\"configs\">");
    writer.WriteLine("    <Config>Configs\\Default</Config>");
    writer.WriteLine("  </Configs>");
    writer.WriteLine("  <NewExtensions/>");

    // Write Sounds.
    writer.WriteLine("  <sounds name=\"sound\">");
    if (Data.Sounds.Count > 0) 
    {
        foreach (UndertaleSound sound in Data.Sounds)
            writer.WriteLine("    <sound>sound\\" + sound.Name.Content + "</sound>");
    }
    writer.WriteLine("  </sounds>");

    // Write Sprites.
    writer.WriteLine("  <sprites name=\"sprites\">");
    if (Data.Sprites.Count > 0) 
    {
        foreach (var sprite in Data.Sprites)
            writer.WriteLine("    <sprite>sprites\\" + sprite.Name.Content + "</sprite>");
    }
    writer.WriteLine("  </sprites>");

    // Write Backgrounds.
    writer.WriteLine("  <backgrounds name=\"background\">");
    if (Data.Backgrounds.Count > 0)
    {
        foreach (var background in Data.Backgrounds)
            writer.WriteLine("    <background>background\\" + background.Name.Content + "</background>");
    }
    writer.WriteLine("  </backgrounds>");

    // Write Paths.
    writer.WriteLine("  <paths name=\"paths\">");
    if (Data.Paths.Count > 0) 
    {
        foreach (UndertalePath path in Data.Paths)
            writer.WriteLine("    <path>paths\\" + path.Name.Content + "</path>");
    }
    writer.WriteLine("  </paths>");

    // Write Scripts.
    writer.WriteLine("  <scripts name=\"scripts\">");
    if (Data.Scripts.Count > 0) 
    {
        foreach (UndertaleScript script in Data.Scripts)
            writer.WriteLine("    <script>scripts\\" + script.Name.Content + ".gml</script>");
    }
    writer.WriteLine("  </scripts>");

    // Write Fonts.
    writer.WriteLine("  <fonts name=\"fonts\">");
    if (Data.Fonts.Count > 0) 
    {
        foreach (UndertaleFont font in Data.Fonts)
            writer.WriteLine("    <font>fonts\\" + font.Name.Content + "</font>");
    }
    writer.WriteLine("  </fonts>");

    // Write Objects.
    writer.WriteLine("  <objects name=\"objects\">");
    if (Data.GameObjects.Count > 0) 
    {
        foreach (UndertaleGameObject gameObject in Data.GameObjects)
            writer.WriteLine("    <object>objects\\" + gameObject.Name.Content + "</object>");
    }
    writer.WriteLine("  </objects>");

    //// Write Timelines.
    //writer.WriteLine("@@timelines@@");
    //if (Data.Timelines.Count > 0)
    //{
    //    foreach (UndertaleTimeline timeline in Data.Timelines)
    //        writer.WriteLine(timeline.Name.Content);
    //}

    // Write Rooms.
    writer.WriteLine("  <rooms name=\"rooms\">");
    if (Data.Rooms.Count > 0)
    {
        foreach (UndertaleRoom room in Data.Rooms)
            writer.WriteLine("    <room>rooms\\" + room.Name.Content + "</room>");
    }
    writer.WriteLine("  </rooms>");

    //// Write Shaders.
    //writer.WriteLine("@@shaders@@");
    //if (Data.Shaders.Count > 0)
    //{
    //    foreach (UndertaleShader shader in Data.Shaders)
    //        writer.WriteLine(shader.Name.Content);
    //}

    //// Write Extensions.
    //writer.WriteLine("@@extensions@@");
    //if (Data.Extensions.Count > 0) 
    //{
    //    foreach (UndertaleExtension extension in Data.Extensions)
    //        writer.WriteLine(extension.Name.Content);
    //}

    // Footer
    writer.WriteLine("  <help>");
    writer.WriteLine("    <rtf>help.rtf</rtf>");
    writer.WriteLine("  </help>");
    writer.WriteLine("  <TutorialState>");
    writer.WriteLine("    <IsTutorial>0</IsTutorial>");
    writer.WriteLine("    <TutorialName></TutorialName>");
    writer.WriteLine("    <TutorialPage>0</TutorialPage>");
    writer.WriteLine("  </TutorialState>");
    writer.WriteLine("</assets>");

    // TODO: Perhaps detect GMS2.3, export those asset names as well.
}
