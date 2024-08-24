using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

EnsureDataLoaded();

string rootFolder = Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));


// Export Project yyp
using (StreamWriter writer = new StreamWriter(rootFolder + "game_object_depth.csv"))
{
    for (int i = 0; i < Data.GameObjects.Count; i++)
    {
        UndertaleGameObject game_object = Data.GameObjects[i];
        writer.WriteLine(game_object.Name.Content + ";" + game_object.Depth);
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + rootFolder);
