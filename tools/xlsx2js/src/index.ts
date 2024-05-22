import path from "path";
import { IConvertOptions, defaultOptions, getDefaultConvertOptions } from "./IConvertOption";
import { json } from "body-parser";
import e from "express";

const xlsx = require('xlsx');
const fs = require('fs');
const request = require("request");
const readline = require("readline");

const columns = [..."ABCDEFGHIJKLMNOPQRSTUVWXYZ".split(''),
	..."ABCDEFGHIJKLMNOPQRSTUVWXYZ".split('').map(x => "A" + x),
	..."ABCDEFGHIJKLMNOPQRSTUVWXYZ".split('').map(x => "B" + x)];

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

// Ready output folder
let a = fs.readFileSync("./paths.json");
let paths = JSON.parse(a.toString());

console.log(process.cwd());

let inputPath: string = paths["xlsxDefaultPath"];
let askForPath = false;
try
{
	if (!fs.lstatSync(inputPath).isFile())
	{
		console.log(`File not found at path ${inputPath}. Now asking for paths:`);
		askForPath = true;
	};
}
catch
{
	console.log(`File not found at path ${inputPath}. Now asking for paths:`);
	askForPath = true;
}

if (askForPath)
{
	rl.question("Input localization path: ", function (link: string) {
		inputPath = link;
		rl.close();
	});

	rl.on("close", () =>
	{
		perform();
	});
}
else
{
	perform();
}

function perform()
{
	try {
		if (!fs.lstatSync(inputPath).isFile()) {
			console.log("Failed. Terminated. Check if \".xlsx\" is missing.");
			return;
		};
	}
	catch (e)
	{
		console.log(e);
		console.log("Failed. Terminated. Check if \".xlsx\" is missing.");
		return;
	}

	// Read data
	let workbook = xlsx.readFile(inputPath);
	let sheetNameList: string[] = Array.from(workbook.SheetNames);
	let configMap: Map<string, IConvertOptions> = new Map<string, IConvertOptions>();

	// Get defaults
	for (let sheetName of sheetNameList)
	{
		if (sheetName.indexOf("IGNORE") >= 0 || sheetName === "CONVERT_OPTIONS")
		{
			continue;
		}

		configMap.set(sheetName, getDefaultConvertOptions());
	}

	// Get configs
	let index = sheetNameList.indexOf("CONVERT_OPTIONS");

	if (index >= 0)
	{
		console.log("Workbook has sheet CONVERT_OPTIONS. Now reading configurations");
		let sheet = workbook.Sheets["CONVERT_OPTIONS"];
		let sheetRow = 1;
		let sheetColIndex = 2; // Is C
		let optionCol = "A";
		let typeCol = "B";
		while (sheetColIndex < columns.length && sheet[columns[sheetColIndex] + sheetRow] !== undefined)
		{
			let sheetName = sheet[columns[sheetColIndex] + sheetRow].v as string;
			if (configMap.has(sheetName))
			{
				let config = configMap.get(sheetName)!;

				let optionRow = 2;
				while (sheet[optionCol + optionRow] !== undefined
					&& sheet[typeCol + optionRow] !== undefined
					&& sheet[columns[sheetColIndex] + optionRow] !== undefined)
				{
					let fieldName = sheet[optionCol + optionRow].v as string;
					let type = sheet[typeCol + optionRow].v as string;

					if (type !== "ignore")
					{
						let cell = sheet[columns[sheetColIndex] + optionRow];
						if (config.hasOwnProperty(fieldName))
						{
							config[fieldName] = castCellByType(cell.v, type);
						}
						else
						{
							console.log("CONVERT_OPTIONS has option \"" + fieldName + "\" for sheet " + sheetName + " but said options are not valid and will be ignored");
						}
					}
					++optionRow;
				}

				configMap.set(sheetName, config);
			}
			else
			{
				console.log("CONVERT_OPTIONS has options for sheet " + sheetName + " but workbook has no sheets with such name. The options will be ignored");
			}

			++sheetColIndex;
		}

	}

	for (let sheetName of configMap.keys())
	{
		console.log("---------------------------------------\nNow converting sheet: " + sheetName);
		let sheet = workbook.Sheets[sheetName];
		let config = configMap.get(sheetName)!;

		let result = makeJsonFromSheet(sheetName, sheet, config);

		if (result === null)
		{
			console.log("Sheet conversion to JSON failed.");
		}
		else
		{
			console.log("Sheet conversion to JSON succeeded. Now writing to file");

			// Write to file
			let outputFileName = sheetName + ".json";
			let savePath = path.join(paths["outputPath"], outputFileName);
			fs.createWriteStream(savePath);
			fs.writeFile(savePath, result, "utf-8", (err: Error) => {
				if (err)
				{
					console.log(err);
					console.log("Saving JSON " + savePath + " failed.");
				}
				else
				{
					console.log("Wrote file " + savePath);
					console.log("Conversion done.\n--------------------------------------------\n");
				}
			});
		}
	}

	console.log("Completed.");
}

