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
using (StreamWriter writer = new StreamWriter(rootFolder + "projectA.yyp"))
{
    writer.WriteLine("{");
    writer.WriteLine("  \"resourceType\": \"GMProject\",");
    writer.WriteLine("  \"resourceVersion\": \"1.7\",");
    writer.WriteLine("  \"name\": \"projectA\",");
    writer.WriteLine("  \"AudioGroups\": [");
    writer.WriteLine("    {\"resourceType\":\"GMAudioGroup\",\"resourceVersion\":\"1.3\",\"name\":\"audiogroup_default\",\"targets\":-1,},");
    writer.WriteLine("  ],");
    writer.WriteLine("  \"configs\": {");
    writer.WriteLine("    \"children\": [],");
    writer.WriteLine("    \"name\": \""+Data.GeneralInfo.Config.Content+"\",");
    writer.WriteLine("  },");
    writer.WriteLine("  \"defaultScriptType\": 1,");
    writer.WriteLine("  \"Folders\": [");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Extensions\",\"folderPath\":\"folders/Extensions.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Fonts\",\"folderPath\":\"folders/Fonts.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Notes\",\"folderPath\":\"folders/Notes.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Objects\",\"folderPath\":\"folders/Objects.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Paths\",\"folderPath\":\"folders/Paths.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Rooms\",\"folderPath\":\"folders/Rooms.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Scripts\",\"folderPath\":\"folders/Scripts.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"compatibility\",\"folderPath\":\"folders/Scripts/compatibility.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"action\",\"folderPath\":\"folders/Scripts/compatibility/action.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"background\",\"folderPath\":\"folders/Scripts/compatibility/background.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"instance\",\"folderPath\":\"folders/Scripts/compatibility/instance.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"joystick\",\"folderPath\":\"folders/Scripts/compatibility/joystick.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"object\",\"folderPath\":\"folders/Scripts/compatibility/object.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"tile\",\"folderPath\":\"folders/Scripts/compatibility/tile.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"view\",\"folderPath\":\"folders/Scripts/compatibility/view.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Sequences\",\"folderPath\":\"folders/Sequences.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Shaders\",\"folderPath\":\"folders/Shaders.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Sounds\",\"folderPath\":\"folders/Sounds.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Sprites\",\"folderPath\":\"folders/Sprites.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"tilesets\",\"folderPath\":\"folders/Sprites/tilesets.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Tile Sets\",\"folderPath\":\"folders/Tile Sets.yy\",},");
    writer.WriteLine("    {\"resourceType\":\"GMFolder\",\"resourceVersion\":\"1.0\",\"name\":\"Timelines\",\"folderPath\":\"folders/Timelines.yy\",},");
    writer.WriteLine("  ],");
    writer.WriteLine("  \"IncludedFiles\": [],");
    writer.WriteLine("  \"isEcma\": false,");
    writer.WriteLine("  \"LibraryEmitters\": [],");
    writer.WriteLine("  \"MetaData\": {");
    writer.WriteLine("    \"IDEVersion\": \"2023.11.1.129\",");
    writer.WriteLine("  },");
    writer.WriteLine("  \"resources\": [");
    // resssources
    for(int i=0; i < Data.Sprites.Count; i++) {
        var sprite_name = Data.Sprites[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ sprite_name +"\",\"path\":\"sprites/"+ sprite_name +"/"+ sprite_name +".yy\",},},");
    }
    for(int i=0; i < Data.Backgrounds.Count; i++) {
        var background_name = Data.Backgrounds[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ background_name +"\",\"path\":\"sprites/"+ background_name +"/"+ background_name +".yy\",},},");
    }
    // no tilesets
    // sounds
    for(int i=0; i < Data.Sounds.Count; i++) {
        var sound_name = Data.Sounds[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ sound_name +"\",\"path\":\"sounds/"+ sound_name +"/"+ sound_name +".yy\",},},");
    }
    // scripts
    for(int i=0; i < Data.Scripts.Count; i++) {
        var script_name = Data.Scripts[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ script_name +"\",\"path\":\"scripts/"+ script_name +"/"+ script_name +".yy\",},},");
    }
    // fonts
    for(int i=0; i < Data.Fonts.Count; i++) {
        var font_name = Data.Fonts[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ font_name +"\",\"path\":\"fonts/"+ font_name +"/"+ font_name +".yy\",},},");
    }
    // objects
    for(int i=0; i < Data.GameObjects.Count; i++) {
        var game_object_name = Data.GameObjects[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ game_object_name +"\",\"path\":\"objects/"+ game_object_name +"/"+ game_object_name +".yy\",},},");
    }
    // rooms
    for(int i=0; i < Data.Rooms.Count; i++) {
        var room_name = Data.Rooms[i].Name.Content;
        writer.WriteLine("    {\"id\":{\"name\":\""+ room_name +"\",\"path\":\"rooms/"+ room_name +"/"+ room_name +".yy\",},},");
    }
    // end resources
    writer.WriteLine("  ],");
    writer.WriteLine("  \"RoomOrderNodes\": [");
    // RoomOrderNodes
    for(int i=0; i < Data.GeneralInfo.RoomOrder.Count; i++) {
        var room_name = Data.GeneralInfo.RoomOrder[i].Name.Content;
        writer.WriteLine("    {\"roomId\":{\"name\":\""+room_name+"\",\"path\":\"rooms/"+room_name+"/"+room_name+".yy\",},},");
    }
    // end RoomOrderNodes
    writer.WriteLine("  ],");
    writer.WriteLine("  \"templateType\": null,");
    writer.WriteLine("  \"TextureGroups\": [");
    writer.WriteLine("    {\"resourceType\":\"GMTextureGroup\",\"resourceVersion\":\"1.3\",\"name\":\"Default\",\"autocrop\":true,\"border\":2,\"compressFormat\":\"bz2\",\"directory\":\"\",\"groupParent\":null,\"isScaled\":false,\"loadType\":\"default\",\"mipsToGenerate\":0,\"targets\":-1,},");
    // for(int i=0; i < Data.TextureGroupInfo.Count; i++) {
    //     var tgroup = Data.TextureGroupInfo[i];
    //     writer.WriteLine("    {\"resourceType\":\"GMTextureGroup\",\"resourceVersion\":\"1.3\",\"name\":\""+tgroup.Name.Content+"\",\"autocrop\":true,\"border\":2,\"compressFormat\":\"bz2\",\"directory\":\"\",\"groupParent\":null,\"isScaled\":false,\"loadType\":\"default\",\"mipsToGenerate\":0,\"targets\":-1,},");
    // }
    writer.WriteLine("  ],");
    writer.WriteLine("}");

}

