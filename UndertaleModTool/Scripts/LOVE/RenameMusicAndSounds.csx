// Exports the names of assets in a data file in order.
// Made by Grossley and colinator27.

using System.Text;
using System;
using System.IO;

EnsureDataLoaded();

// Get the path, and check for overwriting
string outputPath = Path.Combine(Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar, "LOVE_rename_music_and_sounds.sh");
if (File.Exists(outputPath))
{
    bool overwriteCheck = ScriptQuestion(@"An 'LOVE_rename_music_and_sounds.sh' file already exists. 
Would you like to overwrite it?");
    if (overwriteCheck)
        File.Delete(outputPath);
    else
    {
        ScriptError("An 'LOVE_rename_music_and_sounds.sh' file already exists. Please remove it and try again.", "Error: Export already exists.");
        return;
    }
}

using (StreamWriter writer = new StreamWriter(outputPath))
{
    // Write Sounds.
    writer.WriteLine("#!/usr/bin/bash");
    writer.WriteLine("");
    if (Data.Sounds.Count > 0) 
    {

        for (int i = 0; i < Data.Sounds.Count; i++)
        {
            writer.WriteLine("echo \"Traitement de "+ Data.Sounds[i].Name.Content + "\"");
            writer.WriteLine("find . -type f -name \"*.gml\" | xargs sed -ri 's/scr_playMusic[(]" + i + "[)]/scr_playMusic(" + Data.Sounds[i].Name.Content +")/g'");
            writer.WriteLine("find . -type f -name \"*.yy\" | xargs sed -ri 's/scr_playMusic[(]" + i + "[)]/scr_playMusic(" + Data.Sounds[i].Name.Content + ")/g'");
            writer.WriteLine("find . -type f -name \"*.gml\" | xargs sed -ri 's/scr_playSound[(]" + i + "[)]/scr_playSound(" + Data.Sounds[i].Name.Content + ")/g'");
            writer.WriteLine("find . -type f -name \"*.yy\" | xargs sed -ri 's/scr_playSound[(]" + i + "[)]/scr_playSound(" + Data.Sounds[i].Name.Content + ")/g'");
            writer.WriteLine("");
        }

    }

}
