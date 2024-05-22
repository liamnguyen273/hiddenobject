class FileDef {
	path: string;
	name: string;
	extension: string;

	public getFullPath(): string { return this.path + "/" + this.name + "." + this.extension; }

	constructor(path: string, name: string, extension: string)
	{
		this.path = path;
		this.name = name;
		this.extension = extension;
	}

	public clone(): FileDef
	{
		return FileDef.getFromOther(this);
	}

	public static getFromOther(other: FileDef): FileDef
	{
		var def = new FileDef(other.path, other.name, other.extension);
		return def
	}

	public static getFromDoc(doc: Document)
	{
		var filePath = doc.path.toString();
		var fileName = doc.name.toString();

		var lastDot = fileName.lastIndexOf(".");
		if (lastDot == -1) {
			lastDot = fileName.length;
		}

		var fileExtension = fileName.substr(lastDot + 1);
		var fileName = fileName.substr(0, lastDot);

		return new FileDef(filePath, fileName, fileExtension);
	}
}

function tryBackup(doc: Document)
{
	var today = new Date();
	var def = FileDef.getFromDoc(doc);
	def.path = def.path + "/BAK"
	def.name = def.name + "_BAK";
	let file = new File(def.getFullPath());
	doc.saveAs(file, undefined, true);

	// let backupWindow = new Window("dialog", `Back up document "${def.name}"?`);
	// backupWindow.size = { width: 300, height: 100 };
	// backupWindow.alignment = ["fill", "fill"];
	// let btnYes = backupWindow.add("button", undefined, "Yes", {name: "ok"});
	// let btnNo = backupWindow.add("button", undefined, "No", {name: "cancel"});

	// btnYes.onClick = function () 
	// {
	// 	// def.name = def.name + "_" + formatDateForFilename(today);
	// 	def.name = def.name + "_BAK";

	// 	backupWindow.close();

	// 	continueFunction && continueFunction(doc);
	// };

	// btnNo.onClick = function ()
	// {
	// 	backupWindow.close();
	// 	continueFunction && continueFunction(doc);
	// };

	// backupWindow.show();
}