// Export Project Resource Order
using (StreamWriter writer = new StreamWriter(rootFolder + "projectA.resource_order"))
{
    writer.WriteLine("{");
    writer.WriteLine("  \"FolderOrderSettings\": [");
    writer.WriteLine("    {\"name\":\"Extensions\",\"order\":14,\"path\":\"folders/Extensions.yy\",},");
    writer.WriteLine("    {\"name\":\"Fonts\",\"order\":8,\"path\":\"folders/Fonts.yy\",},");
    writer.WriteLine("    {\"name\":\"Notes\",\"order\":13,\"path\":\"folders/Notes.yy\",},");
    writer.WriteLine("    {\"name\":\"Objects\",\"order\":11,\"path\":\"folders/Objects.yy\",},");
    writer.WriteLine("    {\"name\":\"Paths\",\"order\":5,\"path\":\"folders/Paths.yy\",},");
    writer.WriteLine("    {\"name\":\"Rooms\",\"order\":12,\"path\":\"folders/Rooms.yy\",},");
    writer.WriteLine("    {\"name\":\"Scripts\",\"order\":6,\"path\":\"folders/Scripts.yy\",},");
    writer.WriteLine("    {\"name\":\"compatibility\",\"order\":173,\"path\":\"folders/Scripts/compatibility.yy\",},");
    writer.WriteLine("    {\"name\":\"background\",\"order\":1,\"path\":\"folders/Scripts/compatibility/background.yy\",},");
    writer.WriteLine("    {\"name\":\"instance\",\"order\":2,\"path\":\"folders/Scripts/compatibility/instance.yy\",},");
    writer.WriteLine("    {\"name\":\"joystick\",\"order\":3,\"path\":\"folders/Scripts/compatibility/joystick.yy\",},");
    writer.WriteLine("    {\"name\":\"object\",\"order\":4,\"path\":\"folders/Scripts/compatibility/object.yy\",},");
    writer.WriteLine("    {\"name\":\"tile\",\"order\":5,\"path\":\"folders/Scripts/compatibility/tile.yy\",},");
    writer.WriteLine("    {\"name\":\"view\",\"order\":6,\"path\":\"folders/Scripts/compatibility/view.yy\",},");
    writer.WriteLine("    {\"name\":\"Sequences\",\"order\":10,\"path\":\"folders/Sequences.yy\",},");
    writer.WriteLine("    {\"name\":\"Shaders\",\"order\":7,\"path\":\"folders/Shaders.yy\",},");
    writer.WriteLine("    {\"name\":\"Sounds\",\"order\":4,\"path\":\"folders/Sounds.yy\",},");
    writer.WriteLine("    {\"name\":\"tilesets\",\"order\":2472,\"path\":\"folders/Sprites/tilesets.yy\",},");
    writer.WriteLine("    {\"name\":\"Tile Sets\",\"order\":3,\"path\":\"folders/Tile Sets.yy\",},");
    writer.WriteLine("    {\"name\":\"Timelines\",\"order\":9,\"path\":\"folders/Timelines.yy\",},");
    writer.WriteLine("  ],");
    writer.WriteLine("  \"ResourceOrderSettings\": [");
    // counter
    var order = 0;
    // sprites
    order = 0;
    for(int i=0; i < Data.Sprites.Count; i++) {
        order++;
        var sprite_name = Data.Sprites[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ sprite_name +"\",\"order\":"+order+",\"path\":\"sprites/"+sprite_name+"/"+sprite_name+".yy\",},");
    }
    for(int i=0; i < Data.Backgrounds.Count; i++) {
        order++;
        var background_name = Data.Backgrounds[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ background_name +"\",\"order\":"+order+",\"path\":\"sprites/"+background_name+"/"+background_name+".yy\",},");
    }
    // no tilesets
    // sounds
    order = 0;
    for(int i=0; i < Data.Sounds.Count; i++) {
        order++;
        var sound_name = Data.Sounds[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ sound_name +"\",\"order\":"+order+",\"path\":\"sounds/"+sound_name+"/"+sound_name+".yy\",},");
    }
    // scripts
    order = 0;
    for(int i=0; i < Data.Scripts.Count; i++) {
        order++;
        var script_name = Data.Scripts[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ script_name +"\",\"order\":"+order+",\"path\":\"scripts/"+script_name+"/"+script_name+".yy\",},");
    }
    // fonts
    order = 0;
    for(int i=0; i < Data.Fonts.Count; i++) {
        order++;
        var font_name = Data.Fonts[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ font_name +"\",\"order\":"+order+",\"path\":\"fonts/"+font_name+"/"+font_name+".yy\",},");
    }
    // objects
    order = 0;
    for(int i=0; i < Data.GameObjects.Count; i++) {
        order++;
        var game_object_name = Data.GameObjects[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ game_object_name +"\",\"order\":"+order+",\"path\":\"objects/"+game_object_name+"/"+game_object_name+".yy\",},");
    }
    // rooms
    order = 0;
    for(int i=0; i < Data.Rooms.Count; i++) {
        order++;
        var room_name = Data.Rooms[i].Name.Content;
        writer.WriteLine("        {\"name\":\""+ room_name +"\",\"order\":"+order+",\"path\":\"rooms/"+room_name+"/"+room_name+".yy\",},");
    }
    // footer
    writer.WriteLine("  ],");
    writer.WriteLine("}");
}

ScriptMessage("Export Complete.\n\nLocation: " + rootFolder);
