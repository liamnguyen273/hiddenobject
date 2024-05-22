var FileDef = /** @class */ (function () {
    function FileDef(path, name, extension) {
        this.path = path;
        this.name = name;
        this.extension = extension;
    }
    FileDef.prototype.getFullPath = function () { return this.path + "/" + this.name + "." + this.extension; };
    FileDef.prototype.clone = function () {
        return FileDef.getFromOther(this);
    };
    FileDef.getFromOther = function (other) {
        var def = new FileDef(other.path, other.name, other.extension);
        return def;
    };
    FileDef.getFromDoc = function (doc) {
        var filePath = doc.path.toString();
        var fileName = doc.name.toString();
        var lastDot = fileName.lastIndexOf(".");
        if (lastDot == -1) {
            lastDot = fileName.length;
        }
        var fileExtension = fileName.substr(lastDot + 1);
        var fileName = fileName.substr(0, lastDot);
        return new FileDef(filePath, fileName, fileExtension);
    };
    return FileDef;
}());
function tryBackup(doc) {
    var today = new Date();
    var def = FileDef.getFromDoc(doc);
    def.path = def.path + "/BAK";
    def.name = def.name + "_BAK";
    var file = new File(def.getFullPath());
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
function exportFiles(doc) {
    var date = new Date();
    var def = FileDef.getFromDoc(doc);
    var levelName = def.name;
    // Folders
    var exportFolder = new Folder(def.path + "/export");
    if (!exportFolder.exists)
        exportFolder.create();
    var fullFolder = new Folder(def.path + "/export/full_images");
    var thumbFolder = new Folder(def.path + "/export/thumbnails");
    var csvFolder = new Folder(def.path + "/export/csvs");
    var stickersFolder = new Folder(def.path + "/export/stickers");
    var levelStickersFolder = new Folder(def.path + "/export/stickers/" + levelName);
    if (!fullFolder.exists)
        fullFolder.create();
    if (!thumbFolder.exists)
        thumbFolder.create();
    if (!csvFolder.exists)
        csvFolder.create();
    if (!stickersFolder.exists)
        stickersFolder.create();
    if (!levelStickersFolder.exists) {
        levelStickersFolder.create();
    }
    else {
        deleteFolder(levelStickersFolder);
    }
    // Paths
    var fullPath = new FileDef(def.path + "/export/full_images", levelName, "png");
    var thumbPath = new FileDef(def.path + "/export/thumbnails", levelName, "png");
    var csvPath = new FileDef(def.path + "/export/csvs", levelName, "csv");
    var levelStickersPath = new FileDef(def.path + "/export/stickers/" + levelName, "", "");
    var logDef = new FileDef(def.path + "/export", "export_log", "txt");
    var lf = new File(logDef.getFullPath());
    lf.open("w", "TEXT", "????");
    lf.seek(0, 2);
    lf.writeln("===== ".concat(date.toLocaleString(), " ====="));
    var groups = doc.layerSets;
    var dW = doc.width instanceof UnitValue ? doc.width.value : doc.width;
    var dH = doc.height instanceof UnitValue ? doc.height.value : doc.height;
    lf.writeln("Document ".concat(doc.name, " size is: ") + dW.toString() + " x " + dH.toString());
    var e = "Document ".concat(doc.name, " has size (").concat(dW, " x ").concat(dH, ").");
    lf.writeln(e);
    try {
        var otherSets = [];
        var otherLayers = [];
        var backOutlineSet = undefined;
        var stickerOutlineSet = undefined;
        var backColorSet = undefined;
        var stickerColorSet = undefined;
        var decorColorSet = undefined;
        var decorOutlineSet = undefined;
        for (var i_1 = 0; i_1 < doc.layerSets.length; ++i_1) {
            var set = doc.layerSets[i_1];
            var setName = set.name.replace(/^\s*/, "").replace(/\s*$/, "");
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
            if (setName === "decor_color") {
                decorColorSet = set;
            }
            else if (setName === "decor_outline") {
                decorOutlineSet = set;
            }
            else {
                otherSets.push(set);
            }
        }
        if (!backColorSet || !stickerOutlineSet || !backColorSet || !stickerColorSet) {
            throw new Error("Missing imporant layers.");
        }
        for (var i_2 = 0; i_2 < doc.layers.length; ++i_2) {
            var layer = doc.layers[i_2];
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
        if (decorOutlineSet !== undefined)
            setLayerSetVisibleState(decorOutlineSet, true);
        if (decorColorSet !== undefined)
            setLayerSetVisibleState(decorColorSet, false);
        var outlineFile = new File(thumbPath.getFullPath());
        var outlineOptions = new PNGSaveOptions();
        doc.saveAs(outlineFile, outlineOptions, true);
        loadHistory(doc);
        lf.writeln("Now exporting completed image...");
        saveHistory(doc);
        setLayerSetVisibleState(backOutlineSet, false);
        setLayerSetVisibleState(stickerOutlineSet, false);
        setLayerSetVisibleState(backColorSet, true);
        setLayerSetVisibleState(stickerColorSet, true);
        if (decorOutlineSet !== undefined)
            setLayerSetVisibleState(decorOutlineSet, false);
        if (decorColorSet !== undefined)
            setLayerSetVisibleState(decorColorSet, true);
        var coloredFile = new File(fullPath.getFullPath());
        var coloredOptions = new PNGSaveOptions();
        doc.saveAs(coloredFile, coloredOptions, true);
        loadHistory(doc);
        lf.writeln("Now exporting stickers...");
        setLayerSetVisibleState(backOutlineSet, false);
        setLayerSetVisibleState(stickerOutlineSet, false);
        setLayerSetVisibleState(backColorSet, false);
        setLayerSetVisibleState(stickerColorSet, true);
        if (decorOutlineSet !== undefined)
            setLayerSetVisibleState(decorOutlineSet, false);
        if (decorColorSet !== undefined)
            setLayerSetVisibleState(decorColorSet, false);
        setLayersOfSetVisibleState(stickerColorSet, false);
        var csvContent = "name,x,y";
        var lastLayer = undefined;
        var i_3 = 0;
        var nameMap = {};
        for (i_3 = 0; i_3 < stickerColorSet.layers.length; ++i_3) {
            // Make visible
            if (lastLayer !== undefined)
                lastLayer.visible = false;
            var layer = stickerColorSet.layers[i_3];
            layer.visible = true;
            var stickerName = layer.name.replace(/^\s*/, "").replace(/\s*$/, "");
            while (nameMap.hasOwnProperty(stickerName)) {
                var conflictedName = stickerName;
                var count = nameMap[conflictedName];
                nameMap[conflictedName] = count + 1;
                stickerName += " (".concat(count + 1, ")");
            }
            nameMap[stickerName] = 1;
            var rect = layer.bounds;
            // Calculation
            var x1 = (rect[0] instanceof UnitValue ? rect[0].value : rect[0]);
            var y1 = (rect[1] instanceof UnitValue ? rect[1].value : rect[1]);
            var x2 = (rect[2] instanceof UnitValue ? rect[2].value : rect[2]);
            var y2 = (rect[3] instanceof UnitValue ? rect[3].value : rect[3]);
            var dm = dW + " " + dH + " " + x1 + " " + x2 + " " + y1 + " " + y2;
            var _a = getNormalizedCoordinates(lf, dW, dH, x1, x2, y1, y2), nX = _a[0], nY = _a[1];
            var line = "".concat(stickerName, ",").concat(nX.toFixed(4), ",").concat(nY.toFixed(4));
            csvContent += "\n" + line;
            // Save layer
            var layerDef = levelStickersPath.clone();
            layerDef.name = stickerName;
            var layerFile = new File(layerDef.getFullPath());
            saveHistory(doc);
            doc.trim();
            var trimW = doc.width instanceof UnitValue ? doc.width.value : doc.width;
            var trimH = doc.height instanceof UnitValue ? doc.height.value : doc.height;
            doc.resizeCanvas(trimW + 16, trimH + 16, AnchorPosition.MIDDLECENTER);
            var options = new PNGSaveOptions();
            doc.saveAs(layerFile, options, true);
            loadHistory(doc);
            lastLayer = layer;
            lf.writeln("Sticker number ".concat(i_3, " (named \"").concat(stickerName, "\", [").concat(dm, "])} exported\")"));
        }
        // Save csv
        lf.writeln("Document ".concat(doc.name, " produced ").concat(i_3, " stickers."));
        var csvFile = new File(csvPath.getFullPath());
        csvFile.open("w", "TEXT", "????");
        csvFile.write(csvContent);
        csvFile.close();
        setLayersOfSetVisibleState(stickerColorSet, true);
        setLayerSetVisibleState(backOutlineSet, true);
        setLayerSetVisibleState(stickerOutlineSet, true);
        setLayerSetVisibleState(backColorSet, true);
        setLayerSetVisibleState(stickerColorSet, true);
        if (decorOutlineSet !== undefined)
            setLayerSetVisibleState(decorOutlineSet, true);
        if (decorColorSet !== undefined)
            setLayerSetVisibleState(decorColorSet, true);
        lf.writeln("Cleaning up...");
        // alert("Completed.");
    }
    catch (e) {
        var msg = "Excecution failed: " + e.toString();
        lf.writeln(msg);
        msg += "\nSee \"log.txt\" for details.";
        alert(e);
    }
    lf.close();
}
function setLayersOfSetVisibleState(set, visible) {
    for (var i_4 = 0; i_4 < set.layers.length; ++i_4) {
        var layer = set.layers[i_4];
        layer.visible = visible;
    }
}
function setLayerSetVisibleState(set, visible) {
    set.visible = visible;
}
function setLayerSetsVisibleState(sets, visible) {
    for (var i_5 = 0; i_5 < sets.length; ++i_5) {
        var set = sets[i_5];
        set.visible = visible;
    }
}
function setLayersVisibleState(layers, visible) {
    for (var i_6 = 0; i_6 < layers.length; ++i_6) {
        var layer = layers[i_6];
        layer.visible = visible;
    }
}
function formatDateForFilename(date) {
    return date.getFullYear()
        + "_" + (date.getMonth() < 10 ? "0" + date.getMonth() : date.getMonth())
        + "_" + (date.getDate() < 10 ? "0" + date.getDate() : date.getDate())
        + "_" + (date.getHours() < 10 ? "0" + date.getHours() : date.getHours())
        + "_" + (date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes())
        + "_" + (date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds());
}
function getNormalizedCoordinates(lf, dW, dH, x1, x2, y1, y2) {
    if (x1 < x2) {
        var temp = x1;
        x1 = x2;
        x2 = temp;
    }
    if (y1 < y2) {
        var temp = y1;
        y1 = y2;
        y2 = temp;
    }
    var cDX = dW / 2;
    var cDY = dH / 2;
    var cX = x1 + (x2 - x1) / 2;
    var cY = y1 + (y2 - y1) / 2;
    var tX = cX - cDX;
    var tY = cDY - cY;
    return [tX / cDX, tY / cDY];
}
var masterHistory;
var history;
function saveHistory(doc) {
    history = doc.activeHistoryState;
}
function saveMasterHistory(doc) {
    masterHistory = doc.activeHistoryState;
}
function loadHistory(doc) {
    if (history) {
        doc.activeHistoryState = history;
    }
}
function loadMasterHistory(doc) {
    if (masterHistory) {
        doc.activeHistoryState = masterHistory;
    }
}
function clearHistoryCaches() {
    masterHistory = undefined;
    history = undefined;
}
function deleteFolder(mFolder) {
    var fileList = mFolder.getFiles();
    for (var i = 0; i < fileList.length; i++) {
        var file = fileList[i];
        if (file instanceof File) {
            var fName = new File(file.fullName);
            fName.remove();
        }
    }
}
for (var i = 0; i < app.documents.length; i++) {
    var doc = app.documents[i];
    app.activeDocument = doc;
    clearHistoryCaches();
    tryBackup(doc);
    exportFiles(doc);
}
alert("Completed.");
