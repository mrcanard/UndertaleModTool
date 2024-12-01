// Made by mono21400

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
    System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

string shaderPaths = GetFolder(FilePath) + "shaders" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();

if (Directory.Exists(shaderPaths))
{
    Directory.Delete(shaderPaths, true);
}

Directory.CreateDirectory(shaderPaths);

SetProgressBar(null, "Shaders", 0, Data.Shaders.Count);
StartProgressBarUpdater();

await DumpShaders();

// worker.Cleanup();

await StopProgressBarUpdater();
HideProgressBar();

// Export asset
if(Data.Shaders.Count > 0) {
    using (StreamWriter writer = new StreamWriter(shaderPaths + "asset_order.txt"))
    {
        for (int i = 0; i < Data.Shaders.Count; i++)
        {
            UndertaleShader shader = Data.Shaders[i];
            if(!(shader.Name.Content.StartsWith("_"))) {
                writer.WriteLine(
                    "    {\"id\":{\"name\":\""
                    + shader.Name.Content
                    + "\",\"path\":\"shaders/"
                    + shader.Name.Content
                    + "/"
                    + shader.Name.Content
                    + ".yy\",},},"
                );
            }
        }
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + shaderPaths);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpShaders()
{
    await Task.Run(() => Parallel.ForEach(Data.Shaders, DumpShader));
}

void extractShaderString(String shader_path, String shader_content)
{
    bool flag_defined = false;
    string vertex_final_string = "";
    foreach (var myString in shader_content.Split("\n"))
    {
        if (flag_defined)
        {
            vertex_final_string += myString;
            vertex_final_string += "\n";
        }
        if (myString.Contains("#define _YY_GLSLES_ 1"))
        {
            flag_defined = true;
        }
    }
    File.WriteAllText(shader_path, vertex_final_string);
}

void DumpShader(UndertaleShader shader)
{
    if(!(shader.Name.Content.StartsWith("_"))) {
        Directory.CreateDirectory(shaderPaths + shader.Name.Content);

        using (
            StreamWriter writer = new StreamWriter(
                shaderPaths
                + shader.Name.Content
                + Path.DirectorySeparatorChar
                + shader.Name.Content
                + ".yy"
            )
        )
              {
                  writer.WriteLine("{");
                  writer.WriteLine("  \"resourceType\": \"GMShader\",");
                  writer.WriteLine("  \"resourceVersion\": \"1.0\",");
                  writer.WriteLine("  \"name\": \"" + shader.Name.Content + "\",");
                  writer.WriteLine("  \"parent\": {");
                  writer.WriteLine("    \"name\": \"Shaders\",");
                  writer.WriteLine("    \"path\": \"folders/Shaders.yy\",");
                  writer.WriteLine("  },");
                  writer.WriteLine("  \"type\": " + (int)shader.Type + ",");
                  writer.WriteLine("}");
              }

        switch (shader.Type)
        {
            case UndertaleShader.ShaderType.GLSL_ES:
                // Vertex Source
                string path_vsh =
                    shaderPaths
                    + shader.Name.Content
                    + Path.DirectorySeparatorChar
                    + shader.Name.Content
                    + ".vsh";

                extractShaderString(path_vsh, shader.GLSL_ES_Vertex.Content);

                // Fragment Source
                string path_fsh =
                    shaderPaths
                    + shader.Name.Content
                    + Path.DirectorySeparatorChar
                    + shader.Name.Content
                    + ".fsh";

                extractShaderString(path_fsh, shader.GLSL_ES_Fragment.Content);
                break;

            default:
                throw new InvalidOperationException("Type de shader non supporté");
        }
        
    }


    IncrementProgressParallel();
}