function exportFiles(doc: Document): void
{
	let date = new Date();
	var def = FileDef.getFromDoc(doc);
	let levelName = def.name;

	// Folders
	let exportFolder = new Folder(def.path + "/export");
	if (!exportFolder.exists) exportFolder.create();

	let fullFolder = new Folder(def.path + "/export/full_images");
	let thumbFolder = new Folder(def.path + "/export/thumbnails");
	let csvFolder = new Folder(def.path + "/export/csvs");
	let stickersFolder = new Folder(def.path + "/export/stickers");
	let levelStickersFolder = new Folder(def.path + "/export/stickers/" + levelName);

	if (!fullFolder.exists) fullFolder.create();
	if (!thumbFolder.exists) thumbFolder.create();
	if (!csvFolder.exists) csvFolder.create();
	if (!stickersFolder.exists) stickersFolder.create();
	if (!levelStickersFolder.exists)
	{
		levelStickersFolder.create();
	}
	else
	{
		deleteFolder(levelStickersFolder);
	}

	// Paths
	let fullPath = new FileDef(def.path + "/export/full_images", levelName, "png");
	let thumbPath = new FileDef(def.path + "/export/thumbnails", levelName, "png");
	let csvPath = new FileDef(def.path + "/export/csvs", levelName, "csv");
	let levelStickersPath = new FileDef(def.path + "/export/stickers/" + levelName, "", "");

	let logDef = new FileDef(def.path + "/export", "export_log", "txt");

	let lf = new File(logDef.getFullPath());
	lf.open("w", "TEXT", "????");
	lf.seek(0, 2);
	lf.writeln(`===== ${date.toLocaleString()} =====`);

	let groups = doc.layerSets;

	let dW = doc.width instanceof UnitValue ? doc.width.value : doc.width as number;
	let dH = doc.height instanceof UnitValue ? doc.height.value : doc.height as number;

	lf.writeln(`Document ${doc.name} size is: ` + dW.toString() + " x " + dH.toString());
	let e = `Document ${doc.name} has size (${dW} x ${dH}).`;
	lf.writeln(e);

	try
	{
		let otherSets = [];
		let otherLayers = [];

		let backOutlineSet: LayerSet = undefined;
		let stickerOutlineSet: LayerSet = undefined;
		let backColorSet: LayerSet = undefined;
		let stickerColorSet: LayerSet = undefined;

		let decorColorSet: LayerSet = undefined;
		let decorOutlineSet: LayerSet = undefined;

		for (let i = 0; i < doc.layerSets.length; ++i)
		{
			let set = doc.layerSets[i];

			let setName = set.name.replace(/^\s*/, "").replace(/\s*$/, "");

			if (setName === "back_outline") {
				backOutlineSet = set;
				lf.writeln("Found layer \"back_outline\"");
			}
			if (setName === "stickers_outline") {
				stickerOutlineSet = set;
				lf.writeln("Found layer \"stickers_outline\"");
			}
			if (setName === "back_color") {
				backColorSet = set;
				lf.writeln("Found layer \"back_color\"");
			}
			if (setName === "stickers_color") {
				stickerColorSet = set;
				lf.writeln("Found layer \"stickers_color\"");
			}

			if (setName === "decor_color")
			{
				decorColorSet = set;
			}
			else if (setName === "decor_outline")
			{
				decorOutlineSet = set;
			}
			else
			{
				otherSets.push(set);
			}
		}

		if (!backColorSet || !stickerOutlineSet || !backColorSet || !stickerColorSet)
		{
			throw new Error("Missing imporant layers.");
		}

		for (let i = 0; i < doc.layers.length; ++i)
		{
			let layer = doc.layers[i];
			otherLayers.push(layer);
		}

		setLayerSetsVisibleState(otherSets, false);
		setLayersVisibleState(otherLayers, false);


		lf.writeln("Now exporting outline image...");
		saveHistory(doc);

		setLayerSetVisibleState(backOutlineSet, true);
		setLayerSetVisibleState(stickerOutlineSet, true);
		setLayerSetVisibleState(backColorSet, false);
		setLayerSetVisibleState(stickerColorSet, false);
		if (decorOutlineSet !== undefined) setLayerSetVisibleState(decorOutlineSet, true);
		if (decorColorSet !== undefined) setLayerSetVisibleState(decorColorSet, false);

		let outlineFile = new File(thumbPath.getFullPath());
		let outlineOptions = new PNGSaveOptions();
		doc.saveAs(outlineFile, outlineOptions, true);

		loadHistory(doc);



		lf.writeln("Now exporting completed image...");
		saveHistory(doc);

		setLayerSetVisibleState(backOutlineSet, false);
		setLayerSetVisibleState(stickerOutlineSet, false);
		setLayerSetVisibleState(backColorSet, true);
		setLayerSetVisibleState(stickerColorSet, true);
		if (decorOutlineSet !== undefined) setLayerSetVisibleState(decorOutlineSet, false);
		if (decorColorSet !== undefined) setLayerSetVisibleState(decorColorSet, true);


		let coloredFile = new File(fullPath.getFullPath());
		let coloredOptions = new PNGSaveOptions();
		doc.saveAs(coloredFile, coloredOptions, true);

		loadHistory(doc);


		lf.writeln("Now exporting stickers...");

		setLayerSetVisibleState(backOutlineSet, false);
		setLayerSetVisibleState(stickerOutlineSet, false);
		setLayerSetVisibleState(backColorSet, false);
		setLayerSetVisibleState(stickerColorSet, true);
		if (decorOutlineSet !== undefined) setLayerSetVisibleState(decorOutlineSet, false);
		if (decorColorSet !== undefined) setLayerSetVisibleState(decorColorSet, false);

		setLayersOfSetVisibleState(stickerColorSet, false);

		let csvContent = "name,x,y";
		let lastLayer: Layer | undefined = undefined;
		let i = 0;

		let nameMap = {};

		for (i = 0; i < stickerColorSet.layers.length; ++i)
		{
			// Make visible
			if (lastLayer !== undefined) lastLayer.visible = false;
			let layer = stickerColorSet.layers[i];
			layer.visible = true;

			let stickerName = layer.name.replace(/^\s*/, "").replace(/\s*$/, "");
			while (nameMap.hasOwnProperty(stickerName))
			{
				var conflictedName = stickerName;
				var count = nameMap[conflictedName];
				nameMap[conflictedName] = count + 1;
				stickerName += ` (${count + 1})`;
			}

			nameMap[stickerName] = 1;

			let rect = layer.bounds;

			// Calculation
			let x1 = (rect[0] instanceof UnitValue ? rect[0].value : rect[0] as number);
			let y1 = (rect[1] instanceof UnitValue ? rect[1].value : rect[1] as number);
			let x2 = (rect[2] instanceof UnitValue ? rect[2].value : rect[2] as number);
			let y2 = (rect[3] instanceof UnitValue ? rect[3].value : rect[3] as number);

			let dm = dW + " " + dH + " " + x1 + " " + x2 + " " + y1 + " " + y2;
			let [nX, nY] = getNormalizedCoordinates(lf, dW, dH, x1, x2, y1, y2);
			let line = `${stickerName},${nX.toFixed(4)},${nY.toFixed(4)}`;
			csvContent += "\n" + line;

			// Save layer
			let layerDef = levelStickersPath.clone();
			layerDef.name = stickerName;
			let layerFile = new File(layerDef.getFullPath());

			saveHistory(doc);
			doc.trim();
			let trimW = doc.width instanceof UnitValue ? doc.width.value : doc.width as number;
			let trimH = doc.height instanceof UnitValue ? doc.height.value : doc.height as number;
			doc.resizeCanvas(trimW + 16, trimH + 16, AnchorPosition.MIDDLECENTER);
			let options = new PNGSaveOptions();
			doc.saveAs(layerFile, options, true);
			loadHistory(doc);

			lastLayer = layer;

			lf.writeln(`Sticker number ${i} (named "${stickerName}", [${dm}])} exported")`);
		}

		// Save csv
		lf.writeln(`Document ${doc.name} produced ${i} stickers.`)

		let csvFile = new File(csvPath.getFullPath());
		csvFile.open("w", "TEXT", "????");
		csvFile.write(csvContent);
		csvFile.close();

		setLayersOfSetVisibleState(stickerColorSet, true);
		setLayerSetVisibleState(backOutlineSet, true);
		setLayerSetVisibleState(stickerOutlineSet, true);
		setLayerSetVisibleState(backColorSet, true);
		setLayerSetVisibleState(stickerColorSet, true);
		if (decorOutlineSet !== undefined) setLayerSetVisibleState(decorOutlineSet, true);
		if (decorColorSet !== undefined) setLayerSetVisibleState(decorColorSet, true);

		lf.writeln("Cleaning up...");

		// alert("Completed.");
	}
	catch (e: any)
	{
		let msg = "Excecution failed: " + e.toString();
		lf.writeln(msg);
		msg += "\nSee \"log.txt\" for details.";
		alert(e);
	}

	lf.close();
}

