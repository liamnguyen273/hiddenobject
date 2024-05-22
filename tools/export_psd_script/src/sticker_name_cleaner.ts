function cleanStickersOfDoc(doc: Document): void
{
	let groups = doc.layerSets;
	let backOutlineSet: LayerSet;
	let stickerOutlineSet: LayerSet;
	let backColorSet: LayerSet;
	let stickerColorSet: LayerSet;
	try
	{
		backOutlineSet = groups.getByName("back_outline");
		stickerOutlineSet = groups.getByName("stickers_outline");
		backColorSet = groups.getByName("back_colored");
		stickerColorSet = groups.getByName("stickers_color");
	}
	catch
	{
		alert(`Cannot access one of the following layer sets: back_outline, stickers_outline, back_colored, stickers_color. Please check.`);
		return;
	}

	if (stickerColorSet.layers.length !== 50)
	{
		alert(`Warning: Document "${doc.name}" has ${stickerColorSet.layers.length} layers as stickers. Which is >50 or <50.`);
	}

	// Clean up stickers
	for (let i = 0; i < stickerColorSet.layers.length; ++i)
	{
		let layer = stickerColorSet[i];
		let name = layer.name;
		layer.name = formatName(name);
	}

	alert("Validation and renaming completed.");
}

function formatName(name: string): string
{
	return name;
}

cleanStickersOfDoc(app.activeDocument);