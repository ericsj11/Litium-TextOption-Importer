# Litium TextOption Importer

An importer for [Litiums E-Commerce Platform](https://www.litium.com/) to create or update TextOptions fields for all Areas. (*Products, Media, Customers, Websites as of Litium 7*)
![Import UI](/TextOptionImporter.PNG)


## Steps to Install and Use

 1. Copy all files in the Repo into the project. 

    i. Change namespaces if you need to add them to extension projects.
 2. Build the project.
 3. Create a page in the CMS with the url "Panels/TextOptionImport".  And add describing texts.

	 i. Or change to prefered URL here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L12).
 4. Check if the new panel is added in the PIM.

	 i. The panel can be placed under different Areas by changing "ProductArea" here: [https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8](https://github.com/ericsj11/Litium-TextOption-Importer/blob/master/Litium.Accelerator.Mvc/Panels/TextOptionImport.cs#L8).
 5. You can now use the panel. The files can't be to large! (Around 2mb, but it should not be a problem for TextOption, and only 2 columns in the Excel.

	 i. **Important:** The **Key** and **Value** has to be on the first row of the Excel and starts with **Uppercase**!  

	 ii. **Important:** The content of the excel file has to look like this (Multiple of the same is OK, it will be ignored):
	 
    |Key|Value|
    |--|--|
    |Original|Original|
    |ThirdPary|Third Party|
    |Original|Original|
    |Frayed|Well worn|

        iii. Value is the translation and will be put into **en-US** and **sv-SE**. Dynamic support for language will come.