function setLayersOfSetVisibleState(set: LayerSet, visible: boolean)
{
	for (let i = 0; i < set.layers.length; ++i)
	{
		let layer = set.layers[i];
		layer.visible = visible;
	}
}

function setLayerSetVisibleState(set: LayerSet, visible: boolean)
{
	set.visible = visible;
}

function setLayerSetsVisibleState(sets: LayerSet[], visible: boolean)
{
	for (let i = 0; i < sets.length; ++i)
	{
		let set = sets[i];
		set.visible = visible;
	}
}

function setLayersVisibleState(layers: Layer[], visible)
{
	for (let i = 0; i < layers.length; ++i) {
		let layer = layers[i];
		layer.visible = visible;
	}
}


function formatDateForFilename(date: Date): string {
	return date.getFullYear()
		+ "_" + (date.getMonth() < 10 ? "0" + date.getMonth() : date.getMonth())
		+ "_" + (date.getDate() < 10 ? "0" + date.getDate() : date.getDate())
		+ "_" + (date.getHours() < 10 ? "0" + date.getHours() : date.getHours())
		+ "_" + (date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes())
		+ "_" + (date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds())
}

function getNormalizedCoordinates(lf: File, dW: number, dH: number, x1: number, x2: number, y1: number, y2: number): [number, number]
{
	if (x1 < x2)
	{
		let temp = x1;
		x1 = x2;
		x2 = temp;
	}

	if (y1 < y2)
	{
		let temp = y1;
		y1 = y2;
		y2 = temp;
	}

	let cDX = dW / 2;
	let cDY = dH / 2;

	let cX = x1 + (x2 - x1) / 2;
	let cY = y1 + (y2 - y1) / 2;

	let tX = cX - cDX;
	let tY = cDY - cY;
	return [tX / cDX, tY / cDY];
}


var masterHistory: HistoryState | undefined;
var history: HistoryState | undefined;

function saveHistory(doc: Document)
{
	history = doc.activeHistoryState;
}

function saveMasterHistory(doc: Document)
{
	masterHistory = doc.activeHistoryState;
}

function loadHistory(doc: Document)
{
	if (history)
	{
		doc.activeHistoryState = history;
	}
}

function loadMasterHistory(doc: Document)
{
	if (masterHistory)
	{
		doc.activeHistoryState = masterHistory;
	}
}

function clearHistoryCaches()
{
	masterHistory = undefined;
	history = undefined;
}

function deleteFolder(mFolder: Folder)
{
	var fileList = mFolder.getFiles();
	for (var i = 0; i < fileList.length; i++)
	{
		var file = fileList[i];
		if (file instanceof File)
		{
			let fName = new File(file.fullName);
			fName.remove();
		}
	}
}

for (var i = 0; i < app.documents.length; i++)
{
	var doc = app.documents[i];
	app.activeDocument = doc;

	clearHistoryCaches();
	tryBackup(doc);
	exportFiles(doc);
}

alert("Completed.");