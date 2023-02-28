namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DataReader
    {
        private IEnumerable<ImportedObject> ImportedObjects;

        public List<dynamic> Import(string fileToImport)
        {
            var importedLines = ReadLines(fileToImport);
            ImportedObjects = ImportCleanData(importedLines);
            ImportedObjects = AssignChildren(ImportedObjects.ToList());
            var databaseGroups = GetDatabaseGroups(ImportedObjects).ToList();
            return databaseGroups;
        }

        public void PrintImportedData(List<dynamic> ImportedData, bool printData = true)
        {
            if (printData)
                PrintData(ImportedData);
            Console.ReadLine();
        }

        private void PrintData(IEnumerable<dynamic> databaseGroups)
        {
            foreach (var databaseGroup in databaseGroups)
            {
                Console.WriteLine($"Database '{databaseGroup.Database.Name}' ({databaseGroup.Database.NumberOfChildren} tables)");

                foreach (var tableGroup in databaseGroup.Tables)
                {
                    Console.WriteLine($"\tTable '{tableGroup.Table.Schema}.{tableGroup.Table.Name}' ({tableGroup.Table.NumberOfChildren} columns)");

                    foreach (var column in tableGroup.Columns)
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }
        }

        private IEnumerable<dynamic> GetDatabaseGroups(IEnumerable<dynamic> importedObjects)
        {
            var databaseGroups = importedObjects
                .Where(o => o.Type == "DATABASE")
                .GroupJoin(
                    importedObjects,
                    database => new { database.Type, database.Name },
                    table => new { Type = table.ParentType, Name = table.ParentName },
                    (database, tables) => new
                    {
                        Database = database,
                        Tables = tables
                            .GroupJoin(
                                importedObjects,
                                table => new { Type = table.Type, Name = table.Name },
                                column => new { Type = column.ParentType, Name = column.ParentName },
                                (table, columns) => new
                                {
                                    Table = table,
                                    Columns = columns.ToList()
                                }
                            )
                    }
                );
            return databaseGroups;
        }

        private List<ImportedObject> AssignChildren(List<ImportedObject> ImportedObjects)
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.NumberOfChildren = ImportedObjects
                    .Count(impObj => impObj.ParentType == importedObject.Type && impObj.ParentName == importedObject.Name);
            }

            return ImportedObjects;
        }

        private IEnumerable<ImportedObject> ImportCleanData(List<string> importedLines)
        {
            for (int i = 0; i <= importedLines.Count() - 1; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                var importedObject = new ImportedObject()
                {
                    Type = CleanString(values[0]).ToUpper(),
                    Name = CleanString(values[1].ToUpper()),
                    Schema = CleanString(values[2].ToUpper()),
                    ParentName = CleanString(values[3].ToUpper()),
                    ParentType = CleanString(values[4].ToUpper()),
                    DataType = values[5].ToUpper(),
                    IsNullable = NullableValidation(values)
                };
                yield return importedObject;
            }
        }

        private List<string> ReadLines(string fileToImport)
        {
            var streamReader = new StreamReader(fileToImport);
            var importedLines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                    importedLines.Add(line);
            }

            return importedLines;
        }

        private bool NullableValidation(string[] values)
        {
            bool IsCompletedRow = values.Length == 7 ? true : false;
            if (!IsCompletedRow)
                return false;

            string IsNullable = values[6];
            if (String.IsNullOrEmpty(IsNullable) || IsNullable.ToLower() == "null" || IsNullable == "0")
                return false;
            return true;
        }

        private string CleanString(string str) => str.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
    }

    internal class ImportedObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public double NumberOfChildren { get; set; }
    }
}