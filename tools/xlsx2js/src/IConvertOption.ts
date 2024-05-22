export interface IConvertOptions
{
	indexColumns?: string[],
	multiIndexSeparator?: string,
	outputAsDict?: boolean,
	includeIndexColumn?: boolean,
	keyRow?: number,
	typeRow?: number,
	dataStartRow?: number,

	[key: string]: any
}

export const defaultOptions = {
	indexColumns: ["A"],
	multiIndexSeparator: "-",
	outputAsDict: false,
	includeIndexColumn: true,
	keyRow: 1,
	typeRow: 3,
	dataStartRow: 4,
} as IConvertOptions;

export function getDefaultConvertOptions(): IConvertOptions
{
	return {...defaultOptions};
}