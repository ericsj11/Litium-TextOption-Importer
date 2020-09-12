# Litium TextOption Importer

An importer for [Litiums E-Commerce Platform](https://www.litium.com/) to create or update TextOptions fields for all Areas. (Litium 8 compatible)
![Import UI](/TextOptionImporter.PNG)


## Steps to Install and Use

 1. Download [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader) And [EPPlus(Version:4.5.3.3)](https://github.com/JanKallman/EPPlus) with NUGET into the project you place [TextOptionImportController.cs](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Controllers/TextOptionImportController.cs) (Most likley Litium.Accelerator.Mvc)
 2. Copy all files in the Repo into the project. 

    i. Change namespaces if you need to add them to extension projects.
 3. Add this key to your appSettings in web.config ```<add key="TextOptionExportPath" value="C:\Project\Files\TextOptionExport\" />```
 4. Build the project.
 5. Create a page in the CMS with the url "Panels/TextOptionImport".  And add describing texts if needed.

	 i. Or change to prefered URL here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12).
 6. Check if the new panel is added in the PIM.

	 i. The panel can be placed under different Areas by changing "ProductArea" here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8).
 7. You can now use the panel. The files can't be to large! (Around 2mb, but it should not be a problem for Text Options, and only 2 columns in the Excel.)

	 i. Value is the translation and will be put into the cultures **en-US** and **sv-SE**. Dynamic support for language will come.
		 
	 ii. **Important:** The **Key** and **Value** has to be on the first row of the Excel and starts with **Uppercase**!  

	 iii. **Important:** The content of the excel file has to look like one of these 2 (Multiple of the same is OK, multiples will be ignored):
	 
    |Key|Value|
    |--|--|
    |Original|Original|
    |ThirdParty|Third Party|
    |Original|Original|
    |Frayed|Well worn|

    |Key|en-US|sv-SE|
    |--|--|--|
    |Original|Original|Orginal|
    |ThirdParty|Third Party|Tredjepart|
    |Original|Original|Orginal|
    |Frayed|Well worn|Använd|

	Checkout the [Example Excel](/Example-Excel.xlsx)
