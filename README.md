# Litium TextOption Importer

An importer for [Litiums Ecommerce Platform](https://www.litium.com/) to create or update TextOptions fields for all Areas. (*Products, Media, Customers, Websites as of Litium 7*)

## Steps to Install and Use

 1. Copy all files in the Repo into the project. 
	 i. Change namespaces if you need to add them to extension projects.
 2. Add 1 new setting to **appSettings** in the web.config. (And the Test/Prod configs)
     i. <add key="TextOptionImportPath" value="C:\Project\Files\TextOptionImport\" /\>
	 ii. The file will be taken from Litiums Media folder and saved to the files folder to improve read time dramaticly when opening the excel file. 
 3. Build the project.
 4. Create a page in the CMS with the url "Panels/TextOptionImport".  And add describing texts.
	 i. Or change to prefered URL here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12).
 5. Check if the new panel is added in the PIM.
	 i. The panel can be placed under different Areas by changing "ProductArea" here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8).
 6. Upload 1 Excel file (.xlsx) to the media folder "TextOptionImport".
	 i. Litiums Media uploader is used. That way there is no problems with larger files. 
	 ii. The content of the excel file has to look like this (Multiple of the same is OK, it will be ignored): 
	 
    |Key|Value|
    |--|--|
    |Original|Original|
    |ThirdPary|Third Party|
    |Original|Original|
    |Frayed|Well worn|

![Import UI](/TextOptionImporter.PNG)
