# SqlGeometry
Working with SqlGeometry in C#, this repo shows how get the WKT and WKB from JSON array and databases binary data.


The following dependencies are needed

	Install-Package Newtonsoft.Json -Version 12.0.1
	Install-Package Microsoft.SqlServer.Types -Version 14.0.1016.290
  
If you are working in Web Application, you need to add this line in your main method of your Global.asax, if not other instructions are provided when you install the SqlServer.Types package:

	SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

For some reason SqlGeography (and DbGeometry) caused me some troubles, so I ended up using SqlGeometry.
