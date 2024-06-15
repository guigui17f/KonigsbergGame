using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuiGui.Konigsberg;

public class ES2UserType_GuiGuiKonigsbergMapData : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		GuiGui.Konigsberg.MapData data = (GuiGui.Konigsberg.MapData)obj;
		// Add your writer.Write calls here.
		writer.Write(data.Nodes);
		writer.Write(data.Edges);

	}
	
	public override object Read(ES2Reader reader)
	{
		GuiGui.Konigsberg.MapData data = new GuiGui.Konigsberg.MapData();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		GuiGui.Konigsberg.MapData data = (GuiGui.Konigsberg.MapData)c;
		// Add your reader.Read calls here to read the data into the object.
		data.Nodes = reader.ReadArray<System.Single>();
		data.Edges = reader.ReadArray<System.Int32>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_GuiGuiKonigsbergMapData():base(typeof(GuiGui.Konigsberg.MapData)){}
}