function cleanStickersOfDoc(doc) {
    var groups = doc.layerSets;
    var backOutlineSet;
    var stickerOutlineSet;
    var backColorSet;
    var stickerColorSet;
    try {
        backOutlineSet = groups.getByName("back_outline");
        stickerOutlineSet = groups.getByName("stickers_outline");
        backColorSet = groups.getByName("back_colored");
        stickerColorSet = groups.getByName("stickers_color");
    }
    catch (_a) {
        alert("Cannot access one of the following layer sets: back_outline, stickers_outline, back_colored, stickers_color. Please check.");
        return;
    }
    if (stickerColorSet.layers.length !== 50) {
        alert("Warning: Document \"".concat(doc.name, "\" has ").concat(stickerColorSet.layers.length, " layers as stickers. Which is >50 or <50."));
    }
    // Clean up stickers
    for (var i_1 = 0; i_1 < stickerColorSet.layers.length; ++i_1) {
        var layer = stickerColorSet[i_1];
        var name = layer.name;
        layer.name = formatName(name);
    }
    alert("Validation and renaming completed.");
}
function formatName(name) {
    return name;
}
cleanStickersOfDoc(app.activeDocument);
