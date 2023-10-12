// Exports the names of assets in a data file in order.
// Made by Grossley and colinator27.

using System.Text;
using System;
using System.IO;

EnsureDataLoaded();

// Get the path, and check for overwriting
string outputPath = Path.Combine(Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar, "LOVE_rename_music_and_sounds.py");
if (File.Exists(outputPath))
{
    bool overwriteCheck = ScriptQuestion(@"An 'LOVE_rename_music_and_sounds.py' file already exists. 
Would you like to overwrite it?");
    if (overwriteCheck)
        File.Delete(outputPath);
    else
    {
        ScriptError("An 'LOVE_rename_music_and_sounds.py' file already exists. Please remove it and try again.", "Error: Export already exists.");
        return;
    }
}

using (StreamWriter writer = new StreamWriter(outputPath))
{
    // Write Sounds.

    writer.WriteLine("import os");
    writer.WriteLine("import fileinput");
    writer.WriteLine("");
    writer.WriteLine("# Récupération des fichiers intéressants");
    writer.WriteLine("file_list = set()");
    writer.WriteLine("for root,dirs,files in os.walk(\".\"):");
    writer.WriteLine("	if root not in [\".git\"]:");
    writer.WriteLine("		for filename in files:");
    writer.WriteLine("			if filename.endswith(\".yy\") or filename.endswith(\".gml\"):");
    writer.WriteLine("				file_list.add(os.path.join(root, filename))");
    writer.WriteLine("");
    writer.WriteLine("# Table de correspondance");
    writer.WriteLine("replace_str = {");
    if (Data.Sounds.Count > 0)
    {
        for (int i = 0; i < Data.Sounds.Count; i++)
        {
            writer.WriteLine("	\""+ i +"\" : \""+ Data.Sounds[i].Name.Content +"\",");
        }

    }
    writer.WriteLine("}");
    writer.WriteLine("");
    writer.WriteLine("for line in fileinput.input(files=file_list, inplace=True):");
    writer.WriteLine("	if \"scr_playMusic\" in line or \"scr_playSound\" in line:");
    writer.WriteLine("		for key, value in replace_str.items():");
    writer.WriteLine("			line = line.replace(\"scr_playSound({})\".format(key), \"scr_playSound({})\".format(value))");
    writer.WriteLine("			line = line.replace(\"scr_playMusic({})\".format(key), \"scr_playMusic({})\".format(value))");
    writer.WriteLine("		print(line.rstrip())");
    writer.WriteLine("	else:");
    writer.WriteLine("		print(line.rstrip())");
    writer.WriteLine("");



}