function makeJsonFromSheet(sheetName: string, sheet: any, options: IConvertOptions): string | null
{
	// Validate options
	if (options.indexColumns === undefined) options.indexColumns = defaultOptions.indexColumns;
	if (options.multiIndexSeparator === undefined) options.multiIndexSeparator = defaultOptions.multiIndexSeparator;
	if (options.outputAsDict === undefined) options.outputAsDict = defaultOptions.outputAsDict;
	if (options.includeIndexColumn === undefined) options.includeIndexColumn = defaultOptions.includeIndexColumn;
	if (options.keyRow === undefined) options.keyRow = defaultOptions.keyRow;
	if (options.typeRow === undefined) options.typeRow = defaultOptions.typeRow;
	if (options.dataStartRow === undefined) options.dataStartRow = defaultOptions.dataStartRow;

	// Get fields
	let fieldMap: Map<string, [string, string]> = new Map<string, [string, string]>();
	let i = 0;
	while (i < columns.length)
	{
		let col = columns[i];
		if (sheet[col + options.keyRow!] !== undefined && sheet[col + options.typeRow!] !== undefined)
		{
			let fieldName = sheet[col + options.keyRow!].v as string;
			let type = sheet[col + options.typeRow!].v as string;

			if (type !== "ignore")
			{
				fieldMap.set(col, [fieldName, type]);
			}
		}

		++i;
	}

	// Check if indexColumn header(s) exists
	if (!options.indexColumns!.every(x => fieldMap.has(x)))
	{
		console.log("Sheet " + sheetName + " does not have index columns: " + options.indexColumns + " so it is not converted");
		return null;
	}

	let allFieldCols = Array.from(fieldMap.keys());

	if (!options.includeIndexColumn)
	{
		for (let indexCol of options.indexColumns!)
		{
			let index = allFieldCols.findIndex(x => x === indexCol);

			if (index >= 0)
			{
				allFieldCols.splice(index, 1);
			}
			else
			{
				console.log("Warning: Somehow, fieldMap for " + sheetName + " does not include the index column " + options.includeIndexColumn);
			}
		}
	}

	let allFields = Array.from(fieldMap.values());
	console.log("Sheet " + sheetName + " has the following fields: ", allFields);

	// Get data by rows
	let row = options.dataStartRow!;
	let outputDict: Record<string, Record<string, any>> = {};
	let outputArray: Record<string, any>[] = [];
	while (options.indexColumns!.every(x => sheet[x + row] !== undefined))
	{
		let entryId = options.indexColumns!.map(x => sheet[x + row].v.toString()).join(options.multiIndexSeparator!);
		let obj: Record<string, any> = {};

		for (let col of allFieldCols)
		{
			let cell = sheet[col + row.toString()];
			let [fieldName, type] = fieldMap.get(col)!;
			obj[fieldName] = castCellByType(cell.v, type);
		}

		if (options.outputAsDict)
		{
			outputDict[entryId] = obj;
		}
		else
		{
			outputArray.push(obj);
		}

		++row;
	}

	// Json-ify:
	let result: string;
	if (options.outputAsDict)
	{
		result = JSON.stringify(outputDict, null, "\t");
	}
	else
	{
		result = JSON.stringify(outputArray, null, "\t");
	}

	return result;
}

function castCellByType(cellValue: any, type: string): any
{
	if (type === "number")
	{
		return cellValue as number;
	}
	else if (type === "string")
	{
		return cellValue as string;
	}
	else if (type === "boolean")
	{
		return cellValue as boolean
	}
	else if (type === "array-integer")
	{
		return (cellValue as string).split(",").map(x => Number(x));
	}
	else if (type === "array-string")
	{
		return (cellValue as string).split(",");
	}
	else if (type === "json")
	{
		let data = JSON.parse(cellValue as string);
		return data;
	}
	else
	{
		console.log("Cannot parse data \"" + cellValue + "\" as " + type + " will return null");
		return null;
	}
}